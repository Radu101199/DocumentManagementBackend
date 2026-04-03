using DocumentManagementBackend.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;

namespace DocumentManagementBackend.Infrastructure.Jobs;

public class EmailJob
{
    private readonly IEmailService _emailService;
    private readonly ILogger<EmailJob> _logger;

    public EmailJob(IEmailService emailService, ILogger<EmailJob> logger)
    {
        _emailService = emailService;
        _logger = logger;
    }

    public async Task SendDocumentCreatedEmailAsync(Guid documentId, string ownerEmail)
    {
        _logger.LogInformation("Sending document created email for {DocumentId}", documentId);

        await _emailService.SendAsync(new EmailMessage(
            To: ownerEmail,
            Subject: "Document Created Successfully",
            Body: $"Your document {documentId} has been created and is ready for review.",
            IsHtml: false));
    }

    public async Task SendApprovalRequestEmailAsync(Guid documentId, string adminEmail)
    {
        _logger.LogInformation("Sending approval request email for {DocumentId}", documentId);

        await _emailService.SendAsync(new EmailMessage(
            To: adminEmail,
            Subject: "Document Awaiting Approval",
            Body: $"Document {documentId} is waiting for your approval.",
            IsHtml: false));
    }

    public async Task SendDocumentApprovedEmailAsync(Guid documentId, string ownerEmail)
    {
        _logger.LogInformation("Sending approved email for {DocumentId}", documentId);

        await _emailService.SendAsync(new EmailMessage(
            To: ownerEmail,
            Subject: "Document Approved",
            Body: $"Your document {documentId} has been approved.",
            IsHtml: false));
    }
}
