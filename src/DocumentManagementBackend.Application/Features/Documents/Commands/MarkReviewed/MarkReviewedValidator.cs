using FluentValidation;

namespace DocumentManagementBackend.Application.Features.Documents.Commands.MarkReviewed;

public class MarkReviewedCommandValidator : AbstractValidator<MarkReviewedCommand>
{
    public MarkReviewedCommandValidator()
    {
        RuleFor(x => x.DocumentId)
            .NotEmpty().WithMessage("Document ID is required");

        RuleFor(x => x.ReviewerId)
            .NotEmpty().WithMessage("Reviewer ID is required");

        RuleFor(x => x.Notes)
            .MaximumLength(1000).WithMessage("Notes must not exceed 1000 characters");
    }
}