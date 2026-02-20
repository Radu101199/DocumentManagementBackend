using MediatR;
using DocumentManagementBackend.Application.Common.Exceptions;
using DocumentManagementBackend.Domain.Interfaces;

namespace DocumentManagementBackend.Application.Features.Documents.Commands.RejectDocument;

public class RejectDocumentCommandHandler : IRequestHandler<RejectDocumentCommand, Unit>
{
    private readonly IDocumentRepository _documentRepository;
    public RejectDocumentCommandHandler(IDocumentRepository documentRepository)
    {
        _documentRepository = documentRepository;
    }

    public async Task<Unit> Handle(RejectDocumentCommand request, CancellationToken cancellationToken)
    {
        // 1. Get document from repository
        var document = await _documentRepository.GetByIdAsync(request.DocumentId, cancellationToken);
        if (document is null)
        {
            throw new NotFoundException(nameof(document), request.DocumentId);
        }
        // 2. Call domain method - this will validate business rules and throw if invalid
        document.Reject(request.RejectorId, request.RejectionReason);
        // 3. Update repository
        await _documentRepository.UpdateAsync(document, cancellationToken);
        
        return Unit.Value;
    }

}

