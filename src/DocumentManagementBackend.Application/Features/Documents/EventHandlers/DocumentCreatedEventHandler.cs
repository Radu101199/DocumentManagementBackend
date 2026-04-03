using DocumentManagementBackend.Application.Common.Interfaces;
using DocumentManagementBackend.Application.Common.Models;
using DocumentManagementBackend.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace DocumentManagementBackend.Application.Features.Documents.EventHandlers;

public class DocumentCreatedEventHandler : INotificationHandler<DomainEventNotification<DocumentCreatedEvent>>
{
    private readonly INotificationService _notificationService;
    private readonly IBackgroundJobService _backgroundJobService;
    private readonly ILogger<DocumentCreatedEventHandler> _logger;

    
    public DocumentCreatedEventHandler(
        INotificationService notificationService,
        IBackgroundJobService backgroundJobService,
        ILogger<DocumentCreatedEventHandler> logger)
    {
        _notificationService = notificationService;
        _backgroundJobService = backgroundJobService;
        _logger = logger;
    }

    public async Task Handle(DomainEventNotification<DocumentCreatedEvent> notification, CancellationToken cancellationToken)
    {
        var domainEvent = notification.DomainEvent;

        _logger.LogInformation(
            "Document {DocumentId} created by {CreatorId}",
            domainEvent.DocumentId,
            domainEvent.ActorId);
        
        // ✅ Fire-and-forget — nu blochează request-ul
        //TODO modificat aici _backgroundJobService.Enqueue<EmailJob>(job =>
        //     job.SendDocumentCreatedEmailAsync(domainEvent.DocumentId, "owner@example.com"));

        
        await _notificationService.NotifyDocumentCreatedAsync(
            domainEvent.DocumentId,
            domainEvent.ActorId,
            cancellationToken);
    }
}