using FluentAssertions;
using GroceryStore.Domain.Exceptions;
using GroceryStore.Domain.ValueObjects;

namespace GroceryStore.Domain.Tests.ValueObjects;

public class SlugTests
{
    // ───────────────── Creation ─────────────────

    [Fact]
    public void Create_SimpleWord_ReturnsSlug()
    {
        var slug = Slug.Create("apples");

        slug.Value.Should().Be("apples");
    }

    [Fact]
    public void Create_MultipleWords_ConvertsToDashes()
    {
        var slug = Slug.Create("fresh green apples");

        slug.Value.Should().Be("fresh-green-apples");
    }

    [Fact]
    public void Create_UpperCase_NormalizesToLower()
    {
        var slug = Slug.Create("Fresh Apples");

        slug.Value.Should().Be("fresh-apples");
    }

    [Fact]
    public void Create_Underscores_ConvertedToDashes()
    {
        var slug = Slug.Create("fresh_apples");

        slug.Value.Should().Be("fresh-apples");
    }

    [Fact]
    public void Create_ConsecutiveDashes_CollapsedToOne()
    {
        var slug = Slug.Create("fresh--apples");

        slug.Value.Should().Be("fresh-apples");
    }

    [Fact]
    public void Create_LeadingTrailingSpaces_Trimmed()
    {
        var slug = Slug.Create("  apples  ");

        slug.Value.Should().Be("apples");
    }

    [Fact]
    public void Create_WithExistingValidSlug_ReturnsSame()
    {
        var slug = Slug.Create("fresh-organic-milk");

        slug.Value.Should().Be("fresh-organic-milk");
    }

    // ───────────────── Validation ─────────────────

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_NullOrWhitespace_Throws(string? input)
    {
        var act = () => Slug.Create(input!);

        act.Should().Throw<ValidationException>();
    }

    [Fact]
    public void Create_TooLong_Throws()
    {
        var longInput = new string('a', 121);

        var act = () => Slug.Create(longInput);

        act.Should().Throw<ValidationException>();
    }

    [Fact]
    public void Create_OnlySpecialChars_Throws()
    {
        var act = () => Slug.Create("!!!@@@###");

        act.Should().Throw<ValidationException>()
           .WithMessage("*slug*");
    }

    // ───────────────── Value equality ─────────────────

    [Fact]
    public void Equality_SameValue_AreEqual()
    {
        var a = Slug.Create("apples");
        var b = Slug.Create("apples");

        a.Should().Be(b);
    }

    [Fact]
    public void Equality_DifferentValue_NotEqual()
    {
        var a = Slug.Create("apples");
        var b = Slug.Create("oranges");

        a.Should().NotBe(b);
    }

    // ───────────────── ToString ─────────────────

    [Fact]
    public void ToString_ReturnsValue()
    {
        var slug = Slug.Create("apples");

        slug.ToString().Should().Contain("apples");
    }
}
