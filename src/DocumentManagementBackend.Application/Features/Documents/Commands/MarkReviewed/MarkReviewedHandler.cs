using MediatR;
using DocumentManagementBackend.Application.Common.Exceptions;
using DocumentManagementBackend.Domain.Interfaces;

namespace DocumentManagementBackend.Application.Features.Documents.Commands.MarkReviewed;

public class MarkReviewedCommandHandler : IRequestHandler<MarkReviewedCommand, Unit>
{
    private readonly IDocumentRepository _documentRepository;

    public MarkReviewedCommandHandler(IDocumentRepository documentRepository)
    {
        _documentRepository = documentRepository;
    }

    public async Task<Unit> Handle(MarkReviewedCommand request, CancellationToken cancellationToken)
    {
        // 1. Get document from repository
        var document = await _documentRepository.GetByIdAsync(request.DocumentId, cancellationToken);

        if (document == null)
            throw new NotFoundException(nameof(Domain.Entities.Document), request.DocumentId);

        // 2. Call domain method (business logic is in Domain)
        document.MarkAsReviewed(request.ReviewerId, request.Notes);

        // 3. Update repository
        await _documentRepository.UpdateAsync(document, cancellationToken);

        return Unit.Value;
    }
}