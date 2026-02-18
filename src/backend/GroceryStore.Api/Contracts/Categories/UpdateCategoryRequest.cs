namespace GroceryStore.Api.Contracts.Categories;

public sealed record UpdateCategoryRequest(
    string Name,
    string Slug,
    int SortOrder,
    Guid? ParentCategoryId,
    string? Description,
    string? ImageUrl,
    string? IconName,
    string? SeoMetaTitle,
    string? SeoMetaDescription);
