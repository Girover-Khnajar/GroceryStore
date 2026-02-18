using FluentAssertions;
using GroceryStore.Domain.Exceptions;
using GroceryStore.Domain.ValueObjects;

namespace GroceryStore.Domain.Tests.ValueObjects;

public class ProductImageTests
{
    // ───────────────── Creation ─────────────────

    [Fact]
    public void Create_ValidInputs_ReturnsInstance()
    {
        var image = ProductImage.Create("https://img.test/pic.webp", "Fresh apples", true, 0);

        image.Url.Should().Be("https://img.test/pic.webp");
        image.AltText.Should().Be("Fresh apples");
        image.IsPrimary.Should().BeTrue();
        image.SortOrder.Should().Be(0);
    }

    [Fact]
    public void Create_NullAltText_IsAllowed()
    {
        var image = ProductImage.Create("https://img.test/pic.webp", null, false, 0);

        image.AltText.Should().BeNull();
    }

    [Fact]
    public void Create_TrimsUrlAndAltText()
    {
        var image = ProductImage.Create("  https://img.test/pic.webp  ", "  alt  ", false, 0);

        image.Url.Should().Be("https://img.test/pic.webp");
        image.AltText.Should().Be("alt");
    }

    // ───────────────── Validation ─────────────────

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_NullOrEmptyUrl_Throws(string? url)
    {
        var act = () => ProductImage.Create(url!, "alt", false, 0);

        act.Should().Throw<ValidationException>();
    }

    [Fact]
    public void Create_UrlTooLong_Throws()
    {
        var longUrl = "https://" + new string('a', 500);

        var act = () => ProductImage.Create(longUrl, null, false, 0);

        act.Should().Throw<ValidationException>();
    }

    [Fact]
    public void Create_AltTextTooLong_Throws()
    {
        var longAlt = new string('a', 121);

        var act = () => ProductImage.Create("https://img.test/pic.webp", longAlt, false, 0);

        act.Should().Throw<ValidationException>();
    }

    [Fact]
    public void Create_NegativeSortOrder_Throws()
    {
        var act = () => ProductImage.Create("https://img.test/pic.webp", null, false, -1);

        act.Should().Throw<ValidationException>();
    }

    [Fact]
    public void Create_SortOrderAboveMax_Throws()
    {
        var act = () => ProductImage.Create("https://img.test/pic.webp", null, false, 10_001);

        act.Should().Throw<ValidationException>();
    }

    // ───────────────── Value equality ─────────────────

    [Fact]
    public void Equality_SameValues_AreEqual()
    {
        var a = ProductImage.Create("https://img.test/pic.webp", "alt", true, 1);
        var b = ProductImage.Create("https://img.test/pic.webp", "alt", true, 1);

        a.Should().Be(b);
    }

    [Fact]
    public void Equality_DifferentUrl_NotEqual()
    {
        var a = ProductImage.Create("https://a.test/1.webp", null, false, 0);
        var b = ProductImage.Create("https://b.test/2.webp", null, false, 0);

        a.Should().NotBe(b);
    }
}
