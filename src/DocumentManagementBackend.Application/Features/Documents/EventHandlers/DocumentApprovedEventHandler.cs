using DocumentManagementBackend.Application.Common.Interfaces;
using DocumentManagementBackend.Application.Common.Models;
using DocumentManagementBackend.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace DocumentManagementBackend.Application.Features.Documents.EventHandlers;

public class DocumentApprovedEventHandler : INotificationHandler<DomainEventNotification<DocumentApprovedEvent>>
{
    private readonly INotificationService _notificationService;
    private readonly ILogger<DocumentApprovedEventHandler> _logger;

    public DocumentApprovedEventHandler(
        INotificationService notificationService,
        ILogger<DocumentApprovedEventHandler> logger)
    {
        _notificationService = notificationService;
        _logger = logger;
    }

    public async Task Handle(DomainEventNotification<DocumentApprovedEvent> notification, CancellationToken cancellationToken)
    {
        var domainEvent = notification.DomainEvent;

        _logger.LogInformation(
            "Document {DocumentId} approved by {ApproverId}",
            domainEvent.DocumentId,
            domainEvent.ApproverId);

        await _notificationService.NotifyDocumentApprovedAsync(
            domainEvent.DocumentId,
            domainEvent.ApproverId,
            cancellationToken);
    }
}