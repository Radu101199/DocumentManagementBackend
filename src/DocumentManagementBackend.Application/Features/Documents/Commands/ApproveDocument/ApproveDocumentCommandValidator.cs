using FluentValidation;

namespace DocumentManagementBackend.Application.Features.Documents.Commands.ApproveDocument;

public class ApproveDocumentCommandValidator : AbstractValidator<ApproveDocumentCommand>
{
    public ApproveDocumentCommandValidator()
    {
        RuleFor(x => x.DocumentId)
            .NotEmpty().WithMessage("Document ID is required");

        RuleFor(x => x.ApproverId)
            .NotEmpty().WithMessage("Approver ID is required");

        RuleFor(x => x.Notes)
            .MaximumLength(1000).WithMessage("Notes must not exceed 1000 characters");
    }
}