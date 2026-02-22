namespace GroceryStore.Models;

public class Category
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Image { get; set; }
    public bool IsActive { get; set; } = true;
    public int DisplayOrder { get; set; }
}
