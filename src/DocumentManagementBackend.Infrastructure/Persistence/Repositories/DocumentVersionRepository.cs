using DocumentManagementBackend.Domain.Entities;
using DocumentManagementBackend.Domain.Interfaces;
using DocumentManagementBackend.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DocumentManagementBackend.Infrastructure.Persistence.Repositories;

public class DocumentVersionRepository : IDocumentVersionRepository
{
    private readonly ApplicationDbContext _context;

    public DocumentVersionRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<DocumentVersion>> GetByDocumentIdAsync(
        Guid documentId, 
        CancellationToken cancellationToken = default)
    {
        return await _context.Set<DocumentVersion>()
            .Where(v => v.DocumentId == documentId)
            .OrderByDescending(v => v.VersionNumber)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(DocumentVersion version, CancellationToken cancellationToken = default)
    {
        await _context.Set<DocumentVersion>().AddAsync(version, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
