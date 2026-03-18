using DocumentManagementBackend.Application.Common.Exceptions;
using DocumentManagementBackend.Application.Common.Interfaces;
using DocumentManagementBackend.Application.Features.Documents.Queries;
using DocumentManagementBackend.Application.Features.Documents.Queries.GetDocuments;
using MediatR;
using Microsoft.EntityFrameworkCore;
using DocumentManagementBackend.Domain.Entities;


namespace DocumentManagementBackend.Application.Features.Documents.Queries.GetDocumentById;

public class GetDocumentByIdQueryHandler : IRequestHandler<GetDocumentByIdQuery, DocumentDto>
{
    private readonly IApplicationDbContext _context;

    public GetDocumentByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<DocumentDto> Handle(
        GetDocumentByIdQuery request,
        CancellationToken cancellationToken)
    {
        var document = await _context.Documents
            .AsNoTracking()
            .Where(d => d.Id == request.Id)
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
            .FirstOrDefaultAsync(cancellationToken);

        if (document is null)
            throw new NotFoundException(nameof(Document), request.Id);

        return document;
    }
}