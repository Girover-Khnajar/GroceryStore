namespace GroceryStore.Ui.Services;

public interface IApiClient
{
    Task<T?> GetAsync<T>(string relativeUrl,CancellationToken ct = default);
}