using GroceryStore.Domain.Common;

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
            metaTitle = Guard.NotEmpty(metaTitle,nameof(metaTitle),maxLen: 60);

        if (!string.IsNullOrWhiteSpace(metaDescription))
            metaDescription = Guard.NotEmpty(metaDescription,nameof(metaDescription),maxLen: 160);

        MetaTitle = metaTitle;
        MetaDescription = metaDescription;
    }

    public static SeoMeta Create(string? metaTitle,string? metaDescription)
        => new(metaTitle,metaDescription);
}
