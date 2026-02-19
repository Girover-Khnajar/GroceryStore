namespace GroceryStore.Ui.Services;

public interface IToastService
{
    event Func<string, string, Task>? OnShow;
    Task SuccessAsync(string message);
    Task ErrorAsync(string message);
    Task WarningAsync(string message);
    Task InfoAsync(string message);
}
