namespace GroceryStore.Web.ViewModels;

public sealed class TestimonialViewModel
{
    public string ClientName { get; init; } = string.Empty;
    public string? ClientTitle { get; init; }
    public string? ClientCompany { get; init; }
    public string? ClientImage { get; init; }
    public byte? Rating { get; init; }
    public string Testimonial { get; init; } = string.Empty;
}
