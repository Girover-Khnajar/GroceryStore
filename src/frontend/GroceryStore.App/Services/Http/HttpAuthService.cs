using GroceryStore.App.Services.Interfaces;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

namespace GroceryStore.App.Services.Http;

public sealed class HttpAuthService : IAuthService
{
    private readonly HttpClient _http;
    private readonly ProtectedSessionStorage _session;

    private bool _isAuthenticated;
    public bool IsAuthenticated
    {
        get
        {
            return _isAuthenticated;
        }
    }

    public event Action? AuthStateChanged;

    public HttpAuthService(IHttpClientFactory f,ProtectedSessionStorage session)
    {
        _http = f.CreateClient("ApiClient");
        _session = session;
    }

    public async Task<string?> LoginAsync(string username,string password)
    {
        try
        {
            var payload = new { username,password };
            var response = await _http.PostAsJsonAsync("api/auth/login",payload);
            if (!response.IsSuccessStatusCode)
                return null;

            var result = await response.Content.ReadFromJsonAsync<TokenResponse>( );
            if (result?.Token is null)
                return null;

            await _session.SetAsync("admin_token",result.Token);
            SetBearer(result.Token);
            return result.Token;
        }
        catch { return null; }
    }

    public async Task LogoutAsync()
    {
        await _session.DeleteAsync("admin_token");
        _isAuthenticated = false;
        _http.DefaultRequestHeaders.Authorization = null;
        AuthStateChanged?.Invoke( );
    }

    /// <summary>
    /// Call once in OnAfterRenderAsync to restore the session from browser storage.
    /// Browser storage is unavailable during SSR pre-render.
    /// </summary>
    public async Task RestoreSessionAsync()
    {
        try
        {
            var result = await _session.GetAsync<string>("admin_token");
            if (result.Success && !string.IsNullOrWhiteSpace(result.Value))
                SetBearer(result.Value);
        }
        catch { /* storage unavailable during pre-render */ }
    }

    private void SetBearer(string token)
    {
        _isAuthenticated = true;
        _http.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer",token);
        AuthStateChanged?.Invoke( );
    }

    private sealed class TokenResponse { public string? Token { get; set; } }
}
