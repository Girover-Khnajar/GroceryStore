using CQRS.Abstractions.Messaging;
using CQRS.CqrsResult;
using GroceryStore.Application.Products.Dtos;
using GroceryStore.Domain.Interfaces;

namespace GroceryStore.Application.Products.Queries;

public sealed class GetProductBySlugQueryHandler : QueryHandlerBase<GetProductBySlugQuery, ProductDto>
{
    private readonly IProductRepository _productRepository;

    public GetProductBySlugQueryHandler(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public override async Task<Result<ProductDto>> HandleAsync(
        GetProductBySlugQuery query, CancellationToken cancellationToken = default)
    {
        var product = await _productRepository.GetBySlugAsync(query.Slug, cancellationToken);
        if (product is null)
            return NotFound($"Product with slug '{query.Slug}' not found.");

        return Success(product.ToDto());
    }
}
