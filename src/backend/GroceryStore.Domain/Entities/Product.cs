using GroceryStore.Domain.Common.DomainEvents;
using GroceryStore.Domain.Entities.Catalog;
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
    private readonly List<ProductImageRef> _imageRefs = new( );
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

    // Media (cross-aggregate references to ImageAsset by ImageId)
    public IReadOnlyCollection<ProductImageRef> ImageRefs => _imageRefs.AsReadOnly();

    /// <summary>
    /// Returns the primary image reference, or null if no images are attached.
    /// </summary>
    public ProductImageRef? PrimaryImageRef => _imageRefs.FirstOrDefault(r => r.IsPrimary);

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
        ValidationException.ThrowIfNull(price);
        ValidationException.ThrowIfOutOfRange(sortOrder, 0, 10_000);
        ValidationException.ThrowIfNullOrWhiteSpace(name);

        ValidationException.ThrowIfTooLong(name, maxLen: 160);
        if (!string.IsNullOrWhiteSpace(shortDescription))
            ValidationException.ThrowIfNullOrWhiteSpace(shortDescription);

            ValidationException.ThrowIfTooLong(shortDescription, maxLen: 300);
        if (!string.IsNullOrWhiteSpace(longDescription))
            ValidationException.ThrowIfNullOrWhiteSpace(longDescription);

            ValidationException.ThrowIfTooLong(longDescription, maxLen: 5000);

        var product = new Product
        {
            CategoryId = categoryId,
            Name = name.Trim(),
            Slug = Slug.Create(slug),
            Price = price,
            Unit = unit,
            SortOrder = sortOrder,
            IsFeatured = isFeatured,
            ShortDescription = shortDescription?.Trim(),
            LongDescription = longDescription?.Trim(),
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
        ValidationException.ThrowIfNullOrWhiteSpace(name);

        ValidationException.ThrowIfTooLong(name, maxLen: 160);
        Name = name.Trim();
        Touch();
    }

    public void ChangeSlug(string slug)
    {
        Slug = Slug.Create(slug);
        Touch( );
    }

    public void ChangePrice(Money price,ProductUnit unit)
    {
        ValidationException.ThrowIfNull(price);
        Price = price;
        Unit = unit;
        Touch( );
    }

    public void SetDescriptions(string? shortDescription,string? longDescription)
    {
        if (!string.IsNullOrWhiteSpace(shortDescription))
            ValidationException.ThrowIfNullOrWhiteSpace(shortDescription);

            ValidationException.ThrowIfTooLong(shortDescription, maxLen: 300);
        if (!string.IsNullOrWhiteSpace(longDescription))
            ValidationException.ThrowIfNullOrWhiteSpace(longDescription);

            ValidationException.ThrowIfTooLong(longDescription, maxLen: 5000);

        ShortDescription = shortDescription?.Trim();
        LongDescription = longDescription?.Trim();
        Touch();
    }

    public void SetSeo(string? metaTitle,string? metaDescription)
    {
        Seo = SeoMeta.Create(metaTitle,metaDescription);
        Touch( );
    }

    public void Reorder(int sortOrder)
    {
        ValidationException.ThrowIfOutOfRange(sortOrder, 0, 10_000);
        SortOrder = sortOrder;
        Touch( );
    }

    public void Feature() { IsFeatured = true; Touch( ); }
    public void Unfeature() { IsFeatured = false; Touch( ); }

    public void Activate() { IsActive = true; Touch( ); }
    public void Deactivate() { IsActive = false; Touch( ); }

    // ----------------------------------------------
    // Media behavior (cross-aggregate ImageId refs)
    // ----------------------------------------------

    /// <summary>
    /// Attach an existing ImageAsset (by ImageId) to this product.
    /// The application layer must verify the ImageId exists before calling this.
    /// </summary>
    public void AttachImage(ImageId imageId, bool makePrimary = false, int sortOrder = 0, string? altText = null)
    {
        ValidationException.ThrowIfNull(imageId);

        if (_imageRefs.Any(r => r.ImageId == imageId))
            throw new ValidationException($"Image '{imageId}' is already attached to this product.");

        if (makePrimary)
            UnsetPrimaryInternal();

        var imageRef = ProductImageRef.Create(imageId, makePrimary, sortOrder, altText);
        _imageRefs.Add(imageRef);

        // If this is the first image, automatically make it primary
        if (_imageRefs.Count == 1 && !makePrimary)
        {
            imageRef.MarkAsPrimary();
        }

        Touch();
    }

    /// <summary>
    /// Set an already-attached image as the primary image.
    /// </summary>
    public void SetPrimaryImage(ImageId imageId)
    {
        ValidationException.ThrowIfNull(imageId);

        if (_imageRefs.Count == 0)
            throw new ValidationException("No images are attached to set as primary.");

        var target = _imageRefs.FirstOrDefault(r => r.ImageId == imageId)
            ?? throw new ValidationException($"Image '{imageId}' is not attached to this product.");

        if (target.IsPrimary)
            return; // Already primary, no-op

        UnsetPrimaryInternal();
        target.MarkAsPrimary();

        Touch();
    }

    /// <summary>
    /// Remove an image reference from this product.
    /// If the removed image was primary, the first remaining image becomes primary.
    /// </summary>
    public void RemoveImage(ImageId imageId)
    {
        ValidationException.ThrowIfNull(imageId);

        var target = _imageRefs.FirstOrDefault(r => r.ImageId == imageId);
        if (target is null)
            return; // Idempotent: not attached = nothing to do

        var wasPrimary = target.IsPrimary;
        _imageRefs.Remove(target);

        if (wasPrimary)
            EnsurePrimaryInternal();

        Touch();
    }

    /// <summary>
    /// Check whether a given image is attached to this product.
    /// </summary>
    public bool HasImage(ImageId imageId) => _imageRefs.Any(r => r.ImageId == imageId);

    private void UnsetPrimaryInternal()
    {
        foreach (var img in _imageRefs)
        {
            if (img.IsPrimary)
                img.UnmarkAsPrimary();
        }
    }

    private void EnsurePrimaryInternal()
    {
        if (_imageRefs.Count == 0)
            return;

        if (_imageRefs.Any(r => r.IsPrimary))
            return;

        // Promote the first image (by sort order) to primary
        var first = _imageRefs.OrderBy(r => r.SortOrder).First();
        first.MarkAsPrimary();
    }

    // ---------------------------
    // Optional catalog attributes
    // ---------------------------

    public void SetIdentifiers(string? sku,string? barcode)
    {
        if (!string.IsNullOrWhiteSpace(sku))
            ValidationException.ThrowIfNullOrWhiteSpace(sku);

            ValidationException.ThrowIfTooLong(sku, maxLen: 60);
        if (!string.IsNullOrWhiteSpace(barcode))
            ValidationException.ThrowIfNullOrWhiteSpace(barcode);

            ValidationException.ThrowIfTooLong(barcode, maxLen: 40);

        Sku = sku?.Trim();
        Barcode = barcode?.Trim();
        Touch();
    }

    public void SetBrandAndOrigin(string? brand,string? originCountryCode)
    {
        if (!string.IsNullOrWhiteSpace(brand))
            ValidationException.ThrowIfNullOrWhiteSpace(brand);

            ValidationException.ThrowIfTooLong(brand, maxLen: 80);
        Brand = brand?.Trim();

        if (string.IsNullOrWhiteSpace(originCountryCode))
        {
            OriginCountryCode = null;
        }
        else
        {
            ValidationException.ThrowIfNullOrWhiteSpace(originCountryCode);

            ValidationException.ThrowIfTooLong(originCountryCode, maxLen: 2);
            var code = originCountryCode.Trim().ToUpperInvariant();
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
        if (!string.IsNullOrWhiteSpace(ingredients))
            ValidationException.ThrowIfNullOrWhiteSpace(ingredients);

            ValidationException.ThrowIfTooLong(ingredients, maxLen: 4000);
        Ingredients = ingredients?.Trim();
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

        ValidationException.ThrowIfNegative(netWeight.Value);

        // Only allow meaningful units for weight/volume
        if (netWeightUnit is not (ProductUnit.Kg or ProductUnit.Gram or ProductUnit.Liter or ProductUnit.Ml))
            throw new ValidationException("NetWeightUnit must be Kg/Gram/Liter/Ml.");

        NetWeight = netWeight;
        NetWeightUnit = netWeightUnit;
        Touch( );
    }

    public void AddTag(string tag)
    {
        ValidationException.ThrowIfNullOrWhiteSpace(tag);

        ValidationException.ThrowIfTooLong(tag, maxLen: 40);
        tag = tag.Trim().ToLowerInvariant();

        if (_tags.Contains(tag))
            return;

        _tags.Add(tag);
        Touch( );
    }

    public void RemoveTag(string tag)
    {
        ValidationException.ThrowIfNullOrWhiteSpace(tag);

        ValidationException.ThrowIfTooLong(tag, maxLen: 40);
        tag = tag.Trim().ToLowerInvariant();
        _tags.Remove(tag);
        Touch( );
    }

    public void AddAllergen(string allergen)
    {
        ValidationException.ThrowIfNullOrWhiteSpace(allergen);

        ValidationException.ThrowIfTooLong(allergen, maxLen: 60);
        allergen = allergen.Trim().ToLowerInvariant();

        if (_allergens.Contains(allergen))
            return;

        _allergens.Add(allergen);
        Touch( );
    }

    public void RemoveAllergen(string allergen)
    {
        ValidationException.ThrowIfNullOrWhiteSpace(allergen);

        ValidationException.ThrowIfTooLong(allergen, maxLen: 60);
        allergen = allergen.Trim().ToLowerInvariant();
        _allergens.Remove(allergen);
        Touch( );
    }
}