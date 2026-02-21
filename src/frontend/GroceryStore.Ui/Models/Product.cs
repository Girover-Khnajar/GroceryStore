namespace GroceryStore.Ui.Models;

public record Product
{
    public string Category = string.Empty;
    public string Name = string.Empty;
    public decimal CurrentPrice = 0m;
    public string Unit = string.Empty;
    public string Currency = "IQD";
    public string PriceUnit => $"{Currency}/{Unit}";
    public string ImageUrl = string.Empty;
}