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
) : IRequest<PagedResult<DocumentDto>>;