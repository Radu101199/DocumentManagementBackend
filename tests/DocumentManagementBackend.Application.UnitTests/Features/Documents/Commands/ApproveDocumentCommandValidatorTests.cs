using DocumentManagementBackend.Application.Features.Documents.Commands.ApproveDocument;
using NUnit.Framework;

namespace DocumentManagementBackend.Application.UnitTests.Features.Documents.Commands;

public class ApproveDocumentCommandValidatorTests
{
    private ApproveDocumentCommandValidator _validator;

    [SetUp]
    public void Setup()
    {
        _validator = new ApproveDocumentCommandValidator();
    }

    [Test]
    public void Should_Have_Error_When_DocumentId_IsEmpty()
    {
        // Arrange
        var command = new ApproveDocumentCommand(Guid.Empty, Guid.NewGuid(), null);

        // Act
        var result = _validator.Validate(command);

        // Assert
        Assert.That(result.IsValid, Is.False);
        Assert.That(result.Errors.Any(e => e.PropertyName == "DocumentId"), Is.True);
    }

    [Test]
    public void Should_Have_Error_When_ApproverId_IsEmpty()
    {
        // Arrange
        var command = new ApproveDocumentCommand(Guid.NewGuid(), Guid.Empty, null);

        // Act
        var result = _validator.Validate(command);

        // Assert
        Assert.That(result.IsValid, Is.False);
        Assert.That(result.Errors.Any(e => e.PropertyName == "ApproverId"), Is.True);
    }

    [Test]
    public void Should_Have_Error_When_Notes_TooLong()
    {
        // Arrange
        var longNotes = new string('x', 1001);
        var command = new ApproveDocumentCommand(Guid.NewGuid(), Guid.NewGuid(), longNotes);

        // Act
        var result = _validator.Validate(command);

        // Assert
        Assert.That(result.IsValid, Is.False);
        Assert.That(result.Errors.Any(e => e.PropertyName == "Notes"), Is.True);
    }

    [Test]
    public void Should_Not_Have_Error_When_Valid()
    {
        // Arrange
        var command = new ApproveDocumentCommand(
            Guid.NewGuid(), 
            Guid.NewGuid(), 
            "Approved");

        // Act
        var result = _validator.Validate(command);

        // Assert
        Assert.That(result.IsValid, Is.True);
    }

    [Test]
    public void Should_Not_Have_Error_When_Notes_IsNull()
    {
        // Arrange
        var command = new ApproveDocumentCommand(Guid.NewGuid(), Guid.NewGuid(), null);

        // Act
        var result = _validator.Validate(command);

        // Assert
        Assert.That(result.IsValid, Is.True);
    }
}