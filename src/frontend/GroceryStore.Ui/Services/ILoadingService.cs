namespace GroceryStore.Ui.Services;

public interface ILoadingService
{
    event Action? OnChange;
    bool IsLoading { get; }
    void Show();
    void Hide();
}
