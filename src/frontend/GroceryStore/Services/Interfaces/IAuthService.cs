namespace GroceryStore.Services.Interfaces;

public interface IAuthService
{
    bool IsAuthenticated { get; }
    event Action? AuthStateChanged;

    /// <summary>Returns a token string on success, null on failure.</summary>
    Task<string?> LoginAsync(string username,string password);
    Task LogoutAsync();
}
