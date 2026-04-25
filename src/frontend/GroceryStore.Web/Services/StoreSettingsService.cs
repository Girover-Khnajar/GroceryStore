using System.Text.Json;
using GroceryStore.Web.ViewModels.Admin;

namespace GroceryStore.Web.Services;

public sealed class StoreSettingsService : IStoreSettingsService
{
    private readonly IConfiguration _config;
    private readonly string _filePath;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true
    };

    public StoreSettingsService(IConfiguration config, IWebHostEnvironment env)
    {
        _config = config;
        _filePath = Path.Combine(env.ContentRootPath, "App_Data", "store-settings.json");
    }

    public async Task<StoreSettingsViewModel> GetAsync(CancellationToken ct = default)
    {
        if (!File.Exists(_filePath))
            return DefaultFromConfig();

        try
        {
            await using var stream = File.OpenRead(_filePath);
            var model = await JsonSerializer.DeserializeAsync<StoreSettingsViewModel>(stream, JsonOptions, ct);
            return model ?? DefaultFromConfig();
        }
        catch
        {
            return DefaultFromConfig();
        }
    }

    public async Task SaveAsync(StoreSettingsViewModel settings, CancellationToken ct = default)
    {
        var dir = Path.GetDirectoryName(_filePath)!;
        Directory.CreateDirectory(dir);

        await using var stream = File.Create(_filePath);
        await JsonSerializer.SerializeAsync(stream, settings, JsonOptions, ct);
    }

    private StoreSettingsViewModel DefaultFromConfig() => new()
    {
        StoreName = _config["StoreSettings:StoreName"] ?? "Fresh Grocery Store",
        Phone = _config["StoreSettings:Phone"] ?? string.Empty,
        WhatsappNumber = _config["StoreSettings:WhatsappNumber"] ?? string.Empty,
        Email = _config["StoreSettings:Email"] ?? string.Empty,
        Address = _config["StoreSettings:Address"] ?? string.Empty,
        OpeningHours = _config["StoreSettings:OpeningHours"] ?? string.Empty,
        GoogleMapsUrl = _config["StoreSettings:GoogleMapsUrl"],
        Currency = _config["StoreSettings:Currency"] ?? "USD"
    };
}
