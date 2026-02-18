using GroceryStore.Domain.Entities;

namespace GroceryStore.Application.Categories.Dtos;

public static class CategoryMappings
{
    public static CategoryDto ToDto(this Category category)
        => new(
            category.Id,
            category.Name,
            category.Slug.Value,
            category.Description,
            category.IsActive,
            category.SortOrder,
            category.ParentCategoryId,
            category.ImageUrl,
            category.IconName,
            category.Seo.MetaTitle,
            category.Seo.MetaDescription,
            category.CreatedOnUtc,
            category.ModifiedOnUtc);
}
