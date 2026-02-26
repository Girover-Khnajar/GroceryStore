using GroceryStore.App.Models;

namespace GroceryStore.App.Services.Interfaces;

public interface IBrandService
{
    Task<List<Brand>> GetBrandsAsync();       // active only
    Task<List<Brand>> GetAllBrandsAsync();    // admin — includes inactive

    Task<Brand?> GetBrandByIdAsync(Guid id);

    // Admin CRUD
    Task<Brand> CreateBrandAsync(Brand brand);
    Task<Brand> UpdateBrandAsync(Brand brand);
    Task<bool> DeleteBrandAsync(Guid id);
}
