using DocumentManagementBackend.Application.Features.Documents.Commands.RejectDocument;
using NUnit.Framework;

namespace DocumentManagementBackend.Application.UnitTests.Features.Documents.Commands;

public class RejectDocumentCommandValidatorTests
{
    private RejectDocumentCommandValidator _validator;
    [SetUp]
    public void Setup()
    {
        _validator = new RejectDocumentCommandValidator();
    }

    [Test]
    public void Should_Have_Error_When_DocumentId_IsEmpty()
    {
        // Arrange
        var command = new RejectDocumentCommand(Guid.Empty, Guid.NewGuid(), null);
        // Act
        var result = _validator.Validate(command);
        // Assert
        Assert.That(result.IsValid, Is.False);
        Assert.That(result.Errors.Any(e => e.PropertyName == "DocumentId"), Is.True);
    }

    [Test]
    public void Should_Have_Error_When_RejectorId_IsEmpty()
    {
        // Arrange
        var command = new RejectDocumentCommand(Guid.NewGuid(), Guid.Empty, null);
        // Act
        var result = _validator.Validate(command);
        // Assert
        Assert.That(result.IsValid, Is.False);
        Assert.That(result.Errors.Any(e => e.PropertyName == "RejectorId"), Is.True);
    }

    [Test]
    public void Should_Have_Error_When_RejectionReason_TooLong()
    {
        // Arrange
        var longReason = new string('x', 1001);
        var command = new RejectDocumentCommand(Guid.NewGuid(), Guid.NewGuid(), longReason);
        // Act
        var result = _validator.Validate(command);
        // Assert
        Assert.That(result.IsValid, Is.False);
        Assert.That(result.Errors.Any(e => e.PropertyName == "RejectionReason"), Is.True);
    }

    [Test]
    public void Should_Not_Have_Error_When_Valid()
    {
        // Arrange
        var command = new RejectDocumentCommand(Guid.NewGuid(), Guid.NewGuid(), "Needs more work.");
        // Act
        var result = _validator.Validate(command);
        // Assert
        Assert.That(result.IsValid, Is.True);
    }

    [Test]
    public void Should_Not_Have_Error_When_RejectionReason_IsNull()
    {
        // Arrange
        var command = new RejectDocumentCommand(Guid.NewGuid(), Guid.NewGuid(), null);

        // Act
        var result = _validator.Validate(command);

        // Assert
        Assert.That(result.IsValid, Is.True);
    }
}

