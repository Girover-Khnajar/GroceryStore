using GroceryStore.Domain.Common;
using GroceryStore.Domain.Common.DomainEvents;
using GroceryStore.Domain.Exceptions;
using GroceryStore.Domain.Interfaces;
using GroceryStore.Domain.ValueObjects;

namespace GroceryStore.Domain.Entities.Media;

/// <summary>
/// Aggregate root for the Media bounded context.
/// Represents a stored image file that exists independently of any Product.
/// ImageAsset does NOT know about Product — it is a standalone aggregate.
/// Products reference images by ImageId only.
/// </summary>
public sealed class ImageAsset : AuditableEntity, IAggregateRoot
{
    private ImageAsset() { } // For ORM

    /// <summary>
    /// Strongly-typed identity.
    /// </summary>
    public ImageId ImageId { get; private set; } = null!;

    /// <summary>
    /// Relative or absolute storage path/key (e.g. "images/2024/02/abc123.webp").
    /// </summary>
    public string StoragePath { get; private set; } = string.Empty;

    /// <summary>
    /// Public-facing URL for serving the image.
    /// </summary>
    public string Url { get; private set; } = string.Empty;

    /// <summary>
    /// Technical metadata about the stored file.
    /// </summary>
    public ImageMetadata Metadata { get; private set; } = null!;

    /// <summary>
    /// Optional alt text for accessibility / SEO.
    /// </summary>
    public string? AltText { get; private set; }

    /// <summary>
    /// Soft-delete flag.
    /// </summary>
    public bool IsDeleted { get; private set; }

    // ──────────────────────────────────────
    // Factory
    // ──────────────────────────────────────

    public static ImageAsset Create(
        string storagePath,
        string url,
        ImageMetadata metadata,
        string? altText = null)
    {
        ValidationException.ThrowIfNull(metadata);
        ValidationException.ThrowIfNullOrWhiteSpace(storagePath);

        ValidationException.ThrowIfTooLong(storagePath, maxLen: 500);
        ValidationException.ThrowIfNullOrWhiteSpace(url);

        ValidationException.ThrowIfTooLong(url, maxLen: 1000);
        if (!string.IsNullOrWhiteSpace(altText))
            ValidationException.ThrowIfNullOrWhiteSpace(altText);

            ValidationException.ThrowIfTooLong(altText, maxLen: 200);

        var asset = new ImageAsset
        {
            ImageId = ImageId.CreateNew(),
            StoragePath = storagePath.Trim(),
            Url = url.Trim(),
            Metadata = metadata,
            AltText = altText?.Trim()
        };

        // Sync the Entity.Id with the strongly typed id so EF can use it
        asset.Id = asset.ImageId.Value;

        return asset;
    }

    // ──────────────────────────────────────
    // Behavior
    // ──────────────────────────────────────

    public void ChangeAltText(string? altText)
    {
        if (!string.IsNullOrWhiteSpace(altText))
            ValidationException.ThrowIfNullOrWhiteSpace(altText);

            ValidationException.ThrowIfTooLong(altText, maxLen: 200);

        AltText = altText?.Trim();
        Touch();
    }

    /// <summary>
    /// Marks the asset as soft-deleted. The physical file should be cleaned up by an infrastructure process.
    /// </summary>
    public void MarkDeleted()
    {
        if (IsDeleted)
            return;

        IsDeleted = true;
        Touch();
    }
}
