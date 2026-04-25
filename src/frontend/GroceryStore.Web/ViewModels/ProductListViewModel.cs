using GroceryStore.Application.Categories.Dtos;
using GroceryStore.Application.Common;
using GroceryStore.Application.Products.Dtos;

namespace GroceryStore.Web.ViewModels;

public sealed class ProductListViewModel
{
    public PagedResult<ProductListItemDto> Products { get; init; } = new();
    public IReadOnlyList<CategoryDto> Categories { get; init; } = [];

    // Current filter state (round-tripped via query string)
    public string? Search { get; init; }
    public Guid? CategoryId { get; init; }
    public decimal? MinPrice { get; init; }
    public decimal? MaxPrice { get; init; }
    public bool? IsFeatured { get; init; }
    public string? Sort { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 12;
}
