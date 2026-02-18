using GroceryStore.Domain.Entities;

namespace GroceryStore.Application.Products.Dtos;

public static class ProductMappings
{
    public static ProductDto ToDto(this Product product)
        => new(
            product.Id,
            product.CategoryId,
            product.Name,
            product.Slug.Value,
            product.ShortDescription,
            product.LongDescription,
            product.Price.Amount,
            product.Price.Currency,
            product.Unit.ToString(),
            product.IsActive,
            product.IsFeatured,
            product.SortOrder,
            product.Sku,
            product.Barcode,
            product.Brand,
            product.OriginCountryCode,
            product.IsOrganic,
            product.IsHalal,
            product.IsVegan,
            product.Ingredients,
            product.Nutrition is not null
                ? new NutritionFactsDto(
                    product.Nutrition.CaloriesKcal,
                    product.Nutrition.ProteinG,
                    product.Nutrition.CarbsG,
                    product.Nutrition.FatG,
                    product.Nutrition.SaltG)
                : null,
            product.Storage.ToString(),
            product.NetWeight,
            product.NetWeightUnit?.ToString(),
            product.Tags.ToList(),
            product.Allergens.ToList(),
            product.ImageRefs.Select(r => new ProductImageRefDto(
                r.Id,
                r.ImageId.Value,
                r.IsPrimary,
                r.SortOrder,
                r.AltText)).ToList(),
            product.Seo.MetaTitle,
            product.Seo.MetaDescription,
            product.CreatedOnUtc,
            product.ModifiedOnUtc);
}
