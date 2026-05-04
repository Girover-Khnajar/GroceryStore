using GroceryStore.Application.Categories.Dtos;
using GroceryStore.Application.Products.Dtos;

namespace GroceryStore.Web.ViewModels;

public sealed class CategoryDetailViewModel
{
    public required CategoryDto Category { get; init; }
    public IReadOnlyList<ProductDto> Products { get; init; } = [];
    public IReadOnlyDictionary<Guid, string?> ProductPrimaryImageUrls { get; init; } = new Dictionary<Guid, string?>();
}
