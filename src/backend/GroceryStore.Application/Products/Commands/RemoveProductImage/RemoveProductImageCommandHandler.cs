using CQRS.Abstractions.Messaging;
using CQRS.CqrsResult;
using GroceryStore.Domain.Interfaces;
using GroceryStore.Domain.ValueObjects;

namespace GroceryStore.Application.Products.Commands.RemoveProductImage;

public sealed class RemoveProductImageCommandHandler : CommandHandlerBase<RemoveProductImageCommand>
{
    private readonly IProductRepository _productRepository;
    private readonly IUnitOfWork _unitOfWork;

    public RemoveProductImageCommandHandler(
        IProductRepository productRepository,
        IUnitOfWork unitOfWork)
    {
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
    }

    public override async Task<Result> HandleAsync(
        RemoveProductImageCommand command,
        CancellationToken cancellationToken = default)
    {
        var product = await _productRepository.GetByIdAsync(command.ProductId, cancellationToken);
        if (product is null)
            return Failure(Error.NotFound($"Product '{command.ProductId}' not found."));

        var imageId = ImageId.Create(command.ImageId);
        product.RemoveImage(imageId); // idempotent if not attached

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Success();
    }
}
