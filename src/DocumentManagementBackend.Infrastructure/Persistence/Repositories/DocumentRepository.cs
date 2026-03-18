using DocumentManagementBackend.Application.Common.Exceptions;
using DocumentManagementBackend.Application.Common.Interfaces;
using DocumentManagementBackend.Domain.Entities;
using DocumentManagementBackend.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DocumentManagementBackend.Infrastructure.Persistence.Repositories;

public class DocumentRepository : IDocumentRepository
{
    private readonly ApplicationDbContext _context;
    private readonly IDomainEventDispatcher _dispatcher;

    public DocumentRepository(ApplicationDbContext context, IDomainEventDispatcher dispatcher)
    {
        _context = context;
        _dispatcher = dispatcher;
    }

    public async Task<Document?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        // ✅ Fără AsNoTracking — EF trebuie să trackeze RowVersion pentru concurrency
        return await _context.Documents
            .Include(d => d.Owner)
            .FirstOrDefaultAsync(d => d.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<Document>> GetByOwnerIdAsync(Guid ownerId, CancellationToken cancellationToken = default)
    {
        return await _context.Documents
            .Where(d => d.OwnerId == ownerId)
            .OrderByDescending(d => d.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Document document, CancellationToken cancellationToken = default)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            await _context.Documents.AddAsync(document, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            if (document.DomainEvents.Any())
            {
                await _dispatcher.DispatchAsync(document.DomainEvents, cancellationToken);
                document.ClearDomainEvents();
            }

            await transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task UpdateAsync(Document document, CancellationToken cancellationToken = default)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            _context.Documents.Update(document);

            try
            {
                await _context.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                await transaction.RollbackAsync(cancellationToken);

                // ✅ Verifică dacă documentul a fost șters sau doar modificat
                var entry = ex.Entries.Single();
                var databaseValues = await entry.GetDatabaseValuesAsync(cancellationToken);

                throw new ConcurrencyException(
                    databaseValues == null
                        ? $"Document {document.Id} was deleted by another user."
                        : $"Document {document.Id} was modified by another user. Please refresh and try again.",
                    ex);
            }

            if (document.DomainEvents.Any())
            {
                await _dispatcher.DispatchAsync(document.DomainEvents, cancellationToken);
                document.ClearDomainEvents();
            }

            await transaction.CommitAsync(cancellationToken);
        }
        catch (ConcurrencyException)
        {
            throw;
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            var document = await GetByIdAsync(id, cancellationToken);
            if (document == null)
            {
                await transaction.RollbackAsync(cancellationToken);
                return;
            }

            _context.Documents.Remove(document);

            try
            {
                await _context.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                await transaction.RollbackAsync(cancellationToken);
                throw new ConcurrencyException(
                    $"Document {id} was already modified or deleted by another user.", ex);
            }

            await transaction.CommitAsync(cancellationToken);
        }
        catch (ConcurrencyException)
        {
            throw;
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }
}