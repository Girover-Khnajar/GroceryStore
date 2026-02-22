using GroceryStore.Models;

namespace GroceryStore.Services.Interfaces;

public interface ISettingsService
{
    Task<StoreSettings?> GetSettingsAsync();
    Task<StoreSettings> SaveSettingsAsync(StoreSettings settings);
}
