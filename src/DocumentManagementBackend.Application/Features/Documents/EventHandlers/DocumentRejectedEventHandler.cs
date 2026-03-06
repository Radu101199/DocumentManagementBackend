using DocumentManagementBackend.Application.Common.Interfaces;
using DocumentManagementBackend.Application.Common.Models;
using DocumentManagementBackend.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace DocumentManagementBackend.Application.Features.Documents.EventHandlers;

public class DocumentRejectedEventHandler : INotificationHandler<DomainEventNotification<DocumentRejectedEvent>>
{
    private readonly INotificationService _notificationService;
    private readonly ILogger<DocumentRejectedEventHandler> _logger;

    public DocumentRejectedEventHandler(
        INotificationService notificationService,
        ILogger<DocumentRejectedEventHandler> logger)
    {
        _notificationService = notificationService;
        _logger = logger;
    }

    public async Task Handle(DomainEventNotification<DocumentRejectedEvent> notification, CancellationToken cancellationToken)
    {
        var domainEvent = notification.DomainEvent;

        _logger.LogInformation(
            "Document {DocumentId} rejected by {RejectorId}",
            domainEvent.DocumentId,
            domainEvent.RejectorId);

        await _notificationService.NotifyDocumentRejectedAsync(
            domainEvent.DocumentId,
            domainEvent.RejectorId,
            domainEvent.RejectionReason,
            cancellationToken);
    }
}