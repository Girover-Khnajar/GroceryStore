using GroceryStore.Models;
using GroceryStore.Services.Interfaces;

namespace GroceryStore.Services.Http;

public sealed class HttpDashboardService(IHttpClientFactory f) : IDashboardService
{
    private readonly HttpClient _http = f.CreateClient("ApiClient");

    public async Task<DashboardStats> GetStatsAsync()
    {
        return await _http.GetFromJsonAsync<DashboardStats>("api/dashboard/stats") ?? new( );
    }
}
