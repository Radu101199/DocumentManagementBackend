// Application/Common/Models/DomainEventNotification.cs
using DocumentManagementBackend.Domain.Events;
using MediatR;

namespace DocumentManagementBackend.Application.Common.Models;

public class DomainEventNotification<TDomainEvent> : INotification
    where TDomainEvent : BaseDomainEvent
{
    public TDomainEvent DomainEvent { get; }

    public DomainEventNotification(TDomainEvent domainEvent)
    {
        DomainEvent = domainEvent;
    }
}