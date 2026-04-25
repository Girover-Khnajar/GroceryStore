using CQRS.Abstractions.Messaging;
using GroceryStore.Application.Categories.Dtos;
using GroceryStore.Application.Categories.Queries;
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

        var vm = new CategoryDetailViewModel
        {
            Category = category,
            Products = productsResult.IsSuccess ? productsResult.Value! : []
        };

        return View(vm);
    }
}
