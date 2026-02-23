using GroceryStore.App.Models;

namespace GroceryStore.App.Services.Interfaces;

public interface IBannerService
{
    Task<List<Banner>> GetBannersAsync();      // active only, sorted by DisplayOrder
    Task<List<Banner>> GetAllBannersAsync();   // admin

    Task<Banner?> GetBannerByIdAsync(int id);

    // Admin CRUD
    Task<Banner> CreateBannerAsync(Banner banner);
    Task<Banner> UpdateBannerAsync(Banner banner);
    Task<bool> DeleteBannerAsync(int id);
}
