namespace GroceryStore.App.Models;

public class Category
{
    public Guid Id { get; set; }
    public Guid? ParentCategoryId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Image { get; set; }
    public bool IsActive { get; set; } = true;
    public int SortOrder { get; set; } = 0;
    public string? ImageUrl {get; set;} = string.Empty;
    public string? IconName {get; set;} = string.Empty;
    public string? SeoMetaTitle {get; set;} = string.Empty;
    public string? SeoMetaDescription {get; set;} = string.Empty;
}
