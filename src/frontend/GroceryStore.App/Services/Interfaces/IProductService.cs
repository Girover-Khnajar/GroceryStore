using GroceryStore.App.Contracts.Requests;
using GroceryStore.App.Models;

namespace GroceryStore.App.Services.Interfaces;

public interface IProductService
{
    Task<PagedResult<Product>> GetProductsAsync(GetProductsRequest query);
    Task<Product?> GetProductByIdAsync(Guid id);
    Task<Product?> GetProductBySlugAsync(string slug);
    Task<List<Product>> GetProductsByCategoryAsync(Guid categoryId);
    Task<List<Product>> GetFeaturedProductsAsync(int limit = 6);
    Task<List<Product>> SearchProductsAsync(string query);

    // Admin CRUD
    Task<Product> CreateProductAsync(Product product);
    Task<Product> UpdateProductAsync(Product product);
    Task<bool> DeleteProductAsync(Guid id);
}
