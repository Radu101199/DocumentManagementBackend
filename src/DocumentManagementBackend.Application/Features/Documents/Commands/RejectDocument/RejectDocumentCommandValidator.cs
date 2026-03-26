using FluentValidation;

namespace DocumentManagementBackend.Application.Features.Documents.Commands.RejectDocument;

public class RejectDocumentCommandValidator : AbstractValidator<RejectDocumentCommand>
{
    public RejectDocumentCommandValidator()
    {
        RuleFor(x => x.DocumentId)
            .NotEmpty().WithMessage("Document ID is required");
        RuleFor(x => x.RejectorId)
            .NotEmpty().WithMessage("Rejector ID is required");
        RuleFor(x => x.RejectionReason)
            .MaximumLength(1000).WithMessage("Notes must not exceed 1000 characters");
    }
}