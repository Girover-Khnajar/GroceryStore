using CQRS.Abstractions.Messaging;
using CQRS.CqrsResult;
using GroceryStore.Domain.Entities.Media;
using GroceryStore.Domain.Interfaces;
using GroceryStore.Domain.ValueObjects;

namespace GroceryStore.Application.Images.Commands;

public sealed class CreateImageAssetCommandHandler : CommandHandlerBase<CreateImageAssetCommand, Guid>
{
    private readonly IImageAssetRepository _imageAssetRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateImageAssetCommandHandler(IImageAssetRepository imageAssetRepository, IUnitOfWork unitOfWork)
    {
        _imageAssetRepository = imageAssetRepository;
        _unitOfWork = unitOfWork;
    }

    public override async Task<Result<Guid>> HandleAsync(
        CreateImageAssetCommand command, CancellationToken cancellationToken = default)
    {
        var metadata = ImageMetadata.Create(
            command.FileName,
            command.ContentType,
            command.FileSizeBytes,
            command.Width > 0 ? command.Width : null,
            command.Height > 0 ? command.Height : null);

        var asset = ImageAsset.Create(
            command.StoragePath,
            command.Url,
            metadata,
            command.AltText);

        await _imageAssetRepository.AddAsync(asset, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Success(asset.ImageId.Value);
    }
}
