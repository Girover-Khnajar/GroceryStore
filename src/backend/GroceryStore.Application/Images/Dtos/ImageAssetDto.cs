namespace GroceryStore.Application.Images.Dtos;

public sealed record ImageAssetDto(
    Guid Id,
    Guid ImageId,
    string StoragePath,
    string Url,
    string? AltText,
    bool IsDeleted,
    string OriginalFileName,
    string ContentType,
    long FileSizeBytes,
    int? WidthPx,
    int? HeightPx,
    DateTime CreatedOnUtc,
    DateTime? ModifiedOnUtc);
