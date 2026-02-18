using CQRS.Abstractions.Messaging;
using CQRS.CqrsResult;
using GroceryStore.Application.Products.Dtos;
using GroceryStore.Domain.Interfaces;

namespace GroceryStore.Application.Products.Queries;

public sealed class GetProductsByCategoryIdQueryHandler
    : QueryHandlerBase<GetProductsByCategoryIdQuery, IReadOnlyList<ProductDto>>
{
    private readonly IProductRepository _productRepository;

    public GetProductsByCategoryIdQueryHandler(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public override async Task<Result<IReadOnlyList<ProductDto>>> HandleAsync(
        GetProductsByCategoryIdQuery query, CancellationToken cancellationToken = default)
    {
        var products = await _productRepository.GetByCategoryIdAsync(query.CategoryId, cancellationToken);

        var dtos = products.Select(p => p.ToDto()).ToList().AsReadOnly();

        return Success((IReadOnlyList<ProductDto>)dtos);
    }
}
