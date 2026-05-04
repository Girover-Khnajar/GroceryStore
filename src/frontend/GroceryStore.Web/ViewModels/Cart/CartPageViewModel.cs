using GroceryStore.Web.Services;

namespace GroceryStore.Web.ViewModels.Cart;

public sealed class CartPageViewModel
{
    public IReadOnlyList<CartItem> Items { get; init; } = [];
}

public sealed class CheckoutInputViewModel
{
    [System.ComponentModel.DataAnnotations.Required]
    [System.ComponentModel.DataAnnotations.MaxLength(120)]
    public string Name { get; set; } = string.Empty;

    [System.ComponentModel.DataAnnotations.Required]
    [System.ComponentModel.DataAnnotations.MaxLength(40)]
    public string PhoneNumber { get; set; } = string.Empty;

    [System.ComponentModel.DataAnnotations.Required]
    [System.ComponentModel.DataAnnotations.MaxLength(300)]
    public string Address { get; set; } = string.Empty;

    public string DeliveryMethod { get; set; } = "pickup";
}

public sealed class CheckoutPageViewModel
{
    public CheckoutInputViewModel Customer { get; init; } = new();
    public IReadOnlyList<CartItem> Items { get; init; } = [];
}
