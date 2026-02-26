using CQRS.Abstractions.Messaging;
using CQRS.CqrsResult;
using GroceryStore.Application.Images.Dtos;
using GroceryStore.Domain.Interfaces;

namespace GroceryStore.Application.Images.Queries.GetImages;

public sealed class GetImagesQueryHandler : QueryHandlerBase<GetImagesQuery, List<ImageAssetDto>>
{
    private readonly IImageAssetRepository _imageAssetRepository;

    public GetImagesQueryHandler(IImageAssetRepository imageAssetRepository)
    {
        _imageAssetRepository = imageAssetRepository;
    }

    public override async Task<Result<List<ImageAssetDto>>> HandleAsync(GetImagesQuery query, CancellationToken cancellationToken = default)
    {
        var imageAssets = await _imageAssetRepository.GetImagesAsync(query
        .Search, cancellationToken);

        return Success([.. imageAssets.Select(asset => asset.ToDto())]);
    }
}