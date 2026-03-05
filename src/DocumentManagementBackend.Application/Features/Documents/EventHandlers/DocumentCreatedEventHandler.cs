using DocumentManagementBackend.Application.Common.Interfaces;
using DocumentManagementBackend.Application.Common.Interfaces;
using DocumentManagementBackend.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace DocumentManagementBackend.Application.Features.Documents.EventHandlers;

public class DocumentCreatedEventHandler : INotificationHandler<DocumentCreatedEvent>
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

    public async Task Handle(DocumentCreatedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Document {DocumentId} created by {CreatorId}",
            notification.DocumentId,
            notification.CreatorId);

        await _notificationService.NotifyDocumentCreatedAsync(
            notification.DocumentId,
            notification.CreatorId,
            cancellationToken);
    }
}