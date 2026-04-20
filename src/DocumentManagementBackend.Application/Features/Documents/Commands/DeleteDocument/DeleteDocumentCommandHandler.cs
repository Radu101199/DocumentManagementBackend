using DocumentManagementBackend.Application.Common.Exceptions;
using DocumentManagementBackend.Application.Common.Interfaces;
using DocumentManagementBackend.Domain.Interfaces;
using MediatR;
using DocumentManagementBackend.Domain.Entities;


namespace DocumentManagementBackend.Application.Features.Documents.Commands.DeleteDocument;

public class DeleteDocumentCommandHandler : IRequestHandler<DeleteDocumentCommand,Unit>
{
    private readonly IDocumentRepository _documentRepository;

    public DeleteDocumentCommandHandler(IDocumentRepository documentRepository)
    {
        _documentRepository = documentRepository;
    }

    public async Task<Unit> Handle(DeleteDocumentCommand request, CancellationToken cancellationToken)
    {
        var document = await _documentRepository.GetByIdAsync(request.DocumentId, cancellationToken)
                       ?? throw new NotFoundException(nameof(Document), request.DocumentId);

        document.SoftDelete(request.DeletedBy);
        
        await _documentRepository.UpdateAsync(document, cancellationToken);
        
        return Unit.Value;
    }
}