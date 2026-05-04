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
        const string nl = "\n";
        var sb = new StringBuilder();
        var orderNumber = new Random().Next(1000, 9999);
        var store = storeName ?? "Épicerie";

        var deliveryLabel = customer.DeliveryMethod == "delivery"
            ? "Livraison à domicile"
            : "Retrait en magasin";

        const string tableLine = "+------------------------+";
        const string dottedLine = "........................";

        sb.Append("COMMANDE" + nl);
        sb.Append(tableLine + nl);
        sb.Append($"| Magasin : {store}" + nl);
        sb.Append($"| Numero  : #{orderNumber}" + nl);
        sb.Append(dottedLine + nl);
        sb.Append($"| Client  : {customer.Name}" + nl);
        sb.Append($"| Tel     : {customer.PhoneNumber}" + nl);
        sb.Append($"| Adresse : {customer.Address}" + nl);
        sb.Append($"| Methode : {deliveryLabel}" + nl);
        sb.Append(tableLine + nl);
        sb.Append("ARTICLES" + nl);
        sb.Append(tableLine + nl);

        foreach (var item in items)
        {
            var currency = item.Currency;
            sb.Append($"| {item.Name}" + nl);
            sb.Append($"| {item.Quantity} {item.Unit} x {item.UnitPrice:N2} {currency}" + nl);
            sb.Append($"| Total : {item.LineTotal:N2} {currency}" + nl);
            sb.Append(dottedLine + nl);
        }

        sb.Append(tableLine + nl);
        sb.Append("TOTAUX" + nl);
        sb.Append(tableLine + nl);

        // Totals per currency
        foreach (var g in items.GroupBy(x => x.Currency))
        {
            var subtotal = g.Sum(x => x.LineTotal);
            sb.Append($"| Devise      : {g.Key}" + nl);
            sb.Append($"| Sous-total  : {subtotal:N2} {g.Key}" + nl);
            sb.Append($"| Total final : {subtotal:N2} {g.Key}" + nl);
            sb.Append(dottedLine + nl);
        }

        sb.Append(tableLine + nl);
        sb.Append($"Merci de votre confiance en {store}");

        return sb.ToString().Trim();
    }
}
