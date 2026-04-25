using System.ComponentModel.DataAnnotations;

namespace GroceryStore.Web.ViewModels.Admin;

public class StoreSettingsViewModel
{
    [Display(Name = "Store Name")]
    public string StoreName { get; set; } = string.Empty;

    [Display(Name = "Phone")]
    public string Phone { get; set; } = string.Empty;

    [Display(Name = "WhatsApp Number")]
    public string WhatsappNumber { get; set; } = string.Empty;

    [Display(Name = "Email")]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Display(Name = "Address")]
    public string Address { get; set; } = string.Empty;

    [Display(Name = "Opening Hours")]
    public string OpeningHours { get; set; } = string.Empty;

    [Display(Name = "Google Maps Embed URL")]
    public string? GoogleMapsUrl { get; set; }

    [Display(Name = "Currency")]
    public string Currency { get; set; } = "USD";
}
