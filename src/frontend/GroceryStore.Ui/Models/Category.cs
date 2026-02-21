namespace GroceryStore.Ui.Models;

public record Category
{
    public string Slug = string.Empty;
    public string Name = string.Empty;
    public string ImageUrl = string.Empty;

    public string? Alt = null;
    public string CardText = "اكتشف المزيد";

    public string Route => $"category/{Slug}";

    public string ImageAlt => string.IsNullOrWhiteSpace(Alt)
        ? Name
        : Alt;
}
