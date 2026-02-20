using MediatR;

namespace DocumentManagementBackend.Application.Features.Documents.Commands.RejectDocument;

    public record RejectDocumentCommand(
        Guid DocumentId,
        Guid RejectorId,
        string? RejectionReason
    ) : IRequest<Unit>;
