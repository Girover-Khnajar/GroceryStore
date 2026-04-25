namespace GroceryStore.Web.Services;

public sealed class CartItem
{
    public Guid ProductId { get; set; }
    public string Slug { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Brand { get; set; }
    public string? Sku { get; set; }
    public decimal UnitPrice { get; set; }
    public string Currency { get; set; } = "USD";
    public string Unit { get; set; } = "Piece";
    public int Quantity { get; set; } = 1;

    public decimal LineTotal => UnitPrice * Quantity;
}
