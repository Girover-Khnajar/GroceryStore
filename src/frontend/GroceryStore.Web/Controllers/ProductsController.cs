using CQRS.Abstractions.Messaging;
using GroceryStore.Application.Categories.Dtos;
using GroceryStore.Application.Categories.Queries;
using GroceryStore.Application.Common;
using GroceryStore.Application.Images.Dtos;
using GroceryStore.Application.Images.Queries;
using GroceryStore.Application.Products.Dtos;
using GroceryStore.Application.Products.Queries;
using GroceryStore.Application.Products.Queries.GetProducts;
using GroceryStore.Web.Extensions;
using GroceryStore.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace GroceryStore.Web.Controllers;

[Route("Products")]
public class ProductsController : Controller
{
    private readonly IMessageDispatcher _dispatcher;

    public ProductsController(IMessageDispatcher dispatcher)
        => _dispatcher = dispatcher;

    // GET /Products?search=...&categoryId=...&page=1
    [HttpGet("")]
    public async Task<IActionResult> Index(
        string? search,
        Guid? categoryId,
        decimal? minPrice,
        decimal? maxPrice,
        bool? isFeatured,
        string? sort,
        int page = 1,
        int pageSize = 12,
        CancellationToken ct = default)
    {
        var query = new GetProductsQuery(
            Search: search,
            CategoryId: categoryId,
            MinPrice: minPrice,
            MaxPrice: maxPrice,
            IsActive: true,
            IsFeatured: isFeatured,
            Brand: null,
            Sort: sort,
            Page: page,
            PageSize: pageSize);

        var productsResult = await _dispatcher
            .QueryAsync<GetProductsQuery, PagedResult<ProductListItemDto>>(query, ct);

        var categoriesResult = await _dispatcher
            .QueryAsync<GetAllActiveCategoriesQuery, IReadOnlyList<CategoryDto>>(
                new GetAllActiveCategoriesQuery(), ct);

        var vm = new ProductListViewModel
        {
            Products = productsResult.IsSuccess ? productsResult.Value! : new PagedResult<ProductListItemDto>(),
            Categories = categoriesResult.IsSuccess ? categoriesResult.Value! : [],
            Search = search,
            CategoryId = categoryId,
            MinPrice = minPrice,
            MaxPrice = maxPrice,
            IsFeatured = isFeatured,
            Sort = sort,
            Page = page,
            PageSize = pageSize
        };

        return View(vm);
    }

    // GET /Products/{slug}
    [HttpGet("{slug}")]
    public async Task<IActionResult> Details(string slug, CancellationToken ct)
    {
        var result = await _dispatcher
            .QueryAsync<GetProductBySlugQuery, ProductDto>(
                new GetProductBySlugQuery(slug), ct);

        if (result.IsNotFound())
            return NotFound();

        var product = result.ValueOrDefault(this);
        if (product is null)
            return RedirectToAction(nameof(Index));

        // Resolve category name for breadcrumb
        string? categoryName = null;
        var catResult = await _dispatcher
            .QueryAsync<GetCategoryByIdQuery, CategoryDto>(
                new GetCategoryByIdQuery(product.CategoryId), ct);
        if (catResult.IsSuccess)
            categoryName = catResult.Value!.Name;

        var orderedImageIds = product.ImageRefs
            .OrderByDescending(r => r.IsPrimary)
            .ThenBy(r => r.SortOrder)
            .Select(r => r.ImageId)
            .ToList();

        var imageUrls = new List<string>();
        if (orderedImageIds.Count > 0)
        {
            var imagesResult = await _dispatcher
                .QueryAsync<GetImagesByIdsQuery, IReadOnlyList<ImageAssetDto>>(
                    new GetImagesByIdsQuery(orderedImageIds), ct);

            if (imagesResult.IsSuccess)
            {
                var map = imagesResult.Value!
                    .Where(x => !x.IsDeleted && !string.IsNullOrWhiteSpace(x.StoragePath))
                    .ToDictionary(x => x.ImageId, x => x.StoragePath);

                imageUrls = orderedImageIds
                    .Where(map.ContainsKey)
                    .Select(id => map[id])
                    .ToList();
            }
        }

        var vm = new ProductDetailViewModel
        {
            Product = product,
            CategoryName = categoryName,
            ImageUrls = imageUrls,
            PrimaryImageUrl = imageUrls.FirstOrDefault()
        };

        return View(vm);
    }
}
