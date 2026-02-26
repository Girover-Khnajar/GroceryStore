using GroceryStore.App.Models;
using GroceryStore.App.Services.Interfaces;

namespace GroceryStore.App.Services.Http;

public sealed class HttpBannerService : IBannerService
{
    private readonly HttpClient _http;
    public HttpBannerService(IHttpClientFactory f)
    {
        _http = f.CreateClient("ApiClient");
    }

    public async Task<List<Banner>> GetBannersAsync()
    {
        return [.. (await _http.GetFromJsonAsync<List<Banner>>("api/banners?isActive=true") ?? new( )).OrderBy(b => b.DisplayOrder)];
    }

    public async Task<List<Banner>> GetAllBannersAsync()
    {
        return [.. (await _http.GetFromJsonAsync<List<Banner>>("api/banners") ?? new( )).OrderBy(b => b.DisplayOrder)];
    }

    public async Task<Banner?> GetBannerByIdAsync(Guid id)
    {
        return await _http.GetFromJsonAsync<Banner>($"api/banners/{id}");
    }

    public async Task<Banner> CreateBannerAsync(Banner banner)
    {
        var r = await _http.PostAsJsonAsync("api/banners",banner);
        r.EnsureSuccessStatusCode( );
        return (await r.Content.ReadFromJsonAsync<Banner>( ))!;
    }

    public async Task<Banner> UpdateBannerAsync(Banner banner)
    {
        var r = await _http.PutAsJsonAsync($"api/banners/{banner.Id}",banner);
        r.EnsureSuccessStatusCode( );
        return (await r.Content.ReadFromJsonAsync<Banner>( ))!;
    }

    public async Task<bool> DeleteBannerAsync(Guid id)
    {
        return (await _http.DeleteAsync($"api/banners/{id}")).IsSuccessStatusCode;
    }
}
