using GroceryStore.Domain.Common;

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
        Url = Guard.NotEmpty(url,nameof(url),maxLen: 500);

        if (!string.IsNullOrWhiteSpace(altText))
            altText = Guard.NotEmpty(altText,nameof(altText),maxLen: 120);

        Guard.InRange(sortOrder,nameof(sortOrder),0,10_000);

        AltText = altText;
        IsPrimary = isPrimary;
        SortOrder = sortOrder;
    }

    public static ProductImage Create(string url,string? altText,bool isPrimary,int sortOrder)
        => new(url,altText,isPrimary,sortOrder);
}
