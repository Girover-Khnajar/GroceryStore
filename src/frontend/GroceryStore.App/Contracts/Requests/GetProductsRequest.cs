namespace GroceryStore.App.Contracts.Requests;

public record GetProductsRequest
{
    public string? Search { get; set; }
    public Guid? CategoryId { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public bool? IsActive { get; set; } = true;
    public bool? IsFeatured { get; set; }
    public string? Brand { get; set; }
    public string? Sort { get; set; } = "newest";
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
