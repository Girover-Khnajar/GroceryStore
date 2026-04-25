using GroceryStore.Application.Categories.Dtos;
using GroceryStore.Application.Products.Dtos;

namespace GroceryStore.Web.ViewModels;

public sealed class HomeViewModel
{
    public IReadOnlyList<CategoryDto> Categories { get; init; } = [];
    public IReadOnlyList<ProductListItemDto> FeaturedProducts { get; init; } = [];
}
