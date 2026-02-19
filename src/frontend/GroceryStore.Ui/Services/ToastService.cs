namespace GroceryStore.Ui.Services;

public class ToastService : IToastService
{
    public event Func<string, string, Task>? OnShow;

    public async Task SuccessAsync(string message) => await Raise(message, "success");
    public async Task ErrorAsync(string message) => await Raise(message, "error");
    public async Task WarningAsync(string message) => await Raise(message, "warning");
    public async Task InfoAsync(string message) => await Raise(message, "info");

    private async Task Raise(string message, string type)
    {
        if (OnShow is not null)
        {
            await OnShow.Invoke(message, type);
        }
    }
}
