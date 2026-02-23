using GroceryStore.App.Models;

namespace GroceryStore.App.Services.Interfaces;

public interface IDashboardService
{
    Task<DashboardStats> GetStatsAsync();
}
