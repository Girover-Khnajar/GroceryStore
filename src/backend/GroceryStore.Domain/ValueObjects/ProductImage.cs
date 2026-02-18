using GroceryStore.Domain.Exceptions;

namespace GroceryStore.Domain.ValueObjects;

/// <summary>
/// Represents an image attached to a product.
/// </summary>
public sealed record ProductImage
{
    public string Url { get; }
    public string? AltText { get; }
    public bool IsPrimary { get; }
    public int SortOrder { get; }

    private ProductImage(string url,string? altText,bool isPrimary,int sortOrder)
    {
        ValidationException.ThrowIfNullOrWhiteSpace(url);

        ValidationException.ThrowIfTooLong(url, maxLen: 500);
        Url = url.Trim();

        if (!string.IsNullOrWhiteSpace(altText))
            ValidationException.ThrowIfNullOrWhiteSpace(altText);

            ValidationException.ThrowIfTooLong(altText, maxLen: 120);

        ValidationException.ThrowIfOutOfRange(sortOrder, 0, 10_000);

        AltText = altText?.Trim();
        IsPrimary = isPrimary;
        SortOrder = sortOrder;
    }

    public static ProductImage Create(string url,string? altText,bool isPrimary,int sortOrder)
        => new(url,altText,isPrimary,sortOrder);
}
