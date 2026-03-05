using DocumentManagementBackend.Application.Common.Interfaces;
using DocumentManagementBackend.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace DocumentManagementBackend.Application.Features.Documents.EventHandlers;

public class DocumentRejectedEventHandler : INotificationHandler<DocumentRejectedEvent>
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

    public async Task Handle(DocumentRejectedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Document {DocumentId} rejected by {RejectorId}",
            notification.DocumentId,
            notification.RejectorId);

        await _notificationService.NotifyDocumentRejectedAsync(
            notification.DocumentId,
            notification.RejectorId,
            notification.Reason,
            cancellationToken);
    }
}