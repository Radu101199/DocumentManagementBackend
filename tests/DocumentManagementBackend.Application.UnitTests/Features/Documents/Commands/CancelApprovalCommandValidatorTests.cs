using DocumentManagementBackend.Application.Features.Documents.Commands.CancelApproval;
using NUnit.Framework;

namespace DocumentManagementBackend.Application.UnitTests.Features.Documents.Commands;

public class CancelApprovalCommandValidatorTests
{
    private CancelApprovalCommandValidator _validator;

    [SetUp]
    public void Setup()
    {
        _validator = new CancelApprovalCommandValidator();
    }

    [Test]
    public void Should_Have_Error_When_DocumentId_IsEmpty()
    {
        // Arrange
        var command = new CancelApprovalCommand(Guid.Empty, Guid.NewGuid(), null);

        // Act
        var result = _validator.Validate(command);

        // Assert
        Assert.That(result.IsValid, Is.False);
        Assert.That(result.Errors.Any(e => e.PropertyName == "DocumentId"), Is.True);
    }

    [Test]
    public void Should_Have_Error_When_CancelledById_IsEmpty()
    {
        // Arrange
        var command = new CancelApprovalCommand(Guid.NewGuid(), Guid.Empty, null);

        // Act
        var result = _validator.Validate(command);

        // Assert
        Assert.That(result.IsValid, Is.False);
        Assert.That(result.Errors.Any(e => e.PropertyName == "CancelledById"), Is.True);
    }

    [Test]
    public void Should_Have_Error_When_Reason_TooLong()
    {
        // Arrange
        var longReason = new string('x', 1001);
        var command = new CancelApprovalCommand(Guid.NewGuid(), Guid.NewGuid(), longReason);

        // Act
        var result = _validator.Validate(command);

        // Assert
        Assert.That(result.IsValid, Is.False);
        Assert.That(result.Errors.Any(e => e.PropertyName == "Reason"), Is.True);
    }

    [Test]
    public void Should_Not_Have_Error_When_Valid()
    {
        // Arrange
        var command = new CancelApprovalCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Need to upload new version");

        // Act
        var result = _validator.Validate(command);

        // Assert
        Assert.That(result.IsValid, Is.True);
    }

    [Test]
    public void Should_Not_Have_Error_When_Reason_IsNull()
    {
        // Arrange
        var command = new CancelApprovalCommand(Guid.NewGuid(), Guid.NewGuid(), null);

        // Act
        var result = _validator.Validate(command);

        // Assert
        Assert.That(result.IsValid, Is.True);
    }

    [Test]
    public void Should_Not_Have_Error_When_Reason_IsEmpty()
    {
        // Arrange
        var command = new CancelApprovalCommand(Guid.NewGuid(), Guid.NewGuid(), "");

        // Act
        var result = _validator.Validate(command);

        // Assert
        Assert.That(result.IsValid, Is.True);
    }
}