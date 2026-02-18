using CQRS.Abstractions.Messaging;
using CQRS.CqrsResult;
using GroceryStore.Domain.Interfaces;
using GroceryStore.Domain.ValueObjects;

namespace GroceryStore.Application.Images.Commands;

public sealed class DeleteImageAssetCommandHandler : CommandHandlerBase<DeleteImageAssetCommand>
{
    private readonly IImageAssetRepository _imageAssetRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteImageAssetCommandHandler(IImageAssetRepository imageAssetRepository, IUnitOfWork unitOfWork)
    {
        _imageAssetRepository = imageAssetRepository;
        _unitOfWork = unitOfWork;
    }

    public override async Task<Result> HandleAsync(
        DeleteImageAssetCommand command, CancellationToken cancellationToken = default)
    {
        var imageId = ImageId.Create(command.ImageId);
        var asset = await _imageAssetRepository.GetByIdAsync(imageId, cancellationToken);
        if (asset is null)
            return Failure(Error.NotFound($"Image asset '{command.ImageId}' not found."));

        asset.MarkDeleted();

        _imageAssetRepository.Update(asset);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Success();
    }
}
