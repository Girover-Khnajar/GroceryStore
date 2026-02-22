using GroceryStore.Models;
using GroceryStore.Services.Interfaces;

namespace GroceryStore.Services.Http;

public sealed class HttpBrandService : IBrandService
{
    private readonly HttpClient _http;
    public HttpBrandService(IHttpClientFactory f) => _http = f.CreateClient("ApiClient");

    public async Task<List<Brand>> GetBrandsAsync()
    {
        return await _http.GetFromJsonAsync<List<Brand>>("api/brands?isActive=true") ?? new( );
    }

    public async Task<List<Brand>> GetAllBrandsAsync()
    {
        return await _http.GetFromJsonAsync<List<Brand>>("api/brands") ?? new( );
    }

    public async Task<Brand?> GetBrandByIdAsync(int id)
    {
        return await _http.GetFromJsonAsync<Brand>($"api/brands/{id}");
    }

    public async Task<Brand> CreateBrandAsync(Brand brand)
    {
        var r = await _http.PostAsJsonAsync("api/brands",brand);
        r.EnsureSuccessStatusCode( );
        return (await r.Content.ReadFromJsonAsync<Brand>( ))!;
    }

    public async Task<Brand> UpdateBrandAsync(Brand brand)
    {
        var r = await _http.PutAsJsonAsync($"api/brands/{brand.Id}",brand);
        r.EnsureSuccessStatusCode( );
        return (await r.Content.ReadFromJsonAsync<Brand>( ))!;
    }

    public async Task<bool> DeleteBrandAsync(int id)
    {
        return (await _http.DeleteAsync($"api/brands/{id}")).IsSuccessStatusCode;
    }
}
