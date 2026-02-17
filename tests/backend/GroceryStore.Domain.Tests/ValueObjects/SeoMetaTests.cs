using FluentAssertions;
using GroceryStore.Domain.Exceptions;
using GroceryStore.Domain.ValueObjects;

namespace GroceryStore.Domain.Tests.ValueObjects;

public class SeoMetaTests
{
    // ───────────────── Creation ─────────────────

    [Fact]
    public void Create_BothNull_IsAllowed()
    {
        var seo = SeoMeta.Create(null, null);

        seo.MetaTitle.Should().BeNull();
        seo.MetaDescription.Should().BeNull();
    }

    [Fact]
    public void Create_ValidValues_TrimsAndStores()
    {
        var seo = SeoMeta.Create("  My Title  ", "  My Description  ");

        seo.MetaTitle.Should().Be("My Title");
        seo.MetaDescription.Should().Be("My Description");
    }

    [Fact]
    public void Create_OnlyTitle_DescriptionNull()
    {
        var seo = SeoMeta.Create("Title", null);

        seo.MetaTitle.Should().Be("Title");
        seo.MetaDescription.Should().BeNull();
    }

    // ───────────────── Validation ─────────────────

    [Fact]
    public void Create_TitleTooLong_Throws()
    {
        var longTitle = new string('a', 61);

        var act = () => SeoMeta.Create(longTitle, null);

        act.Should().Throw<ValidationException>();
    }

    [Fact]
    public void Create_DescriptionTooLong_Throws()
    {
        var longDesc = new string('a', 161);

        var act = () => SeoMeta.Create(null, longDesc);

        act.Should().Throw<ValidationException>();
    }

    [Fact]
    public void Create_TitleExactLength_DoesNotThrow()
    {
        var title = new string('a', 60);

        var act = () => SeoMeta.Create(title, null);

        act.Should().NotThrow();
    }

    [Fact]
    public void Create_DescriptionExactLength_DoesNotThrow()
    {
        var desc = new string('a', 160);

        var act = () => SeoMeta.Create(null, desc);

        act.Should().NotThrow();
    }

    // ───────────────── Value equality ─────────────────

    [Fact]
    public void Equality_SameValues_AreEqual()
    {
        var a = SeoMeta.Create("Title", "Desc");
        var b = SeoMeta.Create("Title", "Desc");

        a.Should().Be(b);
    }

    [Fact]
    public void Equality_DifferentValues_NotEqual()
    {
        var a = SeoMeta.Create("Title A", null);
        var b = SeoMeta.Create("Title B", null);

        a.Should().NotBe(b);
    }
}
