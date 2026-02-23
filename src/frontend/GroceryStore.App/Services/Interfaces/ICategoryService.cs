using GroceryStore.App.Models;

namespace GroceryStore.App.Services.Interfaces;

public interface ICategoryService
{
    Task<List<Category>> GetCategoriesAsync();       // active only
    Task<List<Category>> GetAllCategoriesAsync();    // admin — includes inactive

    Task<Category?> GetCategoryByIdAsync(int id);
    Task<Category?> GetCategoryBySlugAsync(string slug);

    // Admin CRUD
    Task<Category> CreateCategoryAsync(Category category);
    Task<Category> UpdateCategoryAsync(Category category);
    Task<bool> DeleteCategoryAsync(int id);
}
