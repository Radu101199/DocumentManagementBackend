using DocumentManagementBackend.Application.Common.Interfaces;
using DocumentManagementBackend.Application.Common.Models;
using DocumentManagementBackend.Application.Features.Documents.EventHandlers;
using DocumentManagementBackend.Domain.Events;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace DocumentManagementBackend.Application.UnitTests.EventHandlers;

[TestFixture]
public class DocumentApprovedEventHandlerTests
{
    private Mock<INotificationService> _notificationServiceMock = null!;
    private Mock<ILogger<DocumentApprovedEventHandler>> _loggerMock = null!;
    private DocumentApprovedEventHandler _handler = null!;

    [SetUp]
    public void SetUp()
    {
        _notificationServiceMock = new Mock<INotificationService>();
        _loggerMock = new Mock<ILogger<DocumentApprovedEventHandler>>();
        _handler = new DocumentApprovedEventHandler(
            _notificationServiceMock.Object,
            _loggerMock.Object);
    }

    [Test]
    public async Task Handle_Should_Call_NotifyDocumentApprovedAsync()
    {
        // Arrange
        var documentId = Guid.NewGuid();
        var approverId = Guid.NewGuid();
        var domainEvent = new DocumentApprovedEvent(documentId, approverId, "Looks good");
        var notification = new DomainEventNotification<DocumentApprovedEvent>(domainEvent);

        // Act
        await _handler.Handle(notification, CancellationToken.None);

        // Assert
        _notificationServiceMock.Verify(x => x.NotifyDocumentApprovedAsync(
            documentId,
            approverId,
            CancellationToken.None), Times.Once);
    }

    [Test]
    public async Task Handle_Should_Not_Throw_When_Notification_Succeeds()
    {
        // Arrange
        var domainEvent = new DocumentApprovedEvent(Guid.NewGuid(), Guid.NewGuid(), null);
        var notification = new DomainEventNotification<DocumentApprovedEvent>(domainEvent);

        _notificationServiceMock
            .Setup(x => x.NotifyDocumentApprovedAsync(
                It.IsAny<Guid>(),
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act & Assert
        Assert.DoesNotThrowAsync(() => _handler.Handle(notification, CancellationToken.None));
    }
}