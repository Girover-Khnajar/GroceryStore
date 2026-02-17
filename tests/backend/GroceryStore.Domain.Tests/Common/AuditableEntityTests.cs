using FluentAssertions;
using GroceryStore.Domain.Common.DomainEvents;

namespace GroceryStore.Domain.Tests.Common;

file class TestAuditableEntity : AuditableEntity
{
    public void SimulateTouch() => Touch();
}

public class AuditableEntityTests
{
    [Fact]
    public void NewAuditableEntity_HasCreatedOnUtc()
    {
        var before = DateTime.UtcNow;
        var entity = new TestAuditableEntity();
        var after = DateTime.UtcNow;

        entity.CreatedOnUtc.Should().BeOnOrAfter(before).And.BeOnOrBefore(after);
    }

    [Fact]
    public void NewAuditableEntity_ModifiedOnUtc_IsNull()
    {
        var entity = new TestAuditableEntity();

        entity.ModifiedOnUtc.Should().BeNull();
    }

    [Fact]
    public void Touch_SetsModifiedOnUtc()
    {
        var entity = new TestAuditableEntity();

        var before = DateTime.UtcNow;
        entity.SimulateTouch();
        var after = DateTime.UtcNow;

        entity.ModifiedOnUtc.Should().NotBeNull();
        entity.ModifiedOnUtc!.Value.Should().BeOnOrAfter(before).And.BeOnOrBefore(after);
    }
}
