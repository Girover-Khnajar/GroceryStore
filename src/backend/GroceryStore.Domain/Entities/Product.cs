using GroceryStore.Domain.Common;
using GroceryStore.Domain.Common.DomainEvents;
using GroceryStore.Domain.Enums;
using GroceryStore.Domain.Exceptions;
using GroceryStore.Domain.Interfaces;
using GroceryStore.Domain.ValueObjects;

namespace GroceryStore.Domain.Entities;

/// <summary>
/// Product aggregate root.
/// </summary>
public sealed class Product : AuditableEntity, IAggregateRoot
{
    private readonly List<ProductImage> _images = new( );
    private readonly List<string> _tags = new( );
    private readonly List<string> _allergens = new( );

    private Product() { } // For ORM

    // Identity/Relationship
    public Guid CategoryId { get; private set; }

    // Core fields
    public string Name { get; private set; } = string.Empty;
    public string? ShortDescription { get; private set; }
    public string? LongDescription { get; private set; }

    // Pricing
    public Money Price { get; private set; } = Money.Create(0,"CHF");
    public ProductUnit Unit { get; private set; } = ProductUnit.Piece;

    // Visibility/marketing
    public bool IsActive { get; private set; } = true;
    public bool IsFeatured { get; private set; }
    public int SortOrder { get; private set; }

    // SEO
    public Slug Slug { get; private set; } = Slug.Create("default");
    public SeoMeta Seo { get; private set; } = SeoMeta.Create(null,null);

    // Media
    public IReadOnlyList<ProductImage> Images => _images;

    // Optional commerce-like fields (still useful for catalog)
    public string? Sku { get; private set; }
    public string? Barcode { get; private set; } // EAN/UPC
    public string? Brand { get; private set; }
    public string? OriginCountryCode { get; private set; } // e.g. "CH", "IT"
    public bool? IsOrganic { get; private set; }
    public bool? IsHalal { get; private set; }
    public bool? IsVegan { get; private set; }

    // Optional nutrition / ingredients
    public string? Ingredients { get; private set; }
    public NutritionFacts? Nutrition { get; private set; }
    public StorageCondition Storage { get; private set; } = StorageCondition.None;

    // Optional measures for display (not stock management)
    public decimal? NetWeight { get; private set; } // e.g. 0.5
    public ProductUnit? NetWeightUnit { get; private set; } // e.g. Kg/Gram

    // Tags/Allergens
    public IReadOnlyList<string> Tags => _tags;
    public IReadOnlyList<string> Allergens => _allergens;

    public static Product Create(
        Guid categoryId,
        string name,
        string slug,
        Money price,
        ProductUnit unit,
        int sortOrder = 0,
        bool isFeatured = false,
        string? shortDescription = null,
        string? longDescription = null,
        SeoMeta? seo = null)
    {
        Guard.NotNull(price,nameof(price));
        Guard.InRange(sortOrder,nameof(sortOrder),0,10_000);

        var product = new Product
        {
            CategoryId = categoryId,
            Name = Guard.NotEmpty(name,nameof(name),maxLen: 160),
            Slug = Slug.Create(slug),
            Price = price,
            Unit = unit,
            SortOrder = sortOrder,
            IsFeatured = isFeatured,
            ShortDescription = string.IsNullOrWhiteSpace(shortDescription) ? null : Guard.NotEmpty(shortDescription,nameof(shortDescription),maxLen: 300),
            LongDescription = string.IsNullOrWhiteSpace(longDescription) ? null : Guard.NotEmpty(longDescription,nameof(longDescription),maxLen: 5000),
            Seo = seo ?? SeoMeta.Create(null,null)
        };

        return product;
    }

    // ---------------------------
    // Core behavior
    // ---------------------------

    public void ChangeCategory(Guid categoryId)
    {
        if (categoryId == Guid.Empty)
            throw new ValidationException("CategoryId is required.");
        CategoryId = categoryId;
        Touch( );
    }

    public void Rename(string name)
    {
        Name = Guard.NotEmpty(name,nameof(name),maxLen: 160);
        Touch( );
    }

    public void ChangeSlug(string slug)
    {
        Slug = Slug.Create(slug);
        Touch( );
    }

    public void ChangePrice(Money price,ProductUnit unit)
    {
        Guard.NotNull(price,nameof(price));
        Price = price;
        Unit = unit;
        Touch( );
    }

    public void SetDescriptions(string? shortDescription,string? longDescription)
    {
        ShortDescription = string.IsNullOrWhiteSpace(shortDescription)
            ? null
            : Guard.NotEmpty(shortDescription,nameof(shortDescription),maxLen: 300);

        LongDescription = string.IsNullOrWhiteSpace(longDescription)
            ? null
            : Guard.NotEmpty(longDescription,nameof(longDescription),maxLen: 5000);

        Touch( );
    }

    public void SetSeo(string? metaTitle,string? metaDescription)
    {
        Seo = SeoMeta.Create(metaTitle,metaDescription);
        Touch( );
    }

    public void Reorder(int sortOrder)
    {
        Guard.InRange(sortOrder,nameof(sortOrder),0,10_000);
        SortOrder = sortOrder;
        Touch( );
    }

    public void Feature() { IsFeatured = true; Touch( ); }
    public void Unfeature() { IsFeatured = false; Touch( ); }

    public void Activate() { IsActive = true; Touch( ); }
    public void Deactivate() { IsActive = false; Touch( ); }

    // ---------------------------
    // Media behavior (Images)
    // ---------------------------

    public void AddImage(string url,string? altText = null,bool isPrimary = false,int sortOrder = 0)
    {
        var image = ProductImage.Create(url,altText,isPrimary,sortOrder);

        if (isPrimary)
            UnsetPrimaryImageInternal( );

        _images.Add(image);
        NormalizeImageOrdersInternal( );
        Touch( );
    }

    public void RemoveImage(string url)
    {
        url = Guard.NotEmpty(url,nameof(url),maxLen: 500);

        var removed = _images.RemoveAll(x => x.Url == url);
        if (removed == 0)
            return;

        // Ensure we still have a primary if images remain
        EnsurePrimaryImageInternal( );

        Touch( );
    }

    public void SetPrimaryImage(string url)
    {
        url = Guard.NotEmpty(url,nameof(url),maxLen: 500);

        if (_images.Count == 0)
            throw new ValidationException("No images exist to set as primary.");

        var idx = _images.FindIndex(x => x.Url == url);
        if (idx < 0)
            throw new ValidationException("Image not found.");

        var selected = _images[idx];

        UnsetPrimaryImageInternal( );
        _images[idx] = ProductImage.Create(selected.Url,selected.AltText,isPrimary: true,selected.SortOrder);

        Touch( );
    }

    private void UnsetPrimaryImageInternal()
    {
        for (var i = 0; i < _images.Count; i++)
        {
            var img = _images[i];
            if (img.IsPrimary)
                _images[i] = ProductImage.Create(img.Url,img.AltText,isPrimary: false,img.SortOrder);
        }
    }

    private void EnsurePrimaryImageInternal()
    {
        if (_images.Count == 0)
            return;
        if (_images.Any(x => x.IsPrimary))
            return;

        // make the first one primary
        var first = _images.OrderBy(x => x.SortOrder).First( );
        SetPrimaryImage(first.Url);
    }

    private void NormalizeImageOrdersInternal()
    {
        // Keep stable ordering; optional normalization
        // (You can remove this if you prefer manual SortOrder only.)
        var ordered = _images.OrderBy(x => x.SortOrder).ThenBy(x => x.Url).ToList( );
        _images.Clear( );
        _images.AddRange(ordered);
        EnsurePrimaryImageInternal( );
    }

    // ---------------------------
    // Optional catalog attributes
    // ---------------------------

    public void SetIdentifiers(string? sku,string? barcode)
    {
        Sku = string.IsNullOrWhiteSpace(sku) ? null : Guard.NotEmpty(sku,nameof(sku),maxLen: 60);
        Barcode = string.IsNullOrWhiteSpace(barcode) ? null : Guard.NotEmpty(barcode,nameof(barcode),maxLen: 40);
        Touch( );
    }

    public void SetBrandAndOrigin(string? brand,string? originCountryCode)
    {
        Brand = string.IsNullOrWhiteSpace(brand) ? null : Guard.NotEmpty(brand,nameof(brand),maxLen: 80);

        if (string.IsNullOrWhiteSpace(originCountryCode))
        {
            OriginCountryCode = null;
        }
        else
        {
            var code = Guard.NotEmpty(originCountryCode,nameof(originCountryCode),maxLen: 2).ToUpperInvariant( );
            if (code.Length != 2)
                throw new ValidationException("OriginCountryCode must be 2 letters.");
            OriginCountryCode = code;
        }

        Touch( );
    }

    public void SetDietaryFlags(bool? isOrganic,bool? isHalal,bool? isVegan)
    {
        IsOrganic = isOrganic;
        IsHalal = isHalal;
        IsVegan = isVegan;
        Touch( );
    }

    public void SetIngredientsAndNutrition(string? ingredients,NutritionFacts? nutrition,StorageCondition storage)
    {
        Ingredients = string.IsNullOrWhiteSpace(ingredients) ? null : Guard.NotEmpty(ingredients,nameof(ingredients),maxLen: 4000);
        Nutrition = nutrition;
        Storage = storage;
        Touch( );
    }

    public void SetNetWeight(decimal? netWeight,ProductUnit? netWeightUnit)
    {
        if (netWeight is null)
        {
            NetWeight = null;
            NetWeightUnit = null;
            Touch( );
            return;
        }

        Guard.NonNegative(netWeight.Value,nameof(netWeight));

        // Only allow meaningful units for weight/volume
        if (netWeightUnit is not (ProductUnit.Kg or ProductUnit.Gram or ProductUnit.Liter or ProductUnit.Ml))
            throw new ValidationException("NetWeightUnit must be Kg/Gram/Liter/Ml.");

        NetWeight = netWeight;
        NetWeightUnit = netWeightUnit;
        Touch( );
    }

    public void AddTag(string tag)
    {
        tag = Guard.NotEmpty(tag,nameof(tag),maxLen: 40).ToLowerInvariant( );

        if (_tags.Contains(tag))
            return;

        _tags.Add(tag);
        Touch( );
    }

    public void RemoveTag(string tag)
    {
        tag = Guard.NotEmpty(tag,nameof(tag),maxLen: 40).ToLowerInvariant( );
        _tags.Remove(tag);
        Touch( );
    }

    public void AddAllergen(string allergen)
    {
        allergen = Guard.NotEmpty(allergen,nameof(allergen),maxLen: 60).ToLowerInvariant( );

        if (_allergens.Contains(allergen))
            return;

        _allergens.Add(allergen);
        Touch( );
    }

    public void RemoveAllergen(string allergen)
    {
        allergen = Guard.NotEmpty(allergen,nameof(allergen),maxLen: 60).ToLowerInvariant( );
        _allergens.Remove(allergen);
        Touch( );
    }
}