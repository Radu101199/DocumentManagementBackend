using MediatR;
using DocumentManagementBackend.Application.Common.Exceptions;
using DocumentManagementBackend.Domain.Interfaces;

namespace DocumentManagementBackend.Application.Features.Documents.Commands.CancelApproval;

public class CancelApprovalCommandHandler : IRequestHandler<CancelApprovalCommand, Unit>
{
    private readonly IDocumentRepository _documentRepository;

    public CancelApprovalCommandHandler(IDocumentRepository documentRepository)
    {
        _documentRepository = documentRepository;
    }

    public async Task<Unit> Handle(CancelApprovalCommand request, CancellationToken cancellationToken)
    {
        // 1. Get document from repository
        var document = await _documentRepository.GetByIdAsync(request.DocumentId, cancellationToken);

        if (document is null)
        {
            throw new NotFoundException(nameof(document), request.DocumentId);
        }

        // 2. Call domain method - this will validate business rules
        document.CancelApproval(request.CancelledById, request.Reason);

        // 3. Update repository
        await _documentRepository.UpdateAsync(document, cancellationToken);

        return Unit.Value;
    }
}