using MediatR;

namespace DocumentManagementBackend.Application.Features.Documents.Commands.MarkReviewed;

public record MarkReviewedCommand(
	Guid DocumentId,
	Guid ReviewerId,
	string? Notes
	) : IRequest<Unit>;