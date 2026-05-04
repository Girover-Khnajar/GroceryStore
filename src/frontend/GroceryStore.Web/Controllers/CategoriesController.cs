using CQRS.Abstractions.Messaging;
using GroceryStore.Application.Categories.Dtos;
using GroceryStore.Application.Categories.Queries;
using GroceryStore.Application.Images.Dtos;
using GroceryStore.Application.Images.Queries;
using GroceryStore.Application.Products.Dtos;
using GroceryStore.Application.Products.Queries;
using GroceryStore.Web.Extensions;
using GroceryStore.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace GroceryStore.Web.Controllers;

[Route("Categories")]
public class CategoriesController : Controller
{
    private readonly IMessageDispatcher _dispatcher;

    public CategoriesController(IMessageDispatcher dispatcher)
        => _dispatcher = dispatcher;

    // GET /Categories
    [HttpGet("")]
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var result = await _dispatcher
            .QueryAsync<GetAllActiveCategoriesQuery, IReadOnlyList<CategoryDto>>(
                new GetAllActiveCategoriesQuery(), ct);

        var categories = result.ValueOrDefault(this) ?? (IReadOnlyList<CategoryDto>)[];
        return View(categories);
    }

    // GET /Categories/{slug}
    [HttpGet("{slug}")]
    public async Task<IActionResult> Details(string slug, CancellationToken ct)
    {
        var catResult = await _dispatcher
            .QueryAsync<GetCategoryBySlugQuery, CategoryDto>(
                new GetCategoryBySlugQuery(slug), ct);

        if (catResult.IsNotFound())
            return NotFound();

        var category = catResult.ValueOrDefault(this);
        if (category is null)
            return RedirectToAction(nameof(Index));

        var productsResult = await _dispatcher
            .QueryAsync<GetProductsByCategoryIdQuery, IReadOnlyList<ProductDto>>(
                new GetProductsByCategoryIdQuery(category.Id), ct);

        var products = productsResult.IsSuccess ? productsResult.Value! : [];

        var imageRefs = products
            .SelectMany(p => p.ImageRefs)
            .ToList();

        var orderedImageIds = imageRefs
            .OrderByDescending(r => r.IsPrimary)
            .ThenBy(r => r.SortOrder)
            .Select(r => r.ImageId)
            .Distinct()
            .ToList();

        var imageMap = new Dictionary<Guid, string>();
        if (orderedImageIds.Count > 0)
        {
            var imagesResult = await _dispatcher
                .QueryAsync<GetImagesByIdsQuery, IReadOnlyList<ImageAssetDto>>(
                    new GetImagesByIdsQuery(orderedImageIds), ct);

            if (imagesResult.IsSuccess)
            {
                imageMap = imagesResult.Value!
                    .Where(x => !x.IsDeleted && !string.IsNullOrWhiteSpace(x.StoragePath))
                    .ToDictionary(x => x.ImageId, x => x.StoragePath);
            }
        }

        var productPrimaryImageUrls = new Dictionary<Guid, string?>();
        foreach (var product in products)
        {
            var primaryRef = product.ImageRefs
                .OrderByDescending(r => r.IsPrimary)
                .ThenBy(r => r.SortOrder)
                .FirstOrDefault();

            if (primaryRef is null || !imageMap.TryGetValue(primaryRef.ImageId, out var storagePath))
            {
                productPrimaryImageUrls[product.Id] = null;
                continue;
            }

            productPrimaryImageUrls[product.Id] = storagePath;
        }

        var vm = new CategoryDetailViewModel
        {
            Category = category,
            Products = products,
            ProductPrimaryImageUrls = productPrimaryImageUrls
        };

        return View(vm);
    }
}
