using DocumentManagementBackend.Application.Common.Behaviors;
using DocumentManagementBackend.Application.Common.Models;
using DocumentManagementBackend.Domain.Enums;
using MediatR;

namespace DocumentManagementBackend.Application.Features.Documents.Queries.GetDocuments;

public record GetDocumentsQuery(
    int Page = 1,
    int PageSize = 20,
    Guid? OwnerId = null,
    DocumentStatus? Status = null,
    string? SortBy = null
) : IRequest<PagedResult<DocumentDto>>, ICacheableQuery
{
    public string CacheKey => $"documents:page={Page}:size={PageSize}:owner={OwnerId}:status={Status}:sort={SortBy}";
    public TimeSpan CacheDuration => TimeSpan.FromMinutes(2);
}
