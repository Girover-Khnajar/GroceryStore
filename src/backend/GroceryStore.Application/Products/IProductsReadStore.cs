using GroceryStore.Application.Common;
using GroceryStore.Application.Products.Dtos;
using GroceryStore.Application.Products.Queries.GetProducts;

namespace GroceryStore.Application.Products;

public interface IProductsReadStore
{
    Task<PagedResult<ProductListItemDto>> GetPagedAsync(GetProductsQuery query, CancellationToken ct);
}