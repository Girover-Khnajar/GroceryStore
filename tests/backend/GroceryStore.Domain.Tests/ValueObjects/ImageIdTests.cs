using FluentAssertions;
using GroceryStore.Domain.Exceptions;
using GroceryStore.Domain.ValueObjects;

namespace GroceryStore.Domain.Tests.ValueObjects;

public class ImageIdTests
{
    [Fact]
    public void CreateNew_ReturnsNonEmptyId()
    {
        var id = ImageId.CreateNew();

        id.Value.Should().NotBeEmpty();
    }

    [Fact]
    public void Create_WithValidGuid_ReturnsId()
    {
        var guid = Guid.NewGuid();
        var id = ImageId.Create(guid);

        id.Value.Should().Be(guid);
    }

    [Fact]
    public void Create_EmptyGuid_Throws()
    {
        var act = () => ImageId.Create(Guid.Empty);

        act.Should().Throw<ArgumentException>()
           .WithMessage("*cannot be empty*");
    }

    [Fact]
    public void TwoCreateNew_DifferentValues()
    {
        var a = ImageId.CreateNew();
        var b = ImageId.CreateNew();

        a.Should().NotBe(b);
    }

    // ───────────────── Value equality ─────────────────

    [Fact]
    public void Equality_SameGuid_AreEqual()
    {
        var guid = Guid.NewGuid();
        var a = ImageId.Create(guid);
        var b = ImageId.Create(guid);

        a.Should().Be(b);
    }

    [Fact]
    public void Equality_DifferentGuid_NotEqual()
    {
        var a = ImageId.CreateNew();
        var b = ImageId.CreateNew();

        a.Should().NotBe(b);
    }

    // ───────────────── Implicit conversions ─────────────────

    [Fact]
    public void ImplicitConversion_ImageIdToGuid()
    {
        var id = ImageId.CreateNew();

        Guid guid = id;

        guid.Should().Be(id.Value);
    }

    [Fact]
    public void ImplicitConversion_GuidToImageId()
    {
        var guid = Guid.NewGuid();

        ImageId id = guid;

        id.Value.Should().Be(guid);
    }

    [Fact]
    public void ImplicitConversion_EmptyGuid_Throws()
    {
        var act = () => { ImageId id = Guid.Empty; };

        act.Should().Throw<ArgumentException>();
    }

    // ───────────────── ToString ─────────────────

    [Fact]
    public void ToString_ReturnsGuidString()
    {
        var guid = Guid.NewGuid();
        var id = ImageId.Create(guid);

        id.ToString().Should().Be(guid.ToString());
    }
}
