using Microsoft.Extensions.Options;

namespace GroceryStore.Ui.Services;

public sealed class ApiClient : IApiClient
{
    private readonly HttpClient _http;

    public ApiClient(HttpClient http,IOptions<ApiOptions> options)
    {
        _http = http;
        var baseUrl = options.Value.BaseUrl?.TrimEnd('/');
        _http.BaseAddress = new Uri($"{baseUrl}/");
    }

    public Task<T?> GetAsync<T>(string relativeUrl,CancellationToken ct = default)
        => _http.GetFromJsonAsync<T>(relativeUrl.TrimStart('/'),ct);
}
