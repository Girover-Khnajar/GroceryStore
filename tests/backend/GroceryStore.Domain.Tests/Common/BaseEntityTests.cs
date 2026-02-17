using FluentAssertions;
using GroceryStore.Domain.Common.DomainEvents;

namespace GroceryStore.Domain.Tests.Common;

file class TestBaseEntity : BaseEntity
{
    public void RaiseDomainEvent(IDomainEvent domainEvent) => AddDomainEvent(domainEvent);
}

file record TestDomainEvent : DomainEvent;

public class BaseEntityTests
{
    [Fact]
    public void NewBaseEntity_HasNoDomainEvents()
    {
        var entity = new TestBaseEntity();

        entity.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void AddDomainEvent_EventIsStored()
    {
        var entity = new TestBaseEntity();
        var evt = new TestDomainEvent();

        entity.RaiseDomainEvent(evt);

        entity.DomainEvents.Should().ContainSingle()
              .Which.Should().Be(evt);
    }

    [Fact]
    public void ClearDomainEvents_RemovesAll()
    {
        var entity = new TestBaseEntity();
        entity.RaiseDomainEvent(new TestDomainEvent());
        entity.RaiseDomainEvent(new TestDomainEvent());

        entity.ClearDomainEvents();

        entity.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void DomainEvent_HasOccurredOnUtc()
    {
        var before = DateTime.UtcNow;
        var evt = new TestDomainEvent();
        var after = DateTime.UtcNow;

        evt.OccurredOnUtc.Should().BeOnOrAfter(before).And.BeOnOrBefore(after);
    }
}
