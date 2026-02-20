using DocumentManagementBackend.Application.Common.Exceptions;
using DocumentManagementBackend.Application.Features.Documents.Commands.ApproveDocument;
using DocumentManagementBackend.Domain.Entities;
using DocumentManagementBackend.Domain.Enums;
using DocumentManagementBackend.Domain.Exceptions;
using DocumentManagementBackend.Domain.Interfaces;
using Moq;
using NUnit.Framework;

namespace DocumentManagementBackend.Application.UnitTests.Features.Documents.Commands;

public class ApproveDocumentCommandHandlerTests
{
    private Mock<IDocumentRepository> _mockRepository;
    private ApproveDocumentCommandHandler _handler;

    [SetUp]
    public void Setup()
    {
        _mockRepository = new Mock<IDocumentRepository>();
        _handler = new ApproveDocumentCommandHandler(_mockRepository.Object);
    }

    [Test]
    public async Task Should_Approve_Document_When_Valid()
    {
        // Arrange
        var documentId = Guid.NewGuid();
        var approverId = Guid.NewGuid();
        
        var document = Document.Create(
            "Test Document",
            "Description",
            "test.pdf",
            "/files/test.pdf",
            "application/pdf",
            1024,
            Guid.NewGuid(),
            Guid.NewGuid());
        
        document.RequestApproval();

        _mockRepository
            .Setup(x => x.GetByIdAsync(documentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(document);

        var command = new ApproveDocumentCommand(documentId, approverId, "Looks good!");

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.That(document.Status, Is.EqualTo(DocumentStatus.Published));
        _mockRepository.Verify(
            x => x.UpdateAsync(It.IsAny<Document>(), It.IsAny<CancellationToken>()), 
            Times.Once);
    }

    [Test]
    public async Task Should_Call_Domain_Approve_Method()
    {
        // Arrange
        var documentId = Guid.NewGuid();
        var approverId = Guid.NewGuid();
        
        var document = Document.Create(
            "Test Document",
            "Description",
            "test.pdf",
            "/files/test.pdf",
            "application/pdf",
            1024,
            Guid.NewGuid(),
            Guid.NewGuid());
        
        document.RequestApproval();

        _mockRepository
            .Setup(x => x.GetByIdAsync(documentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(document);

        var command = new ApproveDocumentCommand(documentId, approverId, "Approved");

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert - domain event should be raised
        Assert.That(document.DomainEvents.Count, Is.GreaterThan(0));
    }

    [Test]
    public void Should_Throw_NotFoundException_When_Document_NotFound()
    {
        // Arrange
        var documentId = Guid.NewGuid();
        var approverId = Guid.NewGuid();

        _mockRepository
            .Setup(x => x.GetByIdAsync(documentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Document?)null);

        var command = new ApproveDocumentCommand(documentId, approverId, null);

        // Act & Assert
        Assert.ThrowsAsync<NotFoundException>(async () =>
            await _handler.Handle(command, CancellationToken.None));
    }

    [Test]
    public void Should_Throw_DomainException_When_Document_Cannot_Be_Approved()
    {
        // Arrange
        var documentId = Guid.NewGuid();
        var approverId = Guid.NewGuid();
        
        var document = Document.Create(
            "Test Document",
            "Description",
            "test.pdf",
            "/files/test.pdf",
            "application/pdf",
            1024,
            Guid.NewGuid(),
            Guid.NewGuid());
        
        // Don't request approval - so approve will fail

        _mockRepository
            .Setup(x => x.GetByIdAsync(documentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(document);

        var command = new ApproveDocumentCommand(documentId, approverId, null);

        // Act & Assert
        Assert.ThrowsAsync<DocumentInvalidStateException>(async () =>
            await _handler.Handle(command, CancellationToken.None));
    }

    [Test]
    public async Task Should_Pass_Notes_To_Domain_Method()
    {
        // Arrange
        var documentId = Guid.NewGuid();
        var approverId = Guid.NewGuid();
        const string notes = "Approved with minor comments";
        
        var document = Document.Create(
            "Test Document",
            "Description",
            "test.pdf",
            "/files/test.pdf",
            "application/pdf",
            1024,
            Guid.NewGuid(),
            Guid.NewGuid());
        
        document.RequestApproval();

        _mockRepository
            .Setup(x => x.GetByIdAsync(documentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(document);

        var command = new ApproveDocumentCommand(documentId, approverId, notes);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert - event should contain notes
        var approvedEvent = document.DomainEvents
            .OfType<DocumentManagementBackend.Domain.Events.DocumentApprovedEvent>()
            .FirstOrDefault();

        Assert.That(approvedEvent, Is.Not.Null);
        Assert.That(approvedEvent!.ApprovalNotes, Is.EqualTo(notes));
    }
}