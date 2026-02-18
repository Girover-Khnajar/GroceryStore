using FluentAssertions;
using GroceryStore.Domain.Common.DomainEvents;

namespace GroceryStore.Domain.Tests.Common;

// Concrete subclass to test the abstract Entity
file class TestEntity : Entity { }

public class EntityTests
{
    [Fact]
    public void NewEntity_HasNonEmptyId()
    {
        var entity = new TestEntity();

        entity.Id.Should().NotBeEmpty();
    }

    [Fact]
    public void TwoEntities_HaveDifferentIds()
    {
        var a = new TestEntity();
        var b = new TestEntity();

        a.Id.Should().NotBe(b.Id);
    }

    [Fact]
    public void Equals_SameId_ReturnsTrue()
    {
        var a = new TestEntity();
        var b = new TestEntity();

        // Force same Id via reflection (Id is protected set)
        typeof(Entity).GetProperty(nameof(Entity.Id))!
            .SetValue(b, a.Id);

        a.Equals(b).Should().BeTrue();
    }

    [Fact]
    public void Equals_DifferentId_ReturnsFalse()
    {
        var a = new TestEntity();
        var b = new TestEntity();

        a.Equals(b).Should().BeFalse();
    }

    [Fact]
    public void Equals_Null_ReturnsFalse()
    {
        var entity = new TestEntity();

        entity.Equals(null).Should().BeFalse();
    }

    [Fact]
    public void GetHashCode_BasedOnId()
    {
        var entity = new TestEntity();

        entity.GetHashCode().Should().Be(entity.Id.GetHashCode());
    }
}
