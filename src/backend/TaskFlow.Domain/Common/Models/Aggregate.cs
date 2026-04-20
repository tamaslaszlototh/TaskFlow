using TaskFlow.Domain.Common.Interfaces;

namespace TaskFlow.Domain.Common.Models;

public abstract class Aggregate : Entity
{
    protected readonly List<IDomainEvent> DomainEvents = [];

    protected Aggregate(Guid id) : base(id)
    {
    }

    public void AddDomainEvent(IDomainEvent domainEvent)
    {
        DomainEvents.Add(domainEvent);
    }

    public List<IDomainEvent> PopDomainEvents()
    {
        var domainEventsCopy = DomainEvents.ToList();
        DomainEvents.Clear();
        return domainEventsCopy;
    }
}