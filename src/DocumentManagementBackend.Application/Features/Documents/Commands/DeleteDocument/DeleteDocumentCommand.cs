using MediatR;

namespace DocumentManagementBackend.Application.Features.Documents.Commands.DeleteDocument;

public record DeleteDocumentCommand(
    Guid DocumentId, 
    string? DeletedBy
) : IRequest<Unit>;