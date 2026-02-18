namespace GroceryStore.Api.Contracts.Products;

public sealed record UpdateProductRequest(
    Guid CategoryId,
    string Name,
    string Slug,
    decimal PriceAmount,
    string PriceCurrency,
    string Unit,
    int SortOrder,
    bool IsFeatured,
    string? ShortDescription,
    string? LongDescription,
    string? SeoMetaTitle,
    string? SeoMetaDescription);
