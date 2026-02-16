namespace DocumentManagementBackend.Domain.Events;

public class DocumentApprovalCancelledEvent : BaseDomainEvent
{
    public Guid DocumentId { get; }
    public Guid CancelledById { get; }
    public string? CancellationReason { get; }

    public DocumentApprovalCancelledEvent(Guid documentId, Guid cancelledById, string? cancellationReason = null)
    {
        DocumentId = documentId;
        CancelledById = cancelledById;
        CancellationReason = cancellationReason;
    }
}