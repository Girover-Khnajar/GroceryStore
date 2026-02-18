using GroceryStore.Domain.Exceptions;

namespace GroceryStore.Domain.ValueObjects;

/// <summary>
/// SEO metadata.
/// </summary>
public sealed record SeoMeta
{
    public string? MetaTitle { get; }
    public string? MetaDescription { get; }

    private SeoMeta(string? metaTitle,string? metaDescription)
    {
        if (!string.IsNullOrWhiteSpace(metaTitle))
            ValidationException.ThrowIfNullOrWhiteSpace(metaTitle);

            ValidationException.ThrowIfTooLong(metaTitle, maxLen: 60);

        if (!string.IsNullOrWhiteSpace(metaDescription))
            ValidationException.ThrowIfNullOrWhiteSpace(metaDescription);

            ValidationException.ThrowIfTooLong(metaDescription, maxLen: 160);

        MetaTitle = metaTitle?.Trim();
        MetaDescription = metaDescription?.Trim();
    }

    public static SeoMeta Create(string? metaTitle,string? metaDescription)
        => new(metaTitle,metaDescription);
}
