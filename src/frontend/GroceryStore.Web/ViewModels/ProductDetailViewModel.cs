using GroceryStore.Application.Products.Dtos;

namespace GroceryStore.Web.ViewModels;

public sealed class ProductDetailViewModel
{
    public required ProductDto Product { get; init; }
    public string? CategoryName { get; init; }
    public IReadOnlyList<string> ImageUrls { get; init; } = [];
    public string? PrimaryImageUrl { get; init; }
}
