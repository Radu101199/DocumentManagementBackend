namespace DocumentManagementBackend.Domain.Exceptions;

public class ApprovalWindowExpiredException : DomainException
{
    public Guid DocumentId { get; }
    public DateTime ExpiredOn { get; }

    public ApprovalWindowExpiredException(Guid documentId, DateTime expiredOn)
        : base($"Document '{documentId}' approval window expired on {expiredOn:yyyy-MM-dd HH:mm:ss}")
    {
        DocumentId = documentId;
        ExpiredOn = expiredOn;
    }
}