namespace GroceryStore.Api.Contracts.Products;

public sealed record CreateProductRequest(
    Guid CategoryId,
    string Name,
    string Slug,
    decimal PriceAmount,
    string PriceCurrency,
    string Unit,
    int SortOrder = 0,
    bool IsFeatured = false,
    string? ShortDescription = null,
    string? LongDescription = null,
    string? SeoMetaTitle = null,
    string? SeoMetaDescription = null);
