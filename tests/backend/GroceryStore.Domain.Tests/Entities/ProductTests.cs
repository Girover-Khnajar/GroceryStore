using FluentAssertions;
using GroceryStore.Domain.Entities;
using GroceryStore.Domain.Enums;
using GroceryStore.Domain.Exceptions;
using GroceryStore.Domain.Interfaces;
using GroceryStore.Domain.ValueObjects;

namespace GroceryStore.Domain.Tests.Entities;

public class ProductTests
{
    // ─── Helpers ───

    private static Product CreateValid(
        string name = "Fresh Apples",
        string slug = "fresh-apples",
        decimal price = 3.50m,
        string currency = "CHF",
        ProductUnit unit = ProductUnit.Kg,
        int sortOrder = 0,
        bool isFeatured = false,
        string? shortDesc = null,
        string? longDesc = null)
        => Product.Create(
            Guid.NewGuid(),
            name,
            slug,
            Money.Create(price, currency),
            unit,
            sortOrder,
            isFeatured,
            shortDesc,
            longDesc);

    private static ImageId NewImageId() => ImageId.CreateNew();

    // ═══════════════════════════════════════════
    // Create
    // ═══════════════════════════════════════════

    [Fact]
    public void Create_ValidInputs_ReturnsProduct()
    {
        var catId = Guid.NewGuid();
        var product = Product.Create(
            catId, "Apples", "apples", Money.Create(2, "CHF"), ProductUnit.Kg);

        product.CategoryId.Should().Be(catId);
        product.Name.Should().Be("Apples");
        product.Slug.Value.Should().Be("apples");
        product.Price.Amount.Should().Be(2m);
        product.Unit.Should().Be(ProductUnit.Kg);
        product.IsActive.Should().BeTrue();
        product.IsFeatured.Should().BeFalse();
        product.SortOrder.Should().Be(0);
    }

    [Fact]
    public void Create_TrimsName()
    {
        var p = CreateValid(name: "  Apples  ");

        p.Name.Should().Be("Apples");
    }

    [Fact]
    public void Create_ImplementsIAggregateRoot()
    {
        var p = CreateValid();

        p.Should().BeAssignableTo<IAggregateRoot>();
    }

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
        var act = () => CreateValid(name: new string('a', 161));

        act.Should().Throw<ValidationException>();
    }

    [Fact]
    public void Create_NullPrice_Throws()
    {
        var act = () => Product.Create(Guid.NewGuid(), "Test", "test", null!, ProductUnit.Piece);

        act.Should().Throw<ValidationException>();
    }

    [Fact]
    public void Create_SortOrderOutOfRange_Throws()
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
        var p = CreateValid();

        p.Rename("Oranges");

        p.Name.Should().Be("Oranges");
        p.ModifiedOnUtc.Should().NotBeNull();
    }

    [Fact]
    public void Rename_TrimsName()
    {
        var p = CreateValid();

        p.Rename("  Oranges  ");

        p.Name.Should().Be("Oranges");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Rename_NullOrEmpty_Throws(string? name)
    {
        var p = CreateValid();

        var act = () => p.Rename(name!);

        act.Should().Throw<ValidationException>();
    }

    [Fact]
    public void Rename_TooLong_Throws()
    {
        var p = CreateValid();

        var act = () => p.Rename(new string('x', 161));

        act.Should().Throw<ValidationException>();
    }

    // ═══════════════════════════════════════════
    // ChangeCategory
    // ═══════════════════════════════════════════

    [Fact]
    public void ChangeCategory_ValidId_Updates()
    {
        var p = CreateValid();
        var newCat = Guid.NewGuid();

        p.ChangeCategory(newCat);

        p.CategoryId.Should().Be(newCat);
        p.ModifiedOnUtc.Should().NotBeNull();
    }

    [Fact]
    public void ChangeCategory_EmptyGuid_Throws()
    {
        var p = CreateValid();

        var act = () => p.ChangeCategory(Guid.Empty);

        act.Should().Throw<ValidationException>();
    }

    // ═══════════════════════════════════════════
    // ChangeSlug
    // ═══════════════════════════════════════════

    [Fact]
    public void ChangeSlug_ValidSlug_Updates()
    {
        var p = CreateValid();

        p.ChangeSlug("new-slug");

        p.Slug.Value.Should().Be("new-slug");
        p.ModifiedOnUtc.Should().NotBeNull();
    }

    // ═══════════════════════════════════════════
    // ChangePrice
    // ═══════════════════════════════════════════

    [Fact]
    public void ChangePrice_ValidPrice_Updates()
    {
        var p = CreateValid();
        var newPrice = Money.Create(5, "EUR");

        p.ChangePrice(newPrice, ProductUnit.Piece);

        p.Price.Should().Be(newPrice);
        p.Unit.Should().Be(ProductUnit.Piece);
        p.ModifiedOnUtc.Should().NotBeNull();
    }

    [Fact]
    public void ChangePrice_NullPrice_Throws()
    {
        var p = CreateValid();

        var act = () => p.ChangePrice(null!, ProductUnit.Piece);

        act.Should().Throw<ValidationException>();
    }

    // ═══════════════════════════════════════════
    // SetDescriptions
    // ═══════════════════════════════════════════

    [Fact]
    public void SetDescriptions_ValidValues_Updates()
    {
        var p = CreateValid();

        p.SetDescriptions("Short", "Long description here");

        p.ShortDescription.Should().Be("Short");
        p.LongDescription.Should().Be("Long description here");
    }

    [Fact]
    public void SetDescriptions_NullValues_ClearsDescriptions()
    {
        var p = CreateValid(shortDesc: "old", longDesc: "old");

        p.SetDescriptions(null, null);

        p.ShortDescription.Should().BeNull();
        p.LongDescription.Should().BeNull();
    }

    // ═══════════════════════════════════════════
    // Reorder
    // ═══════════════════════════════════════════

    [Fact]
    public void Reorder_ValidSortOrder_Updates()
    {
        var p = CreateValid();

        p.Reorder(5);

        p.SortOrder.Should().Be(5);
    }

    [Fact]
    public void Reorder_OutOfRange_Throws()
    {
        var p = CreateValid();

        var act = () => p.Reorder(-1);

        act.Should().Throw<ValidationException>();
    }

    // ═══════════════════════════════════════════
    // Feature / Unfeature / Activate / Deactivate
    // ═══════════════════════════════════════════

    [Fact]
    public void Feature_SetsIsFeatured()
    {
        var p = CreateValid();

        p.Feature();

        p.IsFeatured.Should().BeTrue();
    }

    [Fact]
    public void Unfeature_ClearsIsFeatured()
    {
        var p = CreateValid(isFeatured: true);

        p.Unfeature();

        p.IsFeatured.Should().BeFalse();
    }

    [Fact]
    public void Activate_SetsIsActive()
    {
        var p = CreateValid();
        p.Deactivate();

        p.Activate();

        p.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Deactivate_ClearsIsActive()
    {
        var p = CreateValid();

        p.Deactivate();

        p.IsActive.Should().BeFalse();
    }

    // ═══════════════════════════════════════════
    // SetSeo
    // ═══════════════════════════════════════════

    [Fact]
    public void SetSeo_UpdatesSeoMeta()
    {
        var p = CreateValid();

        p.SetSeo("Title", "Description");

        p.Seo.MetaTitle.Should().Be("Title");
        p.Seo.MetaDescription.Should().Be("Description");
    }

    // ═══════════════════════════════════════════
    // AttachImage
    // ═══════════════════════════════════════════

    [Fact]
    public void AttachImage_FirstImage_AutoPrimary()
    {
        var p = CreateValid();
        var imgId = NewImageId();

        p.AttachImage(imgId);

        p.ImageRefs.Should().ContainSingle();
        p.PrimaryImageRef.Should().NotBeNull();
        p.PrimaryImageRef!.ImageId.Should().Be(imgId);
    }

    [Fact]
    public void AttachImage_SecondImageNotPrimary_KeepsExistingPrimary()
    {
        var p = CreateValid();
        var first = NewImageId();
        var second = NewImageId();

        p.AttachImage(first);
        p.AttachImage(second);

        p.ImageRefs.Should().HaveCount(2);
        p.PrimaryImageRef!.ImageId.Should().Be(first);
    }

    [Fact]
    public void AttachImage_MakePrimary_SwitchesPrimary()
    {
        var p = CreateValid();
        var first = NewImageId();
        var second = NewImageId();

        p.AttachImage(first);
        p.AttachImage(second, makePrimary: true);

        p.PrimaryImageRef!.ImageId.Should().Be(second);
        p.ImageRefs.Count(r => r.IsPrimary).Should().Be(1);
    }

    [Fact]
    public void AttachImage_DuplicateImageId_Throws()
    {
        var p = CreateValid();
        var imgId = NewImageId();
        p.AttachImage(imgId);

        var act = () => p.AttachImage(imgId);

        act.Should().Throw<ValidationException>()
           .WithMessage("*already attached*");
    }

    [Fact]
    public void AttachImage_NullImageId_Throws()
    {
        var p = CreateValid();

        var act = () => p.AttachImage(null!);

        act.Should().Throw<ValidationException>();
    }

    // ═══════════════════════════════════════════
    // SetPrimaryImage
    // ═══════════════════════════════════════════

    [Fact]
    public void SetPrimaryImage_ExistingImage_SetsPrimary()
    {
        var p = CreateValid();
        var first = NewImageId();
        var second = NewImageId();
        p.AttachImage(first);
        p.AttachImage(second);

        p.SetPrimaryImage(second);

        p.PrimaryImageRef!.ImageId.Should().Be(second);
        p.ImageRefs.Count(r => r.IsPrimary).Should().Be(1);
    }

    [Fact]
    public void SetPrimaryImage_AlreadyPrimary_NoOp()
    {
        var p = CreateValid();
        var imgId = NewImageId();
        p.AttachImage(imgId); // Auto-primary

        p.SetPrimaryImage(imgId); // No-op

        p.PrimaryImageRef!.ImageId.Should().Be(imgId);
    }

    [Fact]
    public void SetPrimaryImage_NotAttached_Throws()
    {
        var p = CreateValid();
        p.AttachImage(NewImageId());

        var act = () => p.SetPrimaryImage(NewImageId());

        act.Should().Throw<ValidationException>()
           .WithMessage("*not attached*");
    }

    [Fact]
    public void SetPrimaryImage_NoImages_Throws()
    {
        var p = CreateValid();

        var act = () => p.SetPrimaryImage(NewImageId());

        act.Should().Throw<ValidationException>()
           .WithMessage("*No images*");
    }

    // ═══════════════════════════════════════════
    // RemoveImage
    // ═══════════════════════════════════════════

    [Fact]
    public void RemoveImage_Existing_RemovesIt()
    {
        var p = CreateValid();
        var imgId = NewImageId();
        p.AttachImage(imgId);

        p.RemoveImage(imgId);

        p.ImageRefs.Should().BeEmpty();
    }

    [Fact]
    public void RemoveImage_Primary_PromotesNextImage()
    {
        var p = CreateValid();
        var first = NewImageId();
        var second = NewImageId();
        p.AttachImage(first);
        p.AttachImage(second);

        p.RemoveImage(first); // first was primary

        p.PrimaryImageRef.Should().NotBeNull();
        p.PrimaryImageRef!.ImageId.Should().Be(second);
    }

    [Fact]
    public void RemoveImage_NotAttached_IsIdempotent()
    {
        var p = CreateValid();

        var act = () => p.RemoveImage(NewImageId());

        act.Should().NotThrow();
    }

    // ═══════════════════════════════════════════
    // HasImage
    // ═══════════════════════════════════════════

    [Fact]
    public void HasImage_Attached_ReturnsTrue()
    {
        var p = CreateValid();
        var imgId = NewImageId();
        p.AttachImage(imgId);

        p.HasImage(imgId).Should().BeTrue();
    }

    [Fact]
    public void HasImage_NotAttached_ReturnsFalse()
    {
        var p = CreateValid();

        p.HasImage(NewImageId()).Should().BeFalse();
    }

    // ═══════════════════════════════════════════
    // SetIdentifiers
    // ═══════════════════════════════════════════

    [Fact]
    public void SetIdentifiers_ValidValues_Updates()
    {
        var p = CreateValid();

        p.SetIdentifiers("SKU-001", "1234567890123");

        p.Sku.Should().Be("SKU-001");
        p.Barcode.Should().Be("1234567890123");
    }

    [Fact]
    public void SetIdentifiers_NullValues_Clears()
    {
        var p = CreateValid();
        p.SetIdentifiers("SKU", "BAR");

        p.SetIdentifiers(null, null);

        p.Sku.Should().BeNull();
        p.Barcode.Should().BeNull();
    }

    // ═══════════════════════════════════════════
    // SetBrandAndOrigin
    // ═══════════════════════════════════════════

    [Fact]
    public void SetBrandAndOrigin_ValidValues_Updates()
    {
        var p = CreateValid();

        p.SetBrandAndOrigin("Migros", "CH");

        p.Brand.Should().Be("Migros");
        p.OriginCountryCode.Should().Be("CH");
    }

    [Fact]
    public void SetBrandAndOrigin_UppercasesCountryCode()
    {
        var p = CreateValid();

        p.SetBrandAndOrigin(null, "ch");

        p.OriginCountryCode.Should().Be("CH");
    }

    [Fact]
    public void SetBrandAndOrigin_InvalidCountryCodeLength_Throws()
    {
        var p = CreateValid();

        var act = () => p.SetBrandAndOrigin(null, "CHE");

        act.Should().Throw<ValidationException>();
    }

    [Fact]
    public void SetBrandAndOrigin_NullCountryCode_ClearsIt()
    {
        var p = CreateValid();
        p.SetBrandAndOrigin(null, "CH");

        p.SetBrandAndOrigin(null, null);

        p.OriginCountryCode.Should().BeNull();
    }

    // ═══════════════════════════════════════════
    // SetDietaryFlags
    // ═══════════════════════════════════════════

    [Fact]
    public void SetDietaryFlags_Updates()
    {
        var p = CreateValid();

        p.SetDietaryFlags(true, false, true);

        p.IsOrganic.Should().BeTrue();
        p.IsHalal.Should().BeFalse();
        p.IsVegan.Should().BeTrue();
    }

    // ═══════════════════════════════════════════
    // SetIngredientsAndNutrition
    // ═══════════════════════════════════════════

    [Fact]
    public void SetIngredientsAndNutrition_ValidValues_Updates()
    {
        var p = CreateValid();
        var nutrition = NutritionFacts.Create(100, 5, 20, 3, 0.5m);

        p.SetIngredientsAndNutrition("Apples, Sugar", nutrition, StorageCondition.Refrigerated);

        p.Ingredients.Should().Be("Apples, Sugar");
        p.Nutrition.Should().Be(nutrition);
        p.Storage.Should().Be(StorageCondition.Refrigerated);
    }

    [Fact]
    public void SetIngredientsAndNutrition_NullValues_Clears()
    {
        var p = CreateValid();

        p.SetIngredientsAndNutrition(null, null, StorageCondition.None);

        p.Ingredients.Should().BeNull();
        p.Nutrition.Should().BeNull();
    }

    // ═══════════════════════════════════════════
    // SetNetWeight
    // ═══════════════════════════════════════════

    [Fact]
    public void SetNetWeight_ValidValues_Updates()
    {
        var p = CreateValid();

        p.SetNetWeight(0.5m, ProductUnit.Kg);

        p.NetWeight.Should().Be(0.5m);
        p.NetWeightUnit.Should().Be(ProductUnit.Kg);
    }

    [Fact]
    public void SetNetWeight_Null_ClearsBoth()
    {
        var p = CreateValid();
        p.SetNetWeight(1m, ProductUnit.Kg);

        p.SetNetWeight(null, null);

        p.NetWeight.Should().BeNull();
        p.NetWeightUnit.Should().BeNull();
    }

    [Fact]
    public void SetNetWeight_NegativeValue_Throws()
    {
        var p = CreateValid();

        var act = () => p.SetNetWeight(-1, ProductUnit.Kg);

        act.Should().Throw<ValidationException>();
    }

    [Fact]
    public void SetNetWeight_InvalidUnit_Throws()
    {
        var p = CreateValid();

        var act = () => p.SetNetWeight(1, ProductUnit.Piece);

        act.Should().Throw<ValidationException>()
           .WithMessage("*Kg/Gram/Liter/Ml*");
    }

    // ═══════════════════════════════════════════
    // Tags
    // ═══════════════════════════════════════════

    [Fact]
    public void AddTag_ValidTag_AddsIt()
    {
        var p = CreateValid();

        p.AddTag("organic");

        p.Tags.Should().ContainSingle().Which.Should().Be("organic");
    }

    [Fact]
    public void AddTag_NormalizesToLowerCase()
    {
        var p = CreateValid();

        p.AddTag("ORGANIC");

        p.Tags.Should().Contain("organic");
    }

    [Fact]
    public void AddTag_Duplicate_IsIdempotent()
    {
        var p = CreateValid();
        p.AddTag("organic");

        p.AddTag("organic");

        p.Tags.Should().ContainSingle();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void AddTag_NullOrEmpty_Throws(string? tag)
    {
        var p = CreateValid();

        var act = () => p.AddTag(tag!);

        act.Should().Throw<ValidationException>();
    }

    [Fact]
    public void AddTag_TooLong_Throws()
    {
        var p = CreateValid();

        var act = () => p.AddTag(new string('x', 41));

        act.Should().Throw<ValidationException>();
    }

    [Fact]
    public void RemoveTag_Existing_RemovesIt()
    {
        var p = CreateValid();
        p.AddTag("bio");

        p.RemoveTag("bio");

        p.Tags.Should().BeEmpty();
    }

    [Fact]
    public void RemoveTag_NotExisting_NoOp()
    {
        var p = CreateValid();

        var act = () => p.RemoveTag("nonexistent");

        act.Should().NotThrow();
        p.Tags.Should().BeEmpty();
    }

    // ═══════════════════════════════════════════
    // Allergens
    // ═══════════════════════════════════════════

    [Fact]
    public void AddAllergen_ValidAllergen_AddsIt()
    {
        var p = CreateValid();

        p.AddAllergen("Gluten");

        p.Allergens.Should().ContainSingle().Which.Should().Be("gluten");
    }

    [Fact]
    public void AddAllergen_Duplicate_IsIdempotent()
    {
        var p = CreateValid();
        p.AddAllergen("gluten");

        p.AddAllergen("gluten");

        p.Allergens.Should().ContainSingle();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void AddAllergen_NullOrEmpty_Throws(string? allergen)
    {
        var p = CreateValid();

        var act = () => p.AddAllergen(allergen!);

        act.Should().Throw<ValidationException>();
    }

    [Fact]
    public void AddAllergen_TooLong_Throws()
    {
        var p = CreateValid();

        var act = () => p.AddAllergen(new string('x', 61));

        act.Should().Throw<ValidationException>();
    }

    [Fact]
    public void RemoveAllergen_Existing_RemovesIt()
    {
        var p = CreateValid();
        p.AddAllergen("nuts");

        p.RemoveAllergen("nuts");

        p.Allergens.Should().BeEmpty();
    }
}
