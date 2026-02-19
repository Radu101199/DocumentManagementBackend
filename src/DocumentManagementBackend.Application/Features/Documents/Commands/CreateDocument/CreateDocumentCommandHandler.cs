using MediatR;
using DocumentManagementBackend.Domain.Entities;
using DocumentManagementBackend.Domain.Interfaces;

namespace DocumentManagementBackend.Application.Features.Documents.Commands.CreateDocument;

public class CreateDocumentCommandHandler : IRequestHandler<CreateDocumentCommand, Guid>
{
    private readonly IDocumentRepository _documentRepository;

    public CreateDocumentCommandHandler(IDocumentRepository documentRepository)
    {
        _documentRepository = documentRepository;
    }

    public async Task<Guid> Handle(CreateDocumentCommand request, CancellationToken cancellationToken)
    {
        // 1. Create Document using Domain factory method
        var document = Document.Create(
            request.Title,
            request.Description ?? string.Empty,
            request.FileName,
            request.FilePath,
            request.ContentType,
            request.FileSizeBytes,
            request.OwnerId,
            request.CreatorId);

        // 2. Add to repository
        await _documentRepository.AddAsync(document, cancellationToken);

        // 3. Return the document ID
        return document.Id;
    }
}