namespace GroceryStore.App.Models;

/// <summary>Represents a product in the grocery store catalog.</summary>
public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public int CategoryId { get; set; }
    public int BrandId { get; set; }
    public decimal Price { get; set; }
    public string Currency { get; set; } = "USD";
    public string Unit { get; set; } = string.Empty;
    public string Sku { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsFeatured { get; set; }
    public List<string> Images { get; set; } = new( );
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Populated by the service layer after joining with categories/brands
    public string? CategoryName { get; set; }
    public string? BrandName { get; set; }

    // Computed helpers used directly in Razor templates
    public string? PrimaryImage => Images.Count > 0 ? Images[0] : null;
    public string FormattedPrice => $"{Price:0.##} {Currency} / {Unit}";
}
