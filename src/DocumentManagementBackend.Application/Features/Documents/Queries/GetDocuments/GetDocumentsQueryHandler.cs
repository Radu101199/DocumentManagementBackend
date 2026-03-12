using DocumentManagementBackend.Application.Common.Interfaces;
using DocumentManagementBackend.Application.Common.Models;
using DocumentManagementBackend.Application.Features.Documents.Queries;
using DocumentManagementBackend.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DocumentManagementBackend.Application.Features.Documents.Queries.GetDocuments;

public class GetDocumentsQueryHandler : IRequestHandler<GetDocumentsQuery, PagedResult<DocumentDto>>
{
    private readonly IApplicationDbContext _context;

    public GetDocumentsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<DocumentDto>> Handle(
        GetDocumentsQuery request,
        CancellationToken cancellationToken)
    {
        var page = Math.Max(1, request.Page);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);

        var query = _context.Documents
            .Include(d => d.Owner)
            .AsNoTracking()
            .AsQueryable();

        if (request.OwnerId.HasValue)
            query = query.Where(d => d.OwnerId == request.OwnerId.Value);

        if (request.Status.HasValue)
            query = query.Where(d => d.Status == request.Status.Value);

        query = request.SortBy switch
        {
            "title_asc"      => query.OrderBy(d => d.Title),
            "title_desc"     => query.OrderByDescending(d => d.Title),
            "createdAt_asc"  => query.OrderBy(d => d.CreatedAt),
            "createdAt_desc" => query.OrderByDescending(d => d.CreatedAt),
            "status_asc"     => query.OrderBy(d => d.Status),
            "status_desc"    => query.OrderByDescending(d => d.Status),
            _                => query.OrderByDescending(d => d.CreatedAt)
        };

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(d => new DocumentDto(
                d.Id,
                d.Title,
                d.Description,
                d.FileName,
                d.ContentType,
                d.FileSizeBytes,
                d.Status,
                d.OwnerId,
                d.Owner.FirstName + " " + d.Owner.LastName,
                d.CreatedAt,
                d.UpdatedAt,
                d.ApprovalRequestedAt,
                d.ApprovalExpiresAt))
            .ToListAsync(cancellationToken);

        return new PagedResult<DocumentDto>(items, page, pageSize, totalCount);
    }
}