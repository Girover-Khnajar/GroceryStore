using System.Text;
using CQRS.Abstractions.Messaging;
using GroceryStore.Application.Products.Dtos;
using GroceryStore.Application.Products.Queries;
using GroceryStore.Web.Services;
using GroceryStore.Web.ViewModels.Cart;
using Microsoft.AspNetCore.Mvc;

namespace GroceryStore.Web.Controllers;

[Route("Cart")]
public sealed class CartController : Controller
{
    private readonly IMessageDispatcher _dispatcher;
    private readonly ICartService _cartService;
    private readonly IStoreSettingsService _settingsService;

    public CartController(
        IMessageDispatcher dispatcher,
        ICartService cartService,
        IStoreSettingsService settingsService)
    {
        _dispatcher = dispatcher;
        _cartService = cartService;
        _settingsService = settingsService;
    }

    [HttpGet("")]
    public IActionResult Index()
        => View(new CartPageViewModel { Items = _cartService.GetItems() });

    [HttpGet("Add/{productId:guid}")]
    public async Task<IActionResult> Add(Guid productId, string? returnUrl = null, CancellationToken ct = default)
    {
        var result = await _dispatcher
            .QueryAsync<GetProductByIdQuery, ProductDto>(new GetProductByIdQuery(productId), ct);

        if (!result.IsSuccess || result.Value is null)
        {
            TempData["Error"] = "Product not found.";
            return RedirectToAction(nameof(Index));
        }

        var product = result.Value;
        _cartService.Add(new CartItem
        {
            ProductId = product.Id,
            Slug = product.Slug,
            Name = product.Name,
            Brand = product.Brand,
            Sku = product.Sku,
            UnitPrice = product.PriceAmount,
            Currency = product.PriceCurrency,
            Unit = product.Unit,
            Quantity = 1
        });

        TempData["Success"] = $"{product.Name} added to cart.";

        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl);

        return RedirectToAction(nameof(Index));
    }

    [HttpPost("Update")]
    [ValidateAntiForgeryToken]
    public IActionResult Update(Guid productId, int quantity)
    {
        _cartService.UpdateQuantity(productId, quantity);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("Remove")]
    [ValidateAntiForgeryToken]
    public IActionResult Remove(Guid productId)
    {
        _cartService.Remove(productId);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("Clear")]
    [ValidateAntiForgeryToken]
    public IActionResult Clear()
    {
        _cartService.Clear();
        return RedirectToAction(nameof(Index));
    }

    [HttpGet("Checkout")]
    public IActionResult Checkout()
    {
        var items = _cartService.GetItems();
        if (items.Count == 0)
        {
            TempData["Error"] = "Your cart is empty.";
            return RedirectToAction(nameof(Index));
        }

        return View(new CheckoutPageViewModel
        {
            Customer = new CheckoutInputViewModel(),
            Items = items
        });
    }

    [HttpPost("Checkout")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Checkout(CheckoutInputViewModel customer)
    {
        var items = _cartService.GetItems();
        if (items.Count == 0)
        {
            TempData["Error"] = "Your cart is empty.";
            return RedirectToAction(nameof(Index));
        }

        if (!ModelState.IsValid)
        {
            return View(new CheckoutPageViewModel
            {
                Customer = customer,
                Items = items
            });
        }

        var settings = await _settingsService.GetAsync();
        var whatsappNumber = new string((settings.WhatsappNumber ?? string.Empty).Where(char.IsDigit).ToArray());

        if (string.IsNullOrWhiteSpace(whatsappNumber))
        {
            TempData["Error"] = "WhatsApp number is not configured.";
            return RedirectToAction(nameof(Checkout));
        }

        var message = BuildWhatsappMessage(settings.StoreName, customer, items);
        var encoded = Uri.EscapeDataString(message);

        _cartService.Clear();
        return Redirect($"https://wa.me/{whatsappNumber}?text={encoded}");
    }

    private static string BuildWhatsappMessage(
        string? storeName,
        CheckoutInputViewModel customer,
        IReadOnlyList<CartItem> items)
    {
        var sb = new StringBuilder();

        sb.AppendLine($"New Order - {storeName}");
        sb.AppendLine();
        sb.AppendLine("Customer Information:");
        sb.AppendLine($"Name: {customer.Name}");
        sb.AppendLine($"Phone: {customer.PhoneNumber}");
        sb.AppendLine($"Address: {customer.Address}");
        sb.AppendLine();
        sb.AppendLine("Order Items:");

        var index = 1;
        foreach (var item in items)
        {
            sb.AppendLine($"{index}. {item.Name}");
            if (!string.IsNullOrWhiteSpace(item.Brand))
                sb.AppendLine($"   Brand: {item.Brand}");
            if (!string.IsNullOrWhiteSpace(item.Sku))
                sb.AppendLine($"   SKU: {item.Sku}");
            sb.AppendLine($"   Quantity: {item.Quantity}");
            sb.AppendLine($"   Unit Price: {item.UnitPrice:N2} {item.Currency} / {item.Unit}");
            sb.AppendLine($"   Line Total: {item.LineTotal:N2} {item.Currency}");
            sb.AppendLine();
            index++;
        }

        var totalsByCurrency = items
            .GroupBy(x => x.Currency)
            .Select(g => new { Currency = g.Key, Total = g.Sum(x => x.LineTotal) })
            .ToList();

        sb.AppendLine("Totals:");
        foreach (var total in totalsByCurrency)
            sb.AppendLine($"- {total.Total:N2} {total.Currency}");

        return sb.ToString().Trim();
    }
}
