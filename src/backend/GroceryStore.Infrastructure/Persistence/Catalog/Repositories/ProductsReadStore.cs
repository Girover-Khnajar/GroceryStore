using GroceryStore.Application.Common;
using GroceryStore.Application.Products;
using GroceryStore.Application.Products.Dtos;
using GroceryStore.Application.Products.Queries.GetProducts;
using GroceryStore.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GroceryStore.Infrastructure.Persistence.Catalog.Repositories;

public sealed class ProductsReadStore : IProductsReadStore
{
    private readonly AppDbContext _db;

    public ProductsReadStore(AppDbContext db) => _db = db;

    public async Task<PagedResult<ProductListItemDto>> GetPagedAsync(GetProductsQuery q, CancellationToken ct)
    {
        var (page, pageSize) = NormalizePaging(q.Page, q.PageSize);

        IQueryable<Product> query = _db.Products.AsNoTracking();

        // ---- Filters
        if (!string.IsNullOrWhiteSpace(q.Search))
        {
            var s = q.Search.Trim();
            query = query.Where(p =>
                p.Name.Contains(s) ||
                p.Slug.Value.Contains(s) ||
                (p.Sku != null && p.Sku.Contains(s)) ||
                (p.Barcode != null && p.Barcode.Contains(s)));
        }

        if (q.CategoryId.HasValue)
            query = query.Where(p => p.CategoryId == q.CategoryId.Value);

        if (q.IsActive.HasValue)
            query = query.Where(p => p.IsActive == q.IsActive.Value);

        if (q.IsFeatured.HasValue)
            query = query.Where(p => p.IsFeatured == q.IsFeatured.Value);

        if (!string.IsNullOrWhiteSpace(q.Brand))
            query = query.Where(p => p.Brand != null && p.Brand == q.Brand);

        if (q.MinPrice.HasValue)
            query = query.Where(p => p.Price.Amount >= q.MinPrice.Value);

        if (q.MaxPrice.HasValue)
            query = query.Where(p => p.Price.Amount <= q.MaxPrice.Value);

        // ---- Count قبل Paging
        var total = await query.CountAsync(ct);

        // ---- Sorting (whitelist)
        query = ApplySort(query, q.Sort);

        // ---- Paging
        query = query.Skip((page - 1) * pageSize).Take(pageSize);

        var paged = await query
            .Select(p => new
            {
                p.Id,
                p.CategoryId,
                p.Name,
                Slug = p.Slug.Value,
                PriceAmount = p.Price.Amount,
                PriceCurrency = p.Price.Currency,
                Unit = p.Unit.ToString(),
                p.IsActive,
                p.IsFeatured,
                p.SortOrder,
                p.Brand,
                p.CreatedOnUtc,
                PrimaryImageGuid = _db.Set<Domain.Entities.Catalog.ProductImageRef>()
                    .Where(r => EF.Property<Guid>(r, "ProductId") == p.Id)
                    .OrderByDescending(r => r.IsPrimary)
                    .ThenBy(r => r.SortOrder)
                    .Select(r => r.ImageId.Value)
                    .FirstOrDefault()
            })
            .ToListAsync(ct);

        var imageIds = paged
            .Select(x => x.PrimaryImageGuid)
            .Where(x => x != Guid.Empty)
            .Distinct()
            .ToList();

        var imagePathById = imageIds.Count == 0
            ? new Dictionary<Guid, string>()
            : await _db.ImageAssets
                .AsNoTracking()
                .Where(x => !x.IsDeleted && imageIds.Contains(x.Id))
                .ToDictionaryAsync(x => x.Id, x => x.StoragePath, ct);

        var items = paged.Select(x =>
        {
            imagePathById.TryGetValue(x.PrimaryImageGuid, out var primaryPath);

            return new ProductListItemDto(
                x.Id,
                x.CategoryId,
                x.Name,
                x.Slug,
                x.PriceAmount,
                x.PriceCurrency,
                x.Unit,
                x.IsActive,
                x.IsFeatured,
                x.SortOrder,
                x.Brand,
                primaryPath ?? string.Empty,
                x.CreatedOnUtc);
        }).ToList();
        // ---- Projection إلى ProductDto (لو رجّعت ProductDto كامل للّست)
        // var items = await query.Select(p => new ProductDto(
        //     p.Id,
        //     p.CategoryId,
        //     p.Name,
        //     p.Slug.Value,
        //     p.ShortDescription,
        //     p.LongDescription,
        //     p.Price.Amount,
        //     p.Price.Currency,
        //     p.Unit.ToString(),
        //     p.IsActive,
        //     p.IsFeatured,
        //     p.SortOrder,
        //     p.Sku,
        //     p.Barcode,
        //     p.Brand,
        //     p.OriginCountryCode,
        //     p.IsOrganic,
        //     p.IsHalal,
        //     p.IsVegan,
        //     p.Ingredients,
        //     p.Nutrition == null ? null : new NutritionFactsDto(
        //         p.Nutrition.CaloriesKcal,
        //         p.Nutrition.ProteinG,
        //         p.Nutrition.CarbsG,
        //         p.Nutrition.FatG,
        //         p.Nutrition.SaltG
        //     ),
        //     p.Storage.ToString(),
        //     p.NetWeight,
        //     p.NetWeightUnit.ToString(),
        //     p.Tags.ToList(),
        //     p.Allergens.ToList(),
        //     p.ImageRefs
        //         .OrderBy(x => x.SortOrder)
        //         .Select(x => new ProductImageRefDto(
        //             x.Id,
        //             x.ImageId.Value, // عدّل حسب نوع ImageId عندك
        //             x.IsPrimary,
        //             x.SortOrder,
        //             x.AltText
        //         ))
        //         .ToList(),
        //     p.Seo.MetaTitle,
        //     p.Seo.MetaDescription,
        //     p.CreatedOnUtc,
        //     p.ModifiedOnUtc
        // )).ToListAsync(ct);

        return new PagedResult<ProductListItemDto>
        {
            Items = items,
            TotalCount = total,
            Page = page,
            PageSize = pageSize
        };
    }

    private static IQueryable<Product> ApplySort(IQueryable<Product> query, string? sort)
        => sort switch
        {
            "price_asc" => query.OrderBy(p => p.Price.Amount),
            "price_desc" => query.OrderByDescending(p => p.Price.Amount),
            "name_asc" => query.OrderBy(p => p.Name),
            "name_desc" => query.OrderByDescending(p => p.Name),
            "featured" => query.OrderByDescending(p => p.IsFeatured).ThenBy(p => p.SortOrder),
            "newest" => query.OrderByDescending(p => p.CreatedOnUtc),
            _ => query.OrderBy(p => p.SortOrder).ThenBy(p => p.Name),
        };

    private static (int page, int pageSize) NormalizePaging(int page, int pageSize)
    {
        page = page < 1 ? 1 : page;
        pageSize = pageSize < 1 ? 20 : pageSize;
        pageSize = pageSize > 100 ? 100 : pageSize;
        return (page, pageSize);
    }
}