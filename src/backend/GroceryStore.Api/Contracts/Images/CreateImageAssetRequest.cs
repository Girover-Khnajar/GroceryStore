namespace GroceryStore.Api.Contracts.Images;

public sealed record CreateImageAssetRequest(
    string StoragePath,
    string Url,
    string FileName,
    string ContentType,
    long FileSizeBytes,
    int Width,
    int Height,
    string? AltText = null);
