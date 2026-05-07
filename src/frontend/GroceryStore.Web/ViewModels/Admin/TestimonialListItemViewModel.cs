namespace GroceryStore.Web.ViewModels.Admin;

public sealed class TestimonialListItemViewModel
{
    public Guid Id { get; init; }
    public string ClientName { get; init; } = string.Empty;
    public string? ClientTitle { get; init; }
    public string? ClientCompany { get; init; }
    public string? ClientImage { get; init; }
    public byte? Rating { get; init; }
    public string Testimonial { get; init; } = string.Empty;
    public int SortOrder { get; init; }
    public bool IsActive { get; init; }
}
