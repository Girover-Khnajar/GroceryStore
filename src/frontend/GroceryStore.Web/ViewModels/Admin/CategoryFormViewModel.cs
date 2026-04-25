using System.ComponentModel.DataAnnotations;
using GroceryStore.Application.Categories.Dtos;

namespace GroceryStore.Web.ViewModels.Admin;

public sealed class CategoryFormViewModel
{
    public Guid? Id { get; set; }

    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [Required, MaxLength(200)]
    public string Slug { get; set; } = string.Empty;

    public int SortOrder { get; set; }

    public Guid? ParentCategoryId { get; set; }

    [MaxLength(1000)]
    public string? Description { get; set; }

    [MaxLength(500)]
    public string? ImageUrl { get; set; }

    [MaxLength(100)]
    public string? IconName { get; set; }

    [MaxLength(200)]
    public string? SeoMetaTitle { get; set; }

    [MaxLength(300)]
    public string? SeoMetaDescription { get; set; }

    /// <summary>Available parent categories for the dropdown.</summary>
    public IReadOnlyList<CategoryDto> AvailableParents { get; set; } = [];

    // ── Mapping helpers ────────────────────────────────────────────────

    public static CategoryFormViewModel FromDto(CategoryDto dto) => new()
    {
        Id = dto.Id,
        Name = dto.Name,
        Slug = dto.Slug,
        SortOrder = dto.SortOrder,
        ParentCategoryId = dto.ParentCategoryId,
        Description = dto.Description,
        ImageUrl = dto.ImageUrl,
        IconName = dto.IconName,
        SeoMetaTitle = dto.SeoMetaTitle,
        SeoMetaDescription = dto.SeoMetaDescription
    };
}
