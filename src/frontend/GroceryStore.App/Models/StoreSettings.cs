namespace GroceryStore.App.Models;

public class StoreSettings
{
    public string StoreName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string WhatsappNumber { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string? GoogleMapsUrl { get; set; }
    public string OpeningHours { get; set; } = string.Empty;
    public string Currency { get; set; } = "USD";
}
