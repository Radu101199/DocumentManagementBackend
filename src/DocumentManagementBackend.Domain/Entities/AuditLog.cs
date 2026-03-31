namespace DocumentManagementBackend.Domain.Entities;

public class AuditLog
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public string EntityName { get; private set; } = null!;
    public Guid EntityId { get; private set; }
    public string Action { get; private set; } = null!;      // Created, Modified, Deleted
    public string? ChangedBy { get; private set; }
    public DateTime ChangedAt { get; private set; } = DateTime.UtcNow;
    public string? OldValues { get; private set; }           // JSON
    public string? NewValues { get; private set; }           // JSON
    public string? AffectedColumns { get; private set; }     // JSON array

    public static AuditLog Create(
        string entityName,
        Guid entityId,
        string action,
        string? changedBy,
        string? oldValues,
        string? newValues,
        string? affectedColumns) => new()
    {
        EntityName = entityName,
        EntityId = entityId,
        Action = action,
        ChangedBy = changedBy,
        OldValues = oldValues,
        NewValues = newValues,
        AffectedColumns = affectedColumns
    };
}
