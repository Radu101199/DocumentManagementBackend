namespace DocumentManagementBackend.Domain.Events;

public abstract class BaseDomainEvent
{
    public Guid id  { get; } =  Guid.NewGuid();
    public DateTime OccuredOn { get; } = DateTime.UtcNow;
}