using DocumentManagementBackend.Domain.Interfaces;
using MediatR;

namespace DocumentManagementBackend.Application.Features.Documents.Queries.GetDocumentVersions;

public class GetDocumentVersionsQueryHandler 
    : IRequestHandler<GetDocumentVersionsQuery, List<DocumentVersionDto>>
{
    private readonly IDocumentVersionRepository _repository;

    public GetDocumentVersionsQueryHandler(IDocumentVersionRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<DocumentVersionDto>> Handle(
        GetDocumentVersionsQuery request, 
        CancellationToken cancellationToken)
    {
        var versions = await _repository.GetByDocumentIdAsync(request.DocumentId, cancellationToken);
        
        return versions.Select(v => new DocumentVersionDto(
            v.Id,
            v.VersionNumber,
            v.Title,
            v.Description,
            v.Status.ToString(),
            v.Comment,
            v.CreatedBy,
            v.CreatedAt
        )).ToList();
    }
}
