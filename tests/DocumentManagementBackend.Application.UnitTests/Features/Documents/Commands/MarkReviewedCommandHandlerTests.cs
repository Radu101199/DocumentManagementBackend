using DocumentManagementBackend.Application.Common.Exceptions;
using DocumentManagementBackend.Application.Features.Documents.Commands.MarkReviewed;
using DocumentManagementBackend.Application.UnitTests.TestHelpers;
using DocumentManagementBackend.Domain.Entities;
using DocumentManagementBackend.Domain.Interfaces;
using Moq;
using System.Linq;
using NUnit.Framework;


namespace DocumentManagementBackend.Application.UnitTests.Features.Documents.Commands;

public class MarkReviewedCommandHandlerTests
{
    private Mock<IDocumentRepository> _mockRepository;
    private MarkReviewedCommandHandler _handler;

    [SetUp]
    public void Setup()
    {
        _mockRepository = new Mock<IDocumentRepository>();
        _handler = new MarkReviewedCommandHandler(_mockRepository.Object);
    }

    [Test]
    public async Task Should_Mark_Document_As_Reviewed()
    {
        // Arrange
        var document = DocumentBuilder.Create().Build();
        var reviewerId = Guid.NewGuid();

        var command = new MarkReviewedCommand(
            DocumentId: document.Id,
            ReviewerId: reviewerId,
            Notes: "Looks good");

        _mockRepository
            .Setup(x => x.GetByIdAsync(document.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(document);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _mockRepository.Verify(
            x => x.UpdateAsync(It.IsAny<Document>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task Should_Call_Domain_Method_MarkAsReviewed()
    {
        // Arrange
        var document = DocumentBuilder.Create().Build();
        var reviewerId = Guid.NewGuid();

        var command = new MarkReviewedCommand(
            DocumentId: document.Id,
            ReviewerId: reviewerId,
            Notes: "Please check section 3");

        _mockRepository
            .Setup(x => x.GetByIdAsync(document.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(document);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert - domain event should be raised
        Assert.That(document.DomainEvents, Is.Not.Empty);
        var reviewEvent = document.DomainEvents.OfType<Domain.Events.DocumentReviewedEvent>().FirstOrDefault();
        Assert.That(reviewEvent, Is.Not.Null);
        Assert.That(reviewEvent!.ReviewerId, Is.EqualTo(reviewerId));
        Assert.That(reviewEvent.ReviewNotes, Is.EqualTo("Please check section 3"));
    }

    [Test]
    public void Should_Throw_NotFoundException_When_Document_NotFound()
    {
        // Arrange
        var command = new MarkReviewedCommand(
            DocumentId: Guid.NewGuid(),
            ReviewerId: Guid.NewGuid(),
            Notes: "Notes");

        _mockRepository
            .Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Document?)null);

        // Act & Assert
        Assert.ThrowsAsync<NotFoundException>(() =>
            _handler.Handle(command, CancellationToken.None));
    }

    [Test]
    public async Task Should_Update_Repository_After_Review()
    {
        // Arrange
        var document = DocumentBuilder.Create().Build();

        var command = new MarkReviewedCommand(
            DocumentId: document.Id,
            ReviewerId: Guid.NewGuid(),
            Notes: null);

        _mockRepository
            .Setup(x => x.GetByIdAsync(document.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(document);

        Document? updatedDocument = null;
        _mockRepository
            .Setup(x => x.UpdateAsync(It.IsAny<Document>(), It.IsAny<CancellationToken>()))
            .Callback<Document, CancellationToken>((doc, _) => updatedDocument = doc)
            .Returns(Task.CompletedTask);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.That(updatedDocument, Is.Not.Null);
        Assert.That(updatedDocument!.Id, Is.EqualTo(document.Id));

        _mockRepository.Verify(
            x => x.UpdateAsync(It.Is<Document>(d => d.Id == document.Id), It.IsAny<CancellationToken>()),
            Times.Once);
    }
}