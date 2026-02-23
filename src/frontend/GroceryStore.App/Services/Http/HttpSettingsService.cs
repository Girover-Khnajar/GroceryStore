using GroceryStore.App.Models;
using GroceryStore.App.Services.Interfaces;

namespace GroceryStore.App.Services.Http;

public sealed class HttpSettingsService : ISettingsService
{
    private readonly HttpClient _http;
    public HttpSettingsService(IHttpClientFactory f)
    {
        _http = f.CreateClient("ApiClient");
    }

    public async Task<StoreSettings?> GetSettingsAsync()
    {
        return await _http.GetFromJsonAsync<StoreSettings>("api/settings");
    }

    public async Task<StoreSettings> SaveSettingsAsync(StoreSettings settings)
    {
        var r = await _http.PutAsJsonAsync("api/settings",settings);
        r.EnsureSuccessStatusCode( );
        return (await r.Content.ReadFromJsonAsync<StoreSettings>( ))!;
    }
}
