using GroceryStore.Models;

namespace GroceryStore.Services.Interfaces;

public interface IDashboardService
{
    Task<DashboardStats> GetStatsAsync();
}
