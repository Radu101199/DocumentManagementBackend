using DocumentManagementBackend.Application.Common.Interfaces;
using DocumentManagementBackend.Application.Common.Models;
using DocumentManagementBackend.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace DocumentManagementBackend.Application.Features.Documents.EventHandlers;

public class DocumentCreatedEventHandler : INotificationHandler<DomainEventNotification<DocumentCreatedEvent>>
{
    private readonly INotificationService _notificationService;
    private readonly ILogger<DocumentCreatedEventHandler> _logger;

    public DocumentCreatedEventHandler(
        INotificationService notificationService,
        ILogger<DocumentCreatedEventHandler> logger)
    {
        _notificationService = notificationService;
        _logger = logger;
    }

    public async Task Handle(DomainEventNotification<DocumentCreatedEvent> notification, CancellationToken cancellationToken)
    {
        var domainEvent = notification.DomainEvent;

        _logger.LogInformation(
            "Document {DocumentId} created by {CreatorId}",
            domainEvent.DocumentId,
            domainEvent.ActorId);

        await _notificationService.NotifyDocumentCreatedAsync(
            domainEvent.DocumentId,
            domainEvent.ActorId,
            cancellationToken);
    }
}