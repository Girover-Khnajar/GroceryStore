namespace GroceryStore.Web.ViewModels;

public class ContactViewModel
{
    public string  StoreName      { get; set; } = string.Empty;
    public string  Phone          { get; set; } = string.Empty;
    public string  WhatsappNumber { get; set; } = string.Empty;
    public string  Email          { get; set; } = string.Empty;
    public string  Address        { get; set; } = string.Empty;
    public string  OpeningHours   { get; set; } = string.Empty;
    public string? GoogleMapsUrl  { get; set; }
}
