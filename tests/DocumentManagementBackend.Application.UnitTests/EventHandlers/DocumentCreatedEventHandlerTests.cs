using DocumentManagementBackend.Application.Common.Interfaces;
using DocumentManagementBackend.Application.Common.Models;
using DocumentManagementBackend.Application.Features.Documents.EventHandlers;
using DocumentManagementBackend.Domain.Events;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace DocumentManagementBackend.Application.UnitTests.EventHandlers;

[TestFixture]
public class DocumentCreatedEventHandlerTests
{
    private Mock<INotificationService> _notificationServiceMock = null!;
    private Mock<ILogger<DocumentCreatedEventHandler>> _loggerMock = null!;
    private DocumentCreatedEventHandler _handler = null!;

    [SetUp]
    public void SetUp()
    {
        _notificationServiceMock = new Mock<INotificationService>();
        _loggerMock = new Mock<ILogger<DocumentCreatedEventHandler>>();
        _handler = new DocumentCreatedEventHandler(
            _notificationServiceMock.Object,
            _loggerMock.Object);
    }

    [Test]
    public async Task Handle_Should_Call_NotifyDocumentCreatedAsync()
    {
        // Arrange
        var documentId = Guid.NewGuid();
        var actorId = Guid.NewGuid();
        var domainEvent = new DocumentCreatedEvent(documentId, actorId);
        var notification = new DomainEventNotification<DocumentCreatedEvent>(domainEvent);

        // Act
        await _handler.Handle(notification, CancellationToken.None);

        // Assert
        _notificationServiceMock.Verify(x => x.NotifyDocumentCreatedAsync(
            documentId,
            actorId,
            CancellationToken.None), Times.Once);
    }
}