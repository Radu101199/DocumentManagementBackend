using DocumentManagementBackend.Application.Common.Interfaces;
using DocumentManagementBackend.Application.Common.Models;
using DocumentManagementBackend.Application.Features.Documents.EventHandlers;
using DocumentManagementBackend.Domain.Events;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace DocumentManagementBackend.Application.UnitTests.EventHandlers;

[TestFixture]
public class DocumentRejectedEventHandlerTests
{
    private Mock<INotificationService> _notificationServiceMock = null!;
    private Mock<ILogger<DocumentRejectedEventHandler>> _loggerMock = null!;
    private DocumentRejectedEventHandler _handler = null!;

    [SetUp]
    public void SetUp()
    {
        _notificationServiceMock = new Mock<INotificationService>();
        _loggerMock = new Mock<ILogger<DocumentRejectedEventHandler>>();
        _handler = new DocumentRejectedEventHandler(
            _notificationServiceMock.Object,
            _loggerMock.Object);
    }

    [Test]
    public async Task Handle_Should_Call_NotifyDocumentRejectedAsync()
    {
        // Arrange
        var documentId = Guid.NewGuid();
        var rejectorId = Guid.NewGuid();
        var reason = "Missing signatures";
        var domainEvent = new DocumentRejectedEvent(documentId, rejectorId, reason);
        var notification = new DomainEventNotification<DocumentRejectedEvent>(domainEvent);

        // Act
        await _handler.Handle(notification, CancellationToken.None);

        // Assert
        _notificationServiceMock.Verify(x => x.NotifyDocumentRejectedAsync(
            documentId,
            rejectorId,
            reason,
            CancellationToken.None), Times.Once);
    }
}