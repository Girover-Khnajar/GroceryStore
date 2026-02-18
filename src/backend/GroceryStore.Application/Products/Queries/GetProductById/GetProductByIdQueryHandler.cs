using CQRS.Abstractions.Messaging;
using CQRS.CqrsResult;
using GroceryStore.Application.Products.Dtos;
using GroceryStore.Domain.Interfaces;

namespace GroceryStore.Application.Products.Queries;

public sealed class GetProductByIdQueryHandler : QueryHandlerBase<GetProductByIdQuery, ProductDto>
{
    private readonly IProductRepository _productRepository;

    public GetProductByIdQueryHandler(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public override async Task<Result<ProductDto>> HandleAsync(
        GetProductByIdQuery query, CancellationToken cancellationToken = default)
    {
        var product = await _productRepository.GetByIdAsync(query.Id, cancellationToken);
        if (product is null)
            return NotFound($"Product '{query.Id}' not found.");

        return Success(product.ToDto());
    }
}
