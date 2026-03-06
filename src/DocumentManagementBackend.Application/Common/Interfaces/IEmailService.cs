namespace DocumentManagementBackend.Application.Common.Interfaces;

public interface IEmailService
{
    Task SendAsync(EmailMessage message, CancellationToken cancellationToken = default);
}

public record EmailMessage(
    string To,
    string Subject,
    string Body,
    bool IsHtml = false);