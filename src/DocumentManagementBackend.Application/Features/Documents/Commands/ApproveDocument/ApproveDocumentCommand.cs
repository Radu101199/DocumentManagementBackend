using MediatR;

namespace DocumentManagementBackend.Application.Features.Documents.Commands.ApproveDocument;

public record ApproveDocumentCommand(
    Guid DocumentId,
    Guid ApproverId,
    string? Notes
) : IRequest<Unit>;