using GroceryStore.Web.ViewModels.Admin;

namespace GroceryStore.Web.Services;

public interface IStoreSettingsService
{
    Task<StoreSettingsViewModel> GetAsync(CancellationToken ct = default);
    Task SaveAsync(StoreSettingsViewModel settings, CancellationToken ct = default);
}
