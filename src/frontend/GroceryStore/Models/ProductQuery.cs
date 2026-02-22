namespace GroceryStore.Models;

public class ProductQuery
{
    public int? CategoryId { get; set; }
    public int? BrandId { get; set; }
    public string? Search { get; set; }
    public string SortBy { get; set; } = "newest";
    public bool? IsFeatured { get; set; }
    public bool? IsActive { get; set; } = true;
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 12;
}
