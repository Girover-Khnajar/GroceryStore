using CQRS.Abstractions.Messaging;

namespace GroceryStore.Application.Products.Commands;

public sealed record UpdateProductCommand(
    Guid Id,
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
    string? SeoMetaDescription) : ICommand;
