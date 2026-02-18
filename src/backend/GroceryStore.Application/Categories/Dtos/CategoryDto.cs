namespace GroceryStore.Application.Categories.Dtos;

public sealed record CategoryDto(
    Guid Id,
    string Name,
    string Slug,
    string? Description,
    bool IsActive,
    int SortOrder,
    Guid? ParentCategoryId,
    string? ImageUrl,
    string? IconName,
    string? SeoMetaTitle,
    string? SeoMetaDescription,
    DateTime CreatedOnUtc,
    DateTime? ModifiedOnUtc);
