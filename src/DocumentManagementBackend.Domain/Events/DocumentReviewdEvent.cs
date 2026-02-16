namespace DocumentManagementBackend.Domain.Events;

public class DocumentReviewedEvent : BaseDomainEvent
{
    public Guid DocumentId { get; }
    public Guid ReviewerId { get; }
    public string? ReviewNotes { get; }

    public DocumentReviewedEvent(Guid documentId, Guid reviewerId, string? reviewNotes = null)
    {
        DocumentId = documentId;
        ReviewerId = reviewerId;
        ReviewNotes = reviewNotes;
    }
}