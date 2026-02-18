using CQRS.Abstractions.Messaging;
using CQRS.CqrsResult;
using GroceryStore.Application.Images.Dtos;
using GroceryStore.Domain.Interfaces;
using GroceryStore.Domain.ValueObjects;

namespace GroceryStore.Application.Images.Queries;

public sealed class GetImageByIdQueryHandler : QueryHandlerBase<GetImageByIdQuery, ImageAssetDto>
{
    private readonly IImageAssetRepository _imageAssetRepository;

    public GetImageByIdQueryHandler(IImageAssetRepository imageAssetRepository)
    {
        _imageAssetRepository = imageAssetRepository;
    }

    public override async Task<Result<ImageAssetDto>> HandleAsync(
        GetImageByIdQuery query, CancellationToken cancellationToken = default)
    {
        var imageId = ImageId.Create(query.ImageId);
        var asset = await _imageAssetRepository.GetByIdAsync(imageId, cancellationToken);
        if (asset is null)
            return NotFound($"Image asset '{query.ImageId}' not found.");

        return Success(asset.ToDto());
    }
}
