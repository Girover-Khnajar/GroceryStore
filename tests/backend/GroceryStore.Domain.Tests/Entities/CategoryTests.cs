using FluentAssertions;
using GroceryStore.Domain.Entities;
using GroceryStore.Domain.Exceptions;
using GroceryStore.Domain.Interfaces;
using GroceryStore.Domain.ValueObjects;

namespace GroceryStore.Domain.Tests.Entities;

public class CategoryTests
{
    // ─── Helpers ───

    private static Category CreateValid(
        string name = "Fruits",
        string slug = "fruits",
        int sortOrder = 0,
        Guid? parentCategoryId = null,
        string? description = null,
        string? imageUrl = null,
        string? iconName = null)
        => Category.Create(name, slug, sortOrder, parentCategoryId, description, imageUrl, iconName);

    // ═══════════════════════════════════════════
    // Create
    // ═══════════════════════════════════════════

    [Fact]
    public void Create_ValidInputs_ReturnsCategory()
    {
        var parentId = Guid.NewGuid();
        var cat = Category.Create(
            "Fruits", "fruits", 1, parentId, "All fruits", "https://img/fruit.png", "apple");

        cat.Name.Should().Be("Fruits");
        cat.Slug.Value.Should().Be("fruits");
        cat.SortOrder.Should().Be(1);
        cat.ParentCategoryId.Should().Be(parentId);
        cat.Description.Should().Be("All fruits");
        cat.ImageUrl.Should().Be("https://img/fruit.png");
        cat.IconName.Should().Be("apple");
        cat.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Create_MinimalInputs_ReturnsCategory()
    {
        var cat = CreateValid();

        cat.Name.Should().Be("Fruits");
        cat.ParentCategoryId.Should().BeNull();
        cat.Description.Should().BeNull();
        cat.ImageUrl.Should().BeNull();
        cat.IconName.Should().BeNull();
    }

    [Fact]
    public void Create_TrimsName()
    {
        var cat = CreateValid(name: "  Fruits  ");

        cat.Name.Should().Be("Fruits");
    }

    [Fact]
    public void Create_ImplementsIAggregateRoot()
    {
        var cat = CreateValid();

        cat.Should().BeAssignableTo<IAggregateRoot>();
    }

    // ── Create validation ──

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_NullOrEmptyName_Throws(string? name)
    {
        var act = () => CreateValid(name: name!);

        act.Should().Throw<ValidationException>();
    }

    [Fact]
    public void Create_NameTooLong_Throws()
    {
        var act = () => CreateValid(name: new string('a', 121));

        act.Should().Throw<ValidationException>();
    }

    [Fact]
    public void Create_SortOrderBelowMin_Throws()
    {
        var act = () => CreateValid(sortOrder: -1);

        act.Should().Throw<ValidationException>();
    }

    [Fact]
    public void Create_SortOrderAboveMax_Throws()
    {
        var act = () => CreateValid(sortOrder: 10_001);

        act.Should().Throw<ValidationException>();
    }

    // ═══════════════════════════════════════════
    // Rename
    // ═══════════════════════════════════════════

    [Fact]
    public void Rename_ValidName_Updates()
    {
        var cat = CreateValid();

        cat.Rename("Vegetables");

        cat.Name.Should().Be("Vegetables");
        cat.ModifiedOnUtc.Should().NotBeNull();
    }

    [Fact]
    public void Rename_TrimsName()
    {
        var cat = CreateValid();

        cat.Rename("  Vegetables  ");

        cat.Name.Should().Be("Vegetables");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Rename_NullOrEmpty_Throws(string? name)
    {
        var cat = CreateValid();

        var act = () => cat.Rename(name!);

        act.Should().Throw<ValidationException>();
    }

    [Fact]
    public void Rename_TooLong_Throws()
    {
        var cat = CreateValid();

        var act = () => cat.Rename(new string('x', 121));

        act.Should().Throw<ValidationException>();
    }

    // ═══════════════════════════════════════════
    // ChangeSlug
    // ═══════════════════════════════════════════

    [Fact]
    public void ChangeSlug_ValidSlug_Updates()
    {
        var cat = CreateValid();

        cat.ChangeSlug("vegetables");

        cat.Slug.Value.Should().Be("vegetables");
        cat.ModifiedOnUtc.Should().NotBeNull();
    }

    // ═══════════════════════════════════════════
    // SetDescription
    // ═══════════════════════════════════════════

    [Fact]
    public void SetDescription_ValidValue_Updates()
    {
        var cat = CreateValid();

        cat.SetDescription("Fresh fruits");

        cat.Description.Should().Be("Fresh fruits");
        cat.ModifiedOnUtc.Should().NotBeNull();
    }

    [Fact]
    public void SetDescription_Null_Clears()
    {
        var cat = CreateValid(description: "old");

        cat.SetDescription(null);

        cat.Description.Should().BeNull();
    }

    [Fact]
    public void SetDescription_TooLong_Throws()
    {
        var cat = CreateValid();

        var act = () => cat.SetDescription(new string('a', 2001));

        act.Should().Throw<ValidationException>();
    }

    // ═══════════════════════════════════════════
    // SetSeo
    // ═══════════════════════════════════════════

    [Fact]
    public void SetSeo_UpdatesSeoMeta()
    {
        var cat = CreateValid();

        cat.SetSeo("Fruits SEO", "Buy fresh fruits");

        cat.Seo.MetaTitle.Should().Be("Fruits SEO");
        cat.Seo.MetaDescription.Should().Be("Buy fresh fruits");
    }

    // ═══════════════════════════════════════════
    // SetImage
    // ═══════════════════════════════════════════

    [Fact]
    public void SetImage_ValidUrl_Updates()
    {
        var cat = CreateValid();

        cat.SetImage("https://img/new.png");

        cat.ImageUrl.Should().Be("https://img/new.png");
    }

    [Fact]
    public void SetImage_Null_Clears()
    {
        var cat = CreateValid(imageUrl: "https://img/old.png");

        cat.SetImage(null);

        cat.ImageUrl.Should().BeNull();
    }

    // ═══════════════════════════════════════════
    // SetIcon
    // ═══════════════════════════════════════════

    [Fact]
    public void SetIcon_ValidValue_Updates()
    {
        var cat = CreateValid();

        cat.SetIcon("carrot");

        cat.IconName.Should().Be("carrot");
    }

    [Fact]
    public void SetIcon_Null_Clears()
    {
        var cat = CreateValid(iconName: "old");

        cat.SetIcon(null);

        cat.IconName.Should().BeNull();
    }

    [Fact]
    public void SetIcon_TooLong_Throws()
    {
        var cat = CreateValid();

        var act = () => cat.SetIcon(new string('x', 51));

        act.Should().Throw<ValidationException>();
    }

    // ═══════════════════════════════════════════
    // SetParent
    // ═══════════════════════════════════════════

    [Fact]
    public void SetParent_ValidId_Updates()
    {
        var cat = CreateValid();
        var parentId = Guid.NewGuid();

        cat.SetParent(parentId);

        cat.ParentCategoryId.Should().Be(parentId);
    }

    [Fact]
    public void SetParent_Null_ClearsParent()
    {
        var cat = CreateValid(parentCategoryId: Guid.NewGuid());

        cat.SetParent(null);

        cat.ParentCategoryId.Should().BeNull();
    }

    // ═══════════════════════════════════════════
    // Reorder
    // ═══════════════════════════════════════════

    [Fact]
    public void Reorder_ValidSortOrder_Updates()
    {
        var cat = CreateValid();

        cat.Reorder(5);

        cat.SortOrder.Should().Be(5);
    }

    [Fact]
    public void Reorder_OutOfRange_Throws()
    {
        var cat = CreateValid();

        var act = () => cat.Reorder(-1);

        act.Should().Throw<ValidationException>();
    }

    // ═══════════════════════════════════════════
    // Activate / Deactivate
    // ═══════════════════════════════════════════

    [Fact]
    public void Activate_SetsIsActive()
    {
        var cat = CreateValid();
        cat.Deactivate();

        cat.Activate();

        cat.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Deactivate_ClearsIsActive()
    {
        var cat = CreateValid();

        cat.Deactivate();

        cat.IsActive.Should().BeFalse();
    }
}
