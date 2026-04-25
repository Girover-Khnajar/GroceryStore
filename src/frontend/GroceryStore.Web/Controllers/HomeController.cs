using CQRS.Abstractions.Messaging;
using GroceryStore.Application.Categories.Dtos;
using GroceryStore.Application.Categories.Queries;
using GroceryStore.Application.Common;
using GroceryStore.Application.Products.Dtos;
using GroceryStore.Application.Products.Queries.GetProducts;
using GroceryStore.Web.Services;
using GroceryStore.Web.ViewModels;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace GroceryStore.Web.Controllers;

public class HomeController : Controller
{
    private readonly IMessageDispatcher _dispatcher;
    private readonly IStoreSettingsService _settingsService;

    public HomeController(
        IMessageDispatcher dispatcher,
        IStoreSettingsService settingsService)
    {
        _dispatcher = dispatcher;
        _settingsService = settingsService;
    }

    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var categoriesResult = await _dispatcher
            .QueryAsync<GetAllActiveCategoriesQuery, IReadOnlyList<CategoryDto>>(
                new GetAllActiveCategoriesQuery(), ct);

        var productsResult = await _dispatcher
            .QueryAsync<GetProductsQuery, PagedResult<ProductListItemDto>>(
                new GetProductsQuery(
                    Search: null,
                    CategoryId: null,
                    MinPrice: null,
                    MaxPrice: null,
                    IsActive: true,
                    IsFeatured: true,
                    Brand: null,
                    Sort: null,
                    Page: 1,
                    PageSize: 6), ct);

        var vm = new HomeViewModel
        {
            Categories = categoriesResult.IsSuccess
                ? categoriesResult.Value! : [],
            FeaturedProducts = productsResult.IsSuccess
                ? productsResult.Value!.Items : []
        };

        return View(vm);
    }

    [Route("contact")]
    public async Task<IActionResult> Contact(CancellationToken ct)
    {
        var settings = await _settingsService.GetAsync(ct);

        var vm = new ContactViewModel
        {
            StoreName = settings.StoreName,
            Phone = settings.Phone,
            WhatsappNumber = settings.WhatsappNumber,
            Email = settings.Email,
            Address = settings.Address,
            OpeningHours = settings.OpeningHours,
            GoogleMapsUrl = settings.GoogleMapsUrl,
        };

        return View(vm);
    }

    [Route("Error")]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error() => View();

    [Route("Error/{statusCode:int}")]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult StatusCodePage(int statusCode)
    {
        Response.StatusCode = statusCode;

        if (statusCode == 404)
        {
            var reExecuteFeature = HttpContext.Features.Get<IStatusCodeReExecuteFeature>();
            ViewData["OriginalPath"] = reExecuteFeature?.OriginalPath;
            ViewData["OriginalQueryString"] = reExecuteFeature?.OriginalQueryString;
            return View("NotFound");
        }

        return View("Error");
    }
}
