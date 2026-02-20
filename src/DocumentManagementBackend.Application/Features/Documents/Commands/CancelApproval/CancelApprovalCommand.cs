using MediatR;

namespace DocumentManagementBackend.Application.Features.Documents.Commands.CancelApproval;

public record CancelApprovalCommand(
    Guid DocumentId,
    Guid CancelledById,
    string? Reason
) : IRequest<Unit>;