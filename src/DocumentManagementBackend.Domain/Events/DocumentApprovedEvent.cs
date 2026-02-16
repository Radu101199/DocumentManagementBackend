namespace DocumentManagementBackend.Domain.Events;

public class DocumentApprovedEvent : BaseDomainEvent
{
    public Guid DocumentId { get; }
    public Guid ApproverId { get; }
    public string? ApprovalNotes { get; }

    public DocumentApprovedEvent(Guid documentId, Guid approverId, string? approvalNotes = null)
    {
        DocumentId = documentId;
        ApproverId = approverId;
        ApprovalNotes = approvalNotes;
    }
}