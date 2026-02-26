using CQRS.Abstractions.Messaging;
using CQRS.CqrsResult;
using GroceryStore.Application.Common;
using GroceryStore.Application.Products.Dtos;

namespace GroceryStore.Application.Products.Queries.GetProducts;

public sealed class GetProductsQueryHandler
    : IQueryHandler<GetProductsQuery, PagedResult<ProductListItemDto>>
{
    private readonly IProductsReadStore _repo;
    public GetProductsQueryHandler(IProductsReadStore repo) => _repo = repo;

    public async Task<Result<PagedResult<ProductListItemDto>>> HandleAsync(GetProductsQuery q, CancellationToken ct)
    {

        var items = await _repo.GetPagedAsync(q, ct);

        return Result.Ok(items);
    }
}