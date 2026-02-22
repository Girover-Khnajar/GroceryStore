using GroceryStore.Models;

namespace GroceryStore.Services.Interfaces;

public interface IProductService
{
    Task<PagedResult<Product>> GetProductsAsync(ProductQuery query);
    Task<Product?> GetProductByIdAsync(int id);
    Task<Product?> GetProductBySlugAsync(string slug);
    Task<List<Product>> GetProductsByCategoryAsync(int categoryId);
    Task<List<Product>> GetFeaturedProductsAsync(int limit = 6);
    Task<List<Product>> SearchProductsAsync(string query);

    // Admin CRUD
    Task<Product> CreateProductAsync(Product product);
    Task<Product> UpdateProductAsync(Product product);
    Task<bool> DeleteProductAsync(int id);
}
