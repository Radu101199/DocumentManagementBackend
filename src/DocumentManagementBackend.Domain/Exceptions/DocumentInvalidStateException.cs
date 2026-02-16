namespace DocumentManagementBackend.Domain.Exceptions;

public class DocumentInvalidStateException : DomainException
{
    public Guid DocumentId { get; }
    public string CurrentState { get; }

    public DocumentInvalidStateException(Guid documentId, string currentState, string attemptedAction)
        : base($"Document '{documentId}' is in '{currentState}' state and cannot perform action: {attemptedAction}")
    {
        DocumentId = documentId;
        CurrentState = currentState;
    }
}