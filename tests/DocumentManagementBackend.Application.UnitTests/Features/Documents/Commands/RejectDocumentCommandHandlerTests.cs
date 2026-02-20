using DocumentManagementBackend.Application.Common.Exceptions;
using DocumentManagementBackend.Application.Features.Documents.Commands.RejectDocument;
using DocumentManagementBackend.Domain.Entities;
using DocumentManagementBackend.Domain.Enums;
using DocumentManagementBackend.Domain.Exceptions;
using DocumentManagementBackend.Domain.Interfaces;
using Moq;
using NUnit.Framework;


namespace DocumentManagementBackend.Application.UnitTests.Features.Documents.Commands;

public class RejectDocumentCommandHandlerTests
{
    private Mock<IDocumentRepository> _mockRepository;
    private RejectDocumentCommandHandler _handler;
    
    [SetUp]
    public void Setup()
    {
        _mockRepository = new Mock<IDocumentRepository>();
        _handler = new RejectDocumentCommandHandler(_mockRepository.Object);
    }

    [Test]
    public async Task Should_Reject_Document_When_Valid()
    {
        // Arrange
        var documentId = Guid.NewGuid();
        var rejectorId = Guid.NewGuid();
        
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

        var command = new RejectDocumentCommand(documentId, rejectorId, "Needs changes.");
        
        // Act
        await _handler.Handle(command, CancellationToken.None);
        
        // Assert
        Assert.That(document.Status, Is.EqualTo(DocumentStatus.Draft));
        _mockRepository.Verify(
            x => x.UpdateAsync(It.IsAny<Document>(), It.IsAny<CancellationToken>()), 
            Times.Once);
    }

    [Test]
    public async Task Should_Call_Domain_Reject_Method()
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
        var command = new RejectDocumentCommand(documentId, approverId, "Needs changes.");
            
        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.That(document.DomainEvents.Count, Is.GreaterThan(0));
    }

    [Test]
    public async Task Should_Pass_RejectionReason_To_Domain_Method() 
    {
        var documentId = Guid.NewGuid();
        var approverId = Guid.NewGuid();
        const string rejectionReason = "Needs changes.";
        
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
        
        var command = new RejectDocumentCommand(documentId, approverId, rejectionReason);
        
        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert - event should contain rejectionReason
        var rejectedEvent = document.DomainEvents
            .OfType<DocumentManagementBackend.Domain.Events.DocumentRejectedEvent>()
            .FirstOrDefault();

        Assert.That(rejectedEvent, Is.Not.Null);
        Assert.That(rejectedEvent!.RejectionReason, Is.EqualTo(rejectionReason));
    }

}
