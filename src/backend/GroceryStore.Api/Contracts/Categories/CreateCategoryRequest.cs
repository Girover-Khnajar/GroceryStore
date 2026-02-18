namespace GroceryStore.Api.Contracts.Categories;

public sealed record CreateCategoryRequest(
    string Name,
    string Slug,
    int SortOrder = 0,
    Guid? ParentCategoryId = null,
    string? Description = null,
    string? ImageUrl = null,
    string? IconName = null,
    string? SeoMetaTitle = null,
    string? SeoMetaDescription = null);
