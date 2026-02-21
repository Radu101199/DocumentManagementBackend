using DocumentManagementBackend.Application.Features.Documents.Commands.CreateDocument;
using DocumentManagementBackend.Domain.Entities;
using DocumentManagementBackend.Domain.Interfaces;
using Moq;
using NUnit.Framework;
using Document = DocumentManagementBackend.Domain.Entities.Document;

namespace DocumentManagementBackend.Application.UnitTests.Features.Documents.Commands;

public class CreateDocumentCommandHandlerTests
{
    private Mock<IDocumentRepository> _mockRepository;
    private CreateDocumentCommandHandler _handler;

    [SetUp]
    public void Setup()
    {
        _mockRepository = new Mock<IDocumentRepository>();
        _handler = new CreateDocumentCommandHandler(_mockRepository.Object);
    }

    [Test]
    public async Task Should_Create_Document_And_Add_To_Repository()
    {
        // Arrange
        var command = new CreateDocumentCommand(
            Title: "Test Document",
            Description: "Test Description",
            FileName: "test.pdf",
            FilePath: "/files/test.pdf",
            ContentType: "application/pdf",
            FileSizeBytes: 1024,
            OwnerId: Guid.NewGuid(),
            CreatorId: Guid.NewGuid());

        Document? capturedDocument = null;
        _mockRepository
            .Setup(x => x.AddAsync(It.IsAny<Document>(), It.IsAny<CancellationToken>()))
            .Callback<Document, CancellationToken>((doc, _) => capturedDocument = doc)
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.That(result, Is.Not.EqualTo(Guid.Empty));
        Assert.That(capturedDocument, Is.Not.Null);
        Assert.That(capturedDocument!.Title, Is.EqualTo("Test Document"));
        Assert.That(capturedDocument.FileName, Is.EqualTo("test.pdf"));
        Assert.That(capturedDocument.OwnerId, Is.EqualTo(command.OwnerId));
        
        _mockRepository.Verify(
            x => x.AddAsync(It.IsAny<Document>(), It.IsAny<CancellationToken>()), 
            Times.Once);
    }

    [Test]
    public async Task Should_Return_Document_Id()
    {
        // Arrange
        var command = new CreateDocumentCommand(
            Title: "Test Document",
            Description: null,
            FileName: "test.pdf",
            FilePath: "/files/test.pdf",
            ContentType: "application/pdf",
            FileSizeBytes: 1024,
            OwnerId: Guid.NewGuid(),
            CreatorId: Guid.NewGuid());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.That(result, Is.Not.EqualTo(Guid.Empty));
    }

    [Test]
    public async Task Should_Use_Domain_Factory_Method()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var creatorId = Guid.NewGuid();
        
        var command = new CreateDocumentCommand(
            Title: "My Document",
            Description: "Description",
            FileName: "doc.pdf",
            FilePath: "/files/doc.pdf",
            ContentType: "application/pdf",
            FileSizeBytes: 2048,
            OwnerId: ownerId,
            CreatorId: creatorId);

        Document? capturedDocument = null;
        _mockRepository
            .Setup(x => x.AddAsync(It.IsAny<Document>(), It.IsAny<CancellationToken>()))
            .Callback<Document, CancellationToken>((doc, _) => capturedDocument = doc);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert - document should have CreatedEvent from factory
        Assert.That(capturedDocument!.DomainEvents, Is.Not.Empty);
        Assert.That(capturedDocument.DomainEvents.Count, Is.EqualTo(1));
    }
}