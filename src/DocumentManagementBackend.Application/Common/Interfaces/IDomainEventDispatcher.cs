using DocumentManagementBackend.Domain.Events;

namespace DocumentManagementBackend.Application.Common.Interfaces;

public interface IDomainEventDispatcher
{
    Task DispatchAsync(IEnumerable<BaseDomainEvent> domainEvents, CancellationToken cancellationToken = default);
}