using DocumentManagementBackend.Application.Common.Interfaces;
using DocumentManagementBackend.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace DocumentManagementBackend.Infrastructure.Services;

public class NotificationService : INotificationService
{
    private readonly IEmailService _emailService;
    private readonly IDocumentRepository _documentRepository;
    private readonly IUserRepository _userRepository;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(
        IEmailService emailService,
        IDocumentRepository documentRepository,
        IUserRepository userRepository,
        ILogger<NotificationService> logger)
    {
        _emailService = emailService;
        _documentRepository = documentRepository;
        _userRepository = userRepository;
        _logger = logger;
    }

    public async Task NotifyDocumentApprovedAsync(Guid documentId, Guid approverId, CancellationToken cancellationToken = default)
    {
        var document = await _documentRepository.GetByIdAsync(documentId, cancellationToken);
        if (document == null) return;

        var owner = await _userRepository.GetByIdAsync(document.OwnerId, cancellationToken);
        if (owner == null) return;

        await _emailService.SendAsync(new EmailMessage(
            To: owner.Email.Value,
            Subject: $"Document '{document.Title}' has been approved",
            Body: $"Hello {owner.FullName},\n\nYour document '{document.Title}' has been approved.\n\nBest regards,\nDocument Management System"),
            cancellationToken);
    }

    public async Task NotifyDocumentRejectedAsync(Guid documentId, Guid rejectorId, string reason, CancellationToken cancellationToken = default)
    {
        var document = await _documentRepository.GetByIdAsync(documentId, cancellationToken);
        if (document == null) return;

        var owner = await _userRepository.GetByIdAsync(document.OwnerId, cancellationToken);
        if (owner == null) return;

        await _emailService.SendAsync(new EmailMessage(
            To: owner.Email.Value,
            Subject: $"Document '{document.Title}' has been rejected",
            Body: $"Hello {owner.FullName},\n\nYour document '{document.Title}' has been rejected.\n\nReason: {reason}\n\nBest regards,\nDocument Management System"),
            cancellationToken);
    }

    public async Task NotifyDocumentCreatedAsync(Guid documentId, Guid creatorId, CancellationToken cancellationToken = default)
    {
        var creator = await _userRepository.GetByIdAsync(creatorId, cancellationToken);
        if (creator == null) return;

        await _emailService.SendAsync(new EmailMessage(
            To: creator.Email.Value,
            Subject: "Document created successfully",
            Body: $"Hello {creator.FullName},\n\nYour document has been created successfully.\n\nBest regards,\nDocument Management System"),
            cancellationToken);
    }
}