using CQRS.Abstractions.Messaging;
using CQRS.CqrsResult;
using GroceryStore.Domain.Interfaces;
using GroceryStore.Domain.ValueObjects;

namespace GroceryStore.Application.Products.Commands.AssignImageAssetToProduct;

public sealed class AssignImageAssetToProductCommandHandler : CommandHandlerBase<AssignImageAssetToProductCommand>
{
    private readonly IProductRepository _productRepository;
    private readonly IImageAssetRepository _imageAssetRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AssignImageAssetToProductCommandHandler(
        IProductRepository productRepository,
        IImageAssetRepository imageAssetRepository,
        IUnitOfWork unitOfWork)
    {
        _productRepository = productRepository;
        _imageAssetRepository = imageAssetRepository;
        _unitOfWork = unitOfWork;
    }

    public override async Task<Result> HandleAsync(
        AssignImageAssetToProductCommand command,
        CancellationToken cancellationToken = default)
    {
        if (command.SortOrder is < 0 or > 10_000)
            return Failure(Error.Validation("SortOrder must be between 0 and 10000."));

        if (command.AltText is not null && command.AltText.Length > 200)
            return Failure(Error.Validation("AltText must be 200 characters or less."));

        var product = await _productRepository.GetByIdAsync(command.ProductId, cancellationToken);
        if (product is null)
            return Failure(Error.NotFound($"Product '{command.ProductId}' not found."));

        var imageId = ImageId.Create(command.ImageId);
        var asset = await _imageAssetRepository.GetByIdAsync(imageId, cancellationToken);
        if (asset is null || asset.IsDeleted)
            return Failure(Error.NotFound($"Image asset '{command.ImageId}' not found."));

        // Idempotent assignment:
        // - If already attached: optionally promote to primary, then succeed.
        // - If not attached: attach with requested metadata.
        if (product.HasImage(imageId))
        {
            if (command.MakePrimary)
                product.SetPrimaryImage(imageId);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return Success();
        }

        product.AttachImage(
            imageId,
            makePrimary: command.MakePrimary,
            sortOrder: command.SortOrder,
            altText: command.AltText);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Success();
    }
}
