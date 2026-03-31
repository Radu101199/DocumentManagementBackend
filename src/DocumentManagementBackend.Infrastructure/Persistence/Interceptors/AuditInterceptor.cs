using System.Text.Json;
using DocumentManagementBackend.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace DocumentManagementBackend.Infrastructure.Persistence.Interceptors;

public class AuditInterceptor : SaveChangesInterceptor
{
    private readonly List<AuditLog> _auditLogs = new();

    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        CaptureAuditLogs(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        CaptureAuditLogs(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    public override int SavedChanges(SaveChangesCompletedEventData eventData, int result)
    {
        PersistAuditLogs(eventData.Context);
        return base.SavedChanges(eventData, result);
    }

    public override async ValueTask<int> SavedChangesAsync(
        SaveChangesCompletedEventData eventData,
        int result,
        CancellationToken cancellationToken = default)
    {
        await PersistAuditLogsAsync(eventData.Context, cancellationToken);
        return await base.SavedChangesAsync(eventData, result, cancellationToken);
    }

    private void CaptureAuditLogs(DbContext? context)
    {
        if (context == null) return;
        _auditLogs.Clear();

        foreach (var entry in context.ChangeTracker.Entries<BaseAuditableEntity>())
        {
            if (entry.State is not (EntityState.Added or EntityState.Modified or EntityState.Deleted))
                continue;

            var entityId = entry.Properties
                .FirstOrDefault(p => p.Metadata.IsPrimaryKey())?.CurrentValue;

            if (entityId is not Guid id) continue;

            var action = entry.State switch
            {
                EntityState.Added => "Created",
                EntityState.Modified => "Modified",
                EntityState.Deleted => "Deleted",
                _ => "Unknown"
            };

            // Detectează soft delete
            if (entry.State == EntityState.Modified &&
                entry.Property(nameof(BaseAuditableEntity.IsDeleted)).IsModified &&
                entry.Entity.IsDeleted)
                action = "SoftDeleted";

            string? oldValues = null;
            string? newValues = null;
            string? affectedColumns = null;

            if (entry.State == EntityState.Modified)
            {
                var changed = entry.Properties
                    .Where(p => p.IsModified)
                    .ToList();

                oldValues = JsonSerializer.Serialize(
                    changed.ToDictionary(p => p.Metadata.Name, p => p.OriginalValue));
                newValues = JsonSerializer.Serialize(
                    changed.ToDictionary(p => p.Metadata.Name, p => p.CurrentValue));
                affectedColumns = JsonSerializer.Serialize(
                    changed.Select(p => p.Metadata.Name));
            }
            else if (entry.State == EntityState.Added)
            {
                newValues = JsonSerializer.Serialize(
                    entry.Properties.ToDictionary(p => p.Metadata.Name, p => p.CurrentValue));
            }

            _auditLogs.Add(AuditLog.Create(
                entityName: entry.Entity.GetType().Name,
                entityId: id,
                action: action,
                changedBy: entry.State == EntityState.Added
                    ? entry.Entity.CreatedBy
                    : entry.Entity.UpdatedBy,
                oldValues: oldValues,
                newValues: newValues,
                affectedColumns: affectedColumns));
        }
    }

    private void PersistAuditLogs(DbContext? context)
    {
        if (context == null || _auditLogs.Count == 0) return;
        context.Set<AuditLog>().AddRange(_auditLogs);
        context.SaveChanges();
        _auditLogs.Clear();
    }

    private async Task PersistAuditLogsAsync(DbContext? context, CancellationToken ct)
    {
        if (context == null || _auditLogs.Count == 0) return;
        context.Set<AuditLog>().AddRange(_auditLogs);
        await context.SaveChangesAsync(ct);
        _auditLogs.Clear();
    }
}
