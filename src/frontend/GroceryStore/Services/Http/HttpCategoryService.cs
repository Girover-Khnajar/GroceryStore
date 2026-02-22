using GroceryStore.Models;
using GroceryStore.Services.Interfaces;

namespace GroceryStore.Services.Http;

public sealed class HttpCategoryService : ICategoryService
{
    private readonly HttpClient _http;
    public HttpCategoryService(IHttpClientFactory f) => _http = f.CreateClient("ApiClient");

    public async Task<List<Category>> GetCategoriesAsync()
    {
        return await _http.GetFromJsonAsync<List<Category>>("api/categories?isActive=true") ?? new( );
    }

    public async Task<List<Category>> GetAllCategoriesAsync()
    {
        return await _http.GetFromJsonAsync<List<Category>>("api/categories") ?? new( );
    }

    public async Task<Category?> GetCategoryByIdAsync(int id)
    {
        return await _http.GetFromJsonAsync<Category>($"api/categories/{id}");
    }

    public async Task<Category?> GetCategoryBySlugAsync(string slug)
    {
        return await _http.GetFromJsonAsync<Category>($"api/categories/slug/{slug}");
    }

    public async Task<Category> CreateCategoryAsync(Category category)
    {
        var r = await _http.PostAsJsonAsync("api/categories",category);
        r.EnsureSuccessStatusCode( );
        return (await r.Content.ReadFromJsonAsync<Category>( ))!;
    }

    public async Task<Category> UpdateCategoryAsync(Category category)
    {
        var r = await _http.PutAsJsonAsync($"api/categories/{category.Id}",category);
        r.EnsureSuccessStatusCode( );
        return (await r.Content.ReadFromJsonAsync<Category>( ))!;
    }

    public async Task<bool> DeleteCategoryAsync(int id)
    {
        return (await _http.DeleteAsync($"api/categories/{id}")).IsSuccessStatusCode;
    }
}
