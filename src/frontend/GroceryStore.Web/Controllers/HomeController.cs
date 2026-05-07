using CQRS.Abstractions.Messaging;
using GroceryStore.Application.Categories.Dtos;
using GroceryStore.Application.Categories.Queries;
using GroceryStore.Application.Common;
using GroceryStore.Application.Products.Dtos;
using GroceryStore.Application.Products.Queries.GetProducts;
using GroceryStore.Domain.Interfaces;
using GroceryStore.Web.Services;
using GroceryStore.Web.ViewModels;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace GroceryStore.Web.Controllers;

public class HomeController : Controller
{
    private readonly IMessageDispatcher _dispatcher;
    private readonly IStoreSettingsService _settingsService;
    private readonly ITestimonialRepository _testimonialRepository;

    public HomeController(
        IMessageDispatcher dispatcher,
        IStoreSettingsService settingsService,
        ITestimonialRepository testimonialRepository)
    {
        _dispatcher = dispatcher;
        _settingsService = settingsService;
        _testimonialRepository = testimonialRepository;
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

        var testimonials = await _testimonialRepository.GetActiveAsync(ct);

        var vm = new HomeViewModel
        {
            Categories = categoriesResult.IsSuccess
                ? categoriesResult.Value! : [],
            FeaturedProducts = productsResult.IsSuccess
                ? productsResult.Value!.Items : [],
            Testimonials = testimonials
                .Select(t => new TestimonialViewModel
                {
                    ClientName = t.ClientName,
                    ClientTitle = t.ClientTitle,
                    ClientCompany = t.ClientCompany,
                    ClientImage = t.ClientImage,
                    Rating = t.Rating,
                    Testimonial = t.TestimonialText,
                })
                .ToList()
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
