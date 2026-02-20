namespace GroceryStore.Api.Contracts.Products;

public sealed record AssignProductImageRequest(
    Guid ImageId,
    bool MakePrimary = false,
    int SortOrder = 0,
    string? AltText = null);
