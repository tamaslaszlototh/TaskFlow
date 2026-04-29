using FluentAssertions;
using TaskFlow.Domain.Common.Interfaces;
using TaskFlow.Domain.Common.Models;

namespace TaskFlow.Tests.Domain.Common.Models;

public class AggregateTests
{
    private readonly Guid _aggregateId = Guid.NewGuid();

    [Fact]
    public void AddDomainEvent_AddsEventToList()
    {
        var aggregate = new TestAggregate(_aggregateId);
        var domainEvent = new TestDomainEvent();

        aggregate.AddDomainEvent(domainEvent);

        aggregate.DomainEventCount.Should().Be(1);
    }

    [Fact]
    public void AddDomainEvent_AddsMultipleEvents()
    {
        var aggregate = new TestAggregate(_aggregateId);

        aggregate.AddDomainEvent(new TestDomainEvent());
        aggregate.AddDomainEvent(new TestDomainEvent());

        aggregate.DomainEventCount.Should().Be(2);
    }

    [Fact]
    public void PopDomainEvents_ReturnsCopyOfEvents()
    {
        var aggregate = new TestAggregate(_aggregateId);
        var event1 = new TestDomainEvent();
        var event2 = new TestDomainEvent();
        aggregate.AddDomainEvent(event1);
        aggregate.AddDomainEvent(event2);

        var result = aggregate.PopDomainEvents();

        result.Should().HaveCount(2);
        result.Should().Contain(event1);
        result.Should().Contain(event2);
    }

    [Fact]
    public void PopDomainEvents_ClearsEventsList()
    {
        var aggregate = new TestAggregate(_aggregateId);
        aggregate.AddDomainEvent(new TestDomainEvent());

        aggregate.PopDomainEvents();

        aggregate.DomainEventCount.Should().Be(0);
    }

    [Fact]
    public void PopDomainEvents_OnEmptyAggregate_ReturnsEmptyList()
    {
        var aggregate = new TestAggregate(_aggregateId);

        var result = aggregate.PopDomainEvents();

        result.Should().BeEmpty();
    }

    private class TestAggregate(Guid id) : Aggregate(id)
    {
        public int DomainEventCount => DomainEvents.Count;
    }

    private class TestDomainEvent : IDomainEvent
    {
    }
}
