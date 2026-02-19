using FluentValidation;

namespace DocumentManagementBackend.Application.Features.Documents.Commands.CreateDocument;

public class CreateDocumentCommandValidator : AbstractValidator<CreateDocumentCommand>
{
    public CreateDocumentCommandValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required")
            .MaximumLength(200).WithMessage("Title must not exceed 200 characters");

        RuleFor(x => x.FileName)
            .NotEmpty().WithMessage("File name is required")
            .MaximumLength(255).WithMessage("File name must not exceed 255 characters");

        RuleFor(x => x.FilePath)
            .NotEmpty().WithMessage("File path is required");

        RuleFor(x => x.ContentType)
            .NotEmpty().WithMessage("Content type is required");

        RuleFor(x => x.FileSizeBytes)
            .GreaterThan(0).WithMessage("File size must be greater than zero");

        RuleFor(x => x.OwnerId)
            .NotEmpty().WithMessage("Owner ID is required");

        RuleFor(x => x.CreatorId)
            .NotEmpty().WithMessage("Creator ID is required");
    }
}