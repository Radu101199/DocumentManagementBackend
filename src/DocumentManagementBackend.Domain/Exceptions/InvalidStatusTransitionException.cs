using DocumentManagementBackend.Domain.Enums;

namespace DocumentManagementBackend.Domain.Exceptions;

public class InvalidStatusTransitionException : DomainException
{
    public Guid DocumentId { get; }
    public DocumentStatus CurrentStatus { get; }
    public DocumentStatus AttemptedStatus { get; }

    public InvalidStatusTransitionException(
        Guid documentId, 
        DocumentStatus currentStatus, 
        DocumentStatus attemptedStatus)
        : base($"Cannot transition document '{documentId}' from '{currentStatus}' to '{attemptedStatus}'")
    {
        DocumentId = documentId;
        CurrentStatus = currentStatus;
        AttemptedStatus = attemptedStatus;
    }
}