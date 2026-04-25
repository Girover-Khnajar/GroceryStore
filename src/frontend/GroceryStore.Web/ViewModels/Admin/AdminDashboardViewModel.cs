using GroceryStore.Application.Categories.Dtos;
using GroceryStore.Application.Products.Dtos;

namespace GroceryStore.Web.ViewModels.Admin;

public sealed class AdminDashboardViewModel
{
    public int TotalCategories { get; init; }
    public int TotalProducts { get; init; }
    public int ActiveProducts { get; init; }
    public int FeaturedProducts { get; init; }
    public IReadOnlyList<CategoryDto> RecentCategories { get; init; } = [];
}
