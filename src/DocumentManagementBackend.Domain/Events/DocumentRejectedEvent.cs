namespace DocumentManagementBackend.Domain.Events;

public class DocumentRejectedEvent : BaseDomainEvent
{
    public Guid DocumentId { get; }
    public Guid RejectorId { get; }
    public string RejectionReason { get; }

    public DocumentRejectedEvent(Guid documentId, Guid rejectorId, string rejectionReason)
    {
        DocumentId = documentId;
        RejectorId = rejectorId;
        RejectionReason = rejectionReason;
    }
}