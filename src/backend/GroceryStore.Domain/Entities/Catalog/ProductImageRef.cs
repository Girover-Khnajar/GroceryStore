using GroceryStore.Domain.Common.DomainEvents;
using GroceryStore.Domain.Exceptions;
using GroceryStore.Domain.ValueObjects;

namespace GroceryStore.Domain.Entities.Catalog;

/// <summary>
/// Internal entity owned by the Product aggregate.
/// Represents a cross-aggregate reference from Product → ImageAsset via ImageId.
/// This is NOT the ImageAsset itself — it is a lightweight reference stored inside the Catalog context.
/// </summary>
public sealed class ProductImageRef : Entity
{
    private ProductImageRef() { } // For ORM

    /// <summary>
    /// The ImageAsset being referenced (cross-aggregate by ID only).
    /// </summary>
    public ImageId ImageId { get; private set; } = null!;

    /// <summary>
    /// Whether this is the primary/hero image for the product.
    /// </summary>
    public bool IsPrimary { get; private set; }

    /// <summary>
    /// Display sort order.
    /// </summary>
    public int SortOrder { get; private set; }

    /// <summary>
    /// Optional alt text for accessibility.
    /// </summary>
    public string? AltText { get; private set; }

    internal static ProductImageRef Create(ImageId imageId, bool isPrimary, int sortOrder, string? altText = null)
    {
        ValidationException.ThrowIfNull(imageId);
        ValidationException.ThrowIfOutOfRange(sortOrder, 0, 10_000);

        if (!string.IsNullOrWhiteSpace(altText))
            ValidationException.ThrowIfNullOrWhiteSpace(altText);

            ValidationException.ThrowIfTooLong(altText, maxLen: 200);

        return new ProductImageRef
        {
            // Important for EF Core: ProductImageRef.Id is configured as ValueGeneratedOnAdd.
            // If we pre-set a non-default Guid here, EF may assume the entity already exists
            // when it's added via the Product navigation, leading to concurrency exceptions.
            Id = Guid.Empty,
            ImageId = imageId,
            IsPrimary = isPrimary,
            SortOrder = sortOrder,
            AltText = altText?.Trim()
        };
    }

    internal void MarkAsPrimary() => IsPrimary = true;

    internal void UnmarkAsPrimary() => IsPrimary = false;

    internal void ChangeSortOrder(int sortOrder)
    {
        ValidationException.ThrowIfOutOfRange(sortOrder, 0, 10_000);
        SortOrder = sortOrder;
    }

    internal void ChangeAltText(string? altText)
    {
        if (!string.IsNullOrWhiteSpace(altText))
            ValidationException.ThrowIfNullOrWhiteSpace(altText);

            ValidationException.ThrowIfTooLong(altText, maxLen: 200);

        AltText = altText?.Trim();
    }
}
