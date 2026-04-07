using MediatR;

namespace DocumentManagementBackend.Application.Features.Documents.Queries.GetDocumentVersions;

public record GetDocumentVersionsQuery(Guid DocumentId) : IRequest<List<DocumentVersionDto>>;

public record DocumentVersionDto(
    Guid Id,
    int VersionNumber,
    string Title,
    string? Description,
    string Status,
    string? Comment,
    string? CreatedBy,
    DateTime CreatedAt);
