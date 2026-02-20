using FluentValidation;

namespace DocumentManagementBackend.Application.Features.Documents.Commands.CancelApproval;

public class CancelApprovalCommandValidator : AbstractValidator<CancelApprovalCommand>
{
    public CancelApprovalCommandValidator()
    {
        RuleFor(x => x.DocumentId)
            .NotEmpty().WithMessage("Document ID is required");

        RuleFor(x => x.CancelledById)
            .NotEmpty().WithMessage("User ID is required");

        RuleFor(x => x.Reason)
            .MaximumLength(1000).WithMessage("Reason must not exceed 1000 characters");
    }
}