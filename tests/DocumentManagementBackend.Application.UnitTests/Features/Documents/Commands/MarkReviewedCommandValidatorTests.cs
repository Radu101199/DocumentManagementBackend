using DocumentManagementBackend.Application.Features.Documents.Commands.MarkReviewed;
using NUnit.Framework;

namespace DocumentManagementBackend.Application.UnitTests.Features.Documents.Commands;

public class MarkReviewedCommandValidatorTests
{
    private MarkReviewedCommandValidator _validator;

    [SetUp]
    public void Setup()
    {
        _validator = new MarkReviewedCommandValidator();
    }

    [Test]
    public void Should_Have_Error_When_DocumentId_IsEmpty()
    {
        // Arrange
        var command = new MarkReviewedCommand(
            DocumentId: Guid.Empty,
            ReviewerId: Guid.NewGuid(),
            Notes: null);

        // Act
        var result = _validator.Validate(command);

        // Assert
        Assert.That(result.IsValid, Is.False);
        Assert.That(result.Errors.Any(e => e.PropertyName == "DocumentId"), Is.True);
    }

    [Test]
    public void Should_Have_Error_When_ReviewerId_IsEmpty()
    {
        // Arrange
        var command = new MarkReviewedCommand(
            DocumentId: Guid.NewGuid(),
            ReviewerId: Guid.Empty,
            Notes: null);

        // Act
        var result = _validator.Validate(command);

        // Assert
        Assert.That(result.IsValid, Is.False);
        Assert.That(result.Errors.Any(e => e.PropertyName == "ReviewerId"), Is.True);
    }

    [Test]
    public void Should_Have_Error_When_Notes_TooLong()
    {
        // Arrange
        var longNotes = new string('a', 1001);
        var command = new MarkReviewedCommand(
            DocumentId: Guid.NewGuid(),
            ReviewerId: Guid.NewGuid(),
            Notes: longNotes);

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
        var command = new MarkReviewedCommand(
            DocumentId: Guid.NewGuid(),
            ReviewerId: Guid.NewGuid(),
            Notes: "This looks good");

        // Act
        var result = _validator.Validate(command);

        // Assert
        Assert.That(result.IsValid, Is.True);
    }

    [Test]
    public void Should_Not_Have_Error_When_Notes_IsNull()
    {
        // Arrange
        var command = new MarkReviewedCommand(
            DocumentId: Guid.NewGuid(),
            ReviewerId: Guid.NewGuid(),
            Notes: null);

        // Act
        var result = _validator.Validate(command);

        // Assert
        Assert.That(result.IsValid, Is.True);
    }
}