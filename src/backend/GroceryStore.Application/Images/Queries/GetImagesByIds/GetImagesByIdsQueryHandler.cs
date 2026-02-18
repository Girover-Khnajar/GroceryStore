using CQRS.Abstractions.Messaging;
using CQRS.CqrsResult;
using GroceryStore.Application.Images.Dtos;
using GroceryStore.Domain.Interfaces;
using GroceryStore.Domain.ValueObjects;

namespace GroceryStore.Application.Images.Queries;

public sealed class GetImagesByIdsQueryHandler
    : QueryHandlerBase<GetImagesByIdsQuery, IReadOnlyList<ImageAssetDto>>
{
    private readonly IImageAssetRepository _imageAssetRepository;

    public GetImagesByIdsQueryHandler(IImageAssetRepository imageAssetRepository)
    {
        _imageAssetRepository = imageAssetRepository;
    }

    public override async Task<Result<IReadOnlyList<ImageAssetDto>>> HandleAsync(
        GetImagesByIdsQuery query, CancellationToken cancellationToken = default)
    {
        var imageIds = query.ImageIds.Select(ImageId.Create).ToList();
        var assets = await _imageAssetRepository.GetByIdsAsync(imageIds, cancellationToken);

        var dtos = assets.Select(a => a.ToDto()).ToList().AsReadOnly();

        return Success((IReadOnlyList<ImageAssetDto>)dtos);
    }
}
