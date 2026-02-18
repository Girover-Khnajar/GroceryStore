using GroceryStore.Domain.Entities.Media;

namespace GroceryStore.Application.Images.Dtos;

public static class ImageAssetMappings
{
    public static ImageAssetDto ToDto(this ImageAsset asset)
        => new(
            asset.Id,
            asset.ImageId.Value,
            asset.StoragePath,
            asset.Url,
            asset.AltText,
            asset.IsDeleted,
            asset.Metadata.OriginalFileName,
            asset.Metadata.ContentType,
            asset.Metadata.FileSizeBytes,
            asset.Metadata.WidthPx,
            asset.Metadata.HeightPx,
            asset.CreatedOnUtc,
            asset.ModifiedOnUtc);
}
