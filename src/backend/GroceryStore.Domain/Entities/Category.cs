using GroceryStore.Domain.Common;
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
        var category = new Category
        {
            Name = Guard.NotEmpty(name,nameof(name),maxLen: 120),
            Slug = Slug.Create(slug),
            SortOrder = sortOrder,
            ParentCategoryId = parentCategoryId,
            Description = string.IsNullOrWhiteSpace(description) ? null : Guard.NotEmpty(description,nameof(description),maxLen: 2000),
            ImageUrl = string.IsNullOrWhiteSpace(imageUrl) ? null : Guard.NotEmpty(imageUrl,nameof(imageUrl),maxLen: 500),
            IconName = string.IsNullOrWhiteSpace(iconName) ? null : Guard.NotEmpty(iconName,nameof(iconName),maxLen: 50),
            Seo = seo ?? SeoMeta.Create(null,null)
        };

        Guard.InRange(sortOrder,nameof(sortOrder),0,10_000);

        return category;
    }

    public void Rename(string name)
    {
        Name = Guard.NotEmpty(name,nameof(name),maxLen: 120);
        Touch( );
    }

    public void ChangeSlug(string slug)
    {
        Slug = Slug.Create(slug);
        Touch( );
    }

    public void SetDescription(string? description)
    {
        Description = string.IsNullOrWhiteSpace(description)
            ? null
            : Guard.NotEmpty(description,nameof(description),maxLen: 2000);

        Touch( );
    }

    public void SetSeo(string? metaTitle,string? metaDescription)
    {
        Seo = SeoMeta.Create(metaTitle,metaDescription);
        Touch( );
    }

    public void SetImage(string? imageUrl)
    {
        ImageUrl = string.IsNullOrWhiteSpace(imageUrl)
            ? null
            : Guard.NotEmpty(imageUrl,nameof(imageUrl),maxLen: 500);

        Touch( );
    }

    public void SetIcon(string? iconName)
    {
        IconName = string.IsNullOrWhiteSpace(iconName)
            ? null
            : Guard.NotEmpty(iconName,nameof(iconName),maxLen: 50);

        Touch( );
    }

    public void SetParent(Guid? parentCategoryId)
    {
        ParentCategoryId = parentCategoryId;
        Touch( );
    }

    public void Reorder(int sortOrder)
    {
        Guard.InRange(sortOrder,nameof(sortOrder),0,10_000);
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
