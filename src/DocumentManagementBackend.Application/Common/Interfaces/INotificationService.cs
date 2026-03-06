namespace DocumentManagementBackend.Application.Common.Interfaces;

public interface INotificationService
{
    Task NotifyDocumentApprovedAsync(Guid documentId, Guid approverId, CancellationToken cancellationToken = default);
    Task NotifyDocumentRejectedAsync(Guid documentId, Guid rejectorId, string reason, CancellationToken cancellationToken = default);
    Task NotifyDocumentCreatedAsync(Guid documentId, Guid creatorId, CancellationToken cancellationToken = default);
}