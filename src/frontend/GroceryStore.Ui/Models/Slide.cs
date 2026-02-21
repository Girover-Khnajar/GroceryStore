namespace GroceryStore.Ui.Models;

public record Slide(string ImageUrl, string Title, string Link)
{
    public string ImageUrl { get; } = ImageUrl;
    public string Title { get; } = Title;
    public string Link { get; } = Link;
    public bool Active { get; set; } = true;
}