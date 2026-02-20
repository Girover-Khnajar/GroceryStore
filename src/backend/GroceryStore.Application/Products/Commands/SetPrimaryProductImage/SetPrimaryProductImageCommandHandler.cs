using CQRS.Abstractions.Messaging;
using CQRS.CqrsResult;
using GroceryStore.Domain.Interfaces;
using GroceryStore.Domain.ValueObjects;

namespace GroceryStore.Application.Products.Commands.SetPrimaryProductImage;

public sealed class SetPrimaryProductImageCommandHandler : CommandHandlerBase<SetPrimaryProductImageCommand>
{
    private readonly IProductRepository _productRepository;
    private readonly IImageAssetRepository _imageAssetRepository;
    private readonly IUnitOfWork _unitOfWork;

    public SetPrimaryProductImageCommandHandler(
        IProductRepository productRepository,
        IImageAssetRepository imageAssetRepository,
        IUnitOfWork unitOfWork)
    {
        _productRepository = productRepository;
        _imageAssetRepository = imageAssetRepository;
        _unitOfWork = unitOfWork;
    }

    public override async Task<Result> HandleAsync(
        SetPrimaryProductImageCommand command,
        CancellationToken cancellationToken = default)
    {
        var product = await _productRepository.GetByIdAsync(command.ProductId, cancellationToken);
        if (product is null)
            return Failure(Error.NotFound($"Product '{command.ProductId}' not found."));

        var imageId = ImageId.Create(command.ImageId);

        if (product.ImageRefs.Count == 0)
            return Failure(Error.Validation("No images are attached to set as primary."));

        if (!product.HasImage(imageId))
            return Failure(Error.Validation($"Image '{command.ImageId}' is not attached to this product."));

        var asset = await _imageAssetRepository.GetByIdAsync(imageId, cancellationToken);
        if (asset is null || asset.IsDeleted)
            return Failure(Error.NotFound($"Image asset '{command.ImageId}' not found."));

        product.SetPrimaryImage(imageId);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Success();
    }
}
