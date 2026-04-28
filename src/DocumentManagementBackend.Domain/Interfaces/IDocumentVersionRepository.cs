using DocumentManagementBackend.Domain.Entities;

namespace DocumentManagementBackend.Domain.Interfaces;

public interface IDocumentVersionRepository
{
    Task<List<DocumentVersion>> GetByDocumentIdAsync(Guid documentId, CancellationToken cancellationToken = default);
    Task AddAsync(DocumentVersion version, CancellationToken cancellationToken = default);
}
