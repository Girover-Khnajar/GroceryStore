namespace GroceryStore.Api.Services.Images;

public sealed record StoredImageDto(
    Guid Id,
    string Url,
    string StoragePath,
    string FileName,
    string ContentType,
    long FileSizeBytes,
    int Width,
    int Height,
    string? AltText,
    DateTime CreatedAtUtc
);
