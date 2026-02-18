using GroceryStore.Domain.Enums;

namespace GroceryStore.Application.Products.Dtos;

public sealed record ProductDto(
    Guid Id,
    Guid CategoryId,
    string Name,
    string Slug,
    string? ShortDescription,
    string? LongDescription,
    decimal PriceAmount,
    string PriceCurrency,
    string Unit,
    bool IsActive,
    bool IsFeatured,
    int SortOrder,
    string? Sku,
    string? Barcode,
    string? Brand,
    string? OriginCountryCode,
    bool? IsOrganic,
    bool? IsHalal,
    bool? IsVegan,
    string? Ingredients,
    NutritionFactsDto? Nutrition,
    string Storage,
    decimal? NetWeight,
    string? NetWeightUnit,
    IReadOnlyList<string> Tags,
    IReadOnlyList<string> Allergens,
    IReadOnlyList<ProductImageRefDto> ImageRefs,
    string? SeoMetaTitle,
    string? SeoMetaDescription,
    DateTime CreatedOnUtc,
    DateTime? ModifiedOnUtc);

public sealed record NutritionFactsDto(
    decimal? CaloriesKcal,
    decimal? ProteinG,
    decimal? CarbsG,
    decimal? FatG,
    decimal? SaltG);

public sealed record ProductImageRefDto(
    Guid Id,
    Guid ImageId,
    bool IsPrimary,
    int SortOrder,
    string? AltText);
