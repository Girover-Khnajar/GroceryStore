namespace GroceryStore.Api.Contracts.Images;

public sealed record UploadImageRequest(
    IFormFile File,
    string? AltText = null);
