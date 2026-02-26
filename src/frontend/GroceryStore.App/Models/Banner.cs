namespace GroceryStore.App.Models;

public class Banner
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public List<string> Images { get; set; } = [];
    public string? ImageUrl { get; set; }
    public string? LinkUrl { get; set; }
    public string Placement { get; set; } = "HomeHero";
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool IsActive { get; set; } = true;
    public int DisplayOrder { get; set; }

    // Strips leading '#' so Blazor's NavigationManager can use it directly
    public string? NavRoute
    {
        get
        {
            return string.IsNullOrWhiteSpace(LinkUrl) ? null : LinkUrl.TrimStart('#');
        }
    }
}
