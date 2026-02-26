using CQRS.Abstractions.Messaging;
using GroceryStore.Application.Common;
using GroceryStore.Application.Products.Dtos;

namespace GroceryStore.Application.Products.Queries.GetProducts;

public sealed record GetProductsQuery(
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
) : IQuery<PagedResult<ProductListItemDto>>;