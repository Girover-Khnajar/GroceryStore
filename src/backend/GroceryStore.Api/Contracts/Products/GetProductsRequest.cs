namespace GroceryStore.Api.Contracts.Products;

public record GetProductsRequest(
    string? Search,
    Guid? CategoryId,
    decimal? MinPrice,
    decimal? MaxPrice,
    bool? IsActive,
    bool? IsFeatured,
    string? Brand,
    string? Sort,
    int Page,
    int PageSize
);