namespace DocumentManagementBackend.Domain.Events;

public class DocumentCreatedEvent : BaseDomainEvent
{
    public Guid DocumentId { get; }
    public Guid ActorId { get; }

    public DocumentCreatedEvent(Guid documentId, Guid actorId)
    {
        DocumentId = documentId;
        ActorId = actorId;
    }
}