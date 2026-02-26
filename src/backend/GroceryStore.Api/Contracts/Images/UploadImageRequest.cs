namespace GroceryStore.Api.Contracts.Images;

public sealed record UploadImageRequest
{
    public IFormFile File { get; init; } = default!;
    public string? AltText { get; init; }
}


// public sealed record ListImageAssetsResponse(List<ImageAssetDto> Items);

// public sealed record ImageAssetDto(
//     Guid Id,
//     string Url,
//     string StoragePath,
//     string FileName,
//     string ContentType,
//     long FileSizeBytes,
//     int Width,
//     int Height,
//     string? AltText,
//     DateTime CreatedAtUtc
// );

// Product entity list item (asset + ref info)
public sealed record ProductImageItemDto(
    Guid ImageId,
    string Url,
    string FileName,
    string ContentType,
    long FileSizeBytes,
    int Width,
    int Height,
    string? AltText,
    bool IsPrimary,
    int SortOrder
);

public sealed record ListProductImagesResponse(List<ProductImageItemDto> Items);

public sealed record AssignProductImagesRequest(List<Guid> ImageIds, bool MakeFirstPrimary = false);
public sealed record UnassignProductImagesRequest(List<Guid> ImageIds);
public sealed record SetProductPrimaryRequest(Guid ImageId);
public sealed record ReorderProductImagesRequest(Guid DraggingId, Guid TargetId);


// Category (اختياري)
public sealed record CategoryImageItemDto(
    Guid ImageId,
    string Url,
    string FileName,
    string ContentType,
    long FileSizeBytes,
    int Width,
    int Height,
    string? AltText,
    bool IsPrimary,
    int SortOrder
);

public sealed record ListCategoryImagesResponse(List<CategoryImageItemDto> Items);

public sealed record AssignCategoryImagesRequest(List<Guid> ImageIds);
public sealed record UnassignCategoryImagesRequest(List<Guid> ImageIds);
public sealed record SetCategoryPrimaryRequest(Guid ImageId);
public sealed record ReorderCategoryImagesRequest(Guid DraggingId, Guid TargetId);