using GroceryStore.App.Models;

namespace GroceryStore.App.Services.Interfaces;

public interface ISettingsService
{
    Task<StoreSettings?> GetSettingsAsync();
    Task<StoreSettings> SaveSettingsAsync(StoreSettings settings);
}
