using GroceryStore.Domain.Common;
using GroceryStore.Domain.Exceptions;
using GroceryStore.Domain.Common.DomainEvents;
using GroceryStore.Domain.Interfaces;
using GroceryStore.Domain.ValueObjects;

namespace GroceryStore.Domain.Entities;

/// <summary>
/// Category aggregate root.
/// </summary>
public sealed class Category : AuditableEntity, IAggregateRoot
{
    private Category() { } // For ORM

    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public Slug Slug { get; private set; } = Slug.Create("default");
    public SeoMeta Seo { get; private set; } = SeoMeta.Create(null,null);

    public bool IsActive { get; private set; } = true;
    public int SortOrder { get; private set; }

    // Hierarchy (optional)
    public Guid? ParentCategoryId { get; private set; }

    // Media
    public string? ImageUrl { get; private set; }

    // Extra marketing fields (optional)
    public string? IconName { get; private set; } // e.g. "carrot", "meat", etc.

    public static Category Create(
        string name,
        string slug,
        int sortOrder = 0,
        Guid? parentCategoryId = null,
        string? description = null,
        string? imageUrl = null,
        string? iconName = null,
        SeoMeta? seo = null)
    {
        ValidationException.ThrowIfNullOrWhiteSpace(name);

        ValidationException.ThrowIfTooLong(name, maxLen: 120);
        ValidationException.ThrowIfOutOfRange(sortOrder, 0, 10_000);
        if (!string.IsNullOrWhiteSpace(description))
            ValidationException.ThrowIfNullOrWhiteSpace(description);

            ValidationException.ThrowIfTooLong(description, maxLen: 2000);
        if (!string.IsNullOrWhiteSpace(imageUrl))
            ValidationException.ThrowIfNullOrWhiteSpace(imageUrl);

            ValidationException.ThrowIfTooLong(imageUrl, maxLen: 500);
        if (!string.IsNullOrWhiteSpace(iconName))
            ValidationException.ThrowIfNullOrWhiteSpace(iconName);

            ValidationException.ThrowIfTooLong(iconName, maxLen: 50);

        var category = new Category
        {
            Name = name.Trim(),
            Slug = Slug.Create(slug),
            SortOrder = sortOrder,
            ParentCategoryId = parentCategoryId,
            Description = description?.Trim(),
            ImageUrl = imageUrl?.Trim(),
            IconName = iconName?.Trim(),
            Seo = seo ?? SeoMeta.Create(null,null)
        };

        return category;
    }

    public void Rename(string name)
    {
        ValidationException.ThrowIfNullOrWhiteSpace(name);

        ValidationException.ThrowIfTooLong(name, maxLen: 120);
        Name = name.Trim();
        Touch();
    }

    public void ChangeSlug(string slug)
    {
        Slug = Slug.Create(slug);
        Touch( );
    }

    public void SetDescription(string? description)
    {
        if (!string.IsNullOrWhiteSpace(description))
            ValidationException.ThrowIfNullOrWhiteSpace(description);

            ValidationException.ThrowIfTooLong(description, maxLen: 2000);

        Description = description?.Trim();
        Touch();
    }

    public void SetSeo(string? metaTitle,string? metaDescription)
    {
        Seo = SeoMeta.Create(metaTitle,metaDescription);
        Touch( );
    }

    public void SetImage(string? imageUrl)
    {
        if (!string.IsNullOrWhiteSpace(imageUrl))
            ValidationException.ThrowIfNullOrWhiteSpace(imageUrl);

            ValidationException.ThrowIfTooLong(imageUrl, maxLen: 500);

        ImageUrl = imageUrl?.Trim();
        Touch();
    }

    public void SetIcon(string? iconName)
    {
        if (!string.IsNullOrWhiteSpace(iconName))
            ValidationException.ThrowIfNullOrWhiteSpace(iconName);

            ValidationException.ThrowIfTooLong(iconName, maxLen: 50);

        IconName = iconName?.Trim();
        Touch();
    }

    public void SetParent(Guid? parentCategoryId)
    {
        ParentCategoryId = parentCategoryId;
        Touch( );
    }

    public void Reorder(int sortOrder)
    {
        ValidationException.ThrowIfOutOfRange(sortOrder, 0, 10_000);
        SortOrder = sortOrder;
        Touch( );
    }

    public void Activate()
    {
        IsActive = true;
        Touch( );
    }

    public void Deactivate()
    {
        IsActive = false;
        Touch( );
    }
}
