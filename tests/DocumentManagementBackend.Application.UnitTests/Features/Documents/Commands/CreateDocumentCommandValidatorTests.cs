using DocumentManagementBackend.Application.Features.Documents.Commands.CreateDocument;
using NUnit.Framework;

namespace DocumentManagementBackend.Application.UnitTests.Features.Documents.Commands;

public class CreateDocumentCommandValidatorTests
{
    private CreateDocumentCommandValidator _validator;

    [SetUp]
    public void Setup()
    {
        _validator = new CreateDocumentCommandValidator();
    }

    [Test]
    public void Should_Have_Error_When_Title_IsEmpty()
    {
        // Arrange
        var command = new CreateDocumentCommand(
            Title: "",
            Description: null,
            FileName: "test.pdf",
            FilePath: "/files/test.pdf",
            ContentType: "application/pdf",
            FileSizeBytes: 1024,
            OwnerId: Guid.NewGuid(),
            CreatorId: Guid.NewGuid());

        // Act
        var result = _validator.Validate(command);

        // Assert
        Assert.That(result.IsValid, Is.False);
        Assert.That(result.Errors.Any(e => e.PropertyName == "Title"), Is.True);
    }

    [Test]
    public void Should_Have_Error_When_FileName_IsEmpty()
    {
        // Arrange
        var command = new CreateDocumentCommand(
            Title: "Test",
            Description: null,
            FileName: "",
            FilePath: "/files/test.pdf",
            ContentType: "application/pdf",
            FileSizeBytes: 1024,
            OwnerId: Guid.NewGuid(),
            CreatorId: Guid.NewGuid());

        // Act
        var result = _validator.Validate(command);

        // Assert
        Assert.That(result.IsValid, Is.False);
        Assert.That(result.Errors.Any(e => e.PropertyName == "FileName"), Is.True);
    }

    [Test]
    public void Should_Have_Error_When_FileSizeBytes_IsZero()
    {
        // Arrange
        var command = new CreateDocumentCommand(
            Title: "Test",
            Description: null,
            FileName: "test.pdf",
            FilePath: "/files/test.pdf",
            ContentType: "application/pdf",
            FileSizeBytes: 0,
            OwnerId: Guid.NewGuid(),
            CreatorId: Guid.NewGuid());

        // Act
        var result = _validator.Validate(command);

        // Assert
        Assert.That(result.IsValid, Is.False);
        Assert.That(result.Errors.Any(e => e.PropertyName == "FileSizeBytes"), Is.True);
    }

    [Test]
    public void Should_Not_Have_Error_When_Valid()
    {
        // Arrange
        var command = new CreateDocumentCommand(
            Title: "Test Document",
            Description: "Description",
            FileName: "test.pdf",
            FilePath: "/files/test.pdf",
            ContentType: "application/pdf",
            FileSizeBytes: 1024,
            OwnerId: Guid.NewGuid(),
            CreatorId: Guid.NewGuid());

        // Act
        var result = _validator.Validate(command);

        // Assert
        Assert.That(result.IsValid, Is.True);
    }
}