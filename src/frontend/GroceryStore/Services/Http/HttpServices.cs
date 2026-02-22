using GroceryStore.Models;
using GroceryStore.Services.Interfaces;

namespace GroceryStore.Services.Http;

public sealed class HttpProductService : IProductService
{
    private readonly HttpClient _http;
    public HttpProductService(IHttpClientFactory f)
    {
        _http = f.CreateClient("ApiClient");
    }

    public async Task<PagedResult<Product>> GetProductsAsync(ProductQuery q)
    {
        var result = await _http.GetFromJsonAsync<PagedResult<Product>>($"api/products?{Build(q)}");
        return result ?? new( );
    }

    public async Task<Product?> GetProductByIdAsync(int id)
    {
        return await _http.GetFromJsonAsync<Product>($"api/products/{id}");
    }

    public async Task<Product?> GetProductBySlugAsync(string slug)
    {
        return await _http.GetFromJsonAsync<Product>($"api/products/slug/{slug}");
    }

    public async Task<List<Product>> GetProductsByCategoryAsync(int categoryId)
    {
        return await _http.GetFromJsonAsync<List<Product>>(
                   $"api/products?categoryId={categoryId}&isActive=true&pageSize=100") ?? new( );
    }

    public async Task<List<Product>> GetFeaturedProductsAsync(int limit = 6)
    {
        return await _http.GetFromJsonAsync<List<Product>>(
                   $"api/products?isFeatured=true&isActive=true&pageSize={limit}") ?? new( );
    }

    public async Task<List<Product>> SearchProductsAsync(string query)
    {
        return await _http.GetFromJsonAsync<List<Product>>(
                   $"api/products?search={Uri.EscapeDataString(query)}&isActive=true&pageSize=20") ?? new( );
    }

    public async Task<Product> CreateProductAsync(Product product)
    {
        var r = await _http.PostAsJsonAsync("api/products",product);
        r.EnsureSuccessStatusCode( );
        return (await r.Content.ReadFromJsonAsync<Product>( ))!;
    }

    public async Task<Product> UpdateProductAsync(Product product)
    {
        var r = await _http.PutAsJsonAsync($"api/products/{product.Id}",product);
        r.EnsureSuccessStatusCode( );
        return (await r.Content.ReadFromJsonAsync<Product>( ))!;
    }

    public async Task<bool> DeleteProductAsync(int id)
    {
        return (await _http.DeleteAsync($"api/products/{id}")).IsSuccessStatusCode;
    }

    private static string Build(ProductQuery q)
    {
        var p = new List<string> { $"page={q.Page}",$"pageSize={q.PageSize}",$"sortBy={q.SortBy}" };
        if (q.CategoryId.HasValue)
            p.Add($"categoryId={q.CategoryId}");
        if (q.BrandId.HasValue)
            p.Add($"brandId={q.BrandId}");
        if (q.IsFeatured.HasValue)
            p.Add($"isFeatured={q.IsFeatured.ToString( )!.ToLower( )}");
        if (q.IsActive.HasValue)
            p.Add($"isActive={q.IsActive.ToString( )!.ToLower( )}");
        if (!string.IsNullOrWhiteSpace(q.Search))
            p.Add($"search={Uri.EscapeDataString(q.Search)}");
        return string.Join("&",p);
    }
}
