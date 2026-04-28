using System.Globalization;

namespace GroceryStore.Web.Services.Localization;

/// <summary>
/// Helper class for managing cultures and localization operations.
/// </summary>
public static class CultureHelper
{
    /// <summary>
    /// Supported cultures for the application.
    /// </summary>
    public static readonly List<CultureInfo> SupportedCultures = new()
    {
        new CultureInfo("en"), // English (default)
        new CultureInfo("ar"), // Arabic (RTL)
        new CultureInfo("fr"), // French
        new CultureInfo("de"), // German
    };

    /// <summary>
    /// Gets the list of supported culture codes.
    /// </summary>
    public static List<string> SupportedCultureCodes => 
        SupportedCultures.Select(c => c.Name).ToList();

    /// <summary>
    /// Checks if a culture code is supported.
    /// </summary>
    public static bool IsSupportedCulture(string cultureName) =>
        SupportedCultureCodes.Contains(cultureName ?? string.Empty);

    /// <summary>
    /// Gets the default culture info.
    /// </summary>
    public static CultureInfo DefaultCulture => new("en");

    /// <summary>
    /// Gets the default culture code.
    /// </summary>
    public static string DefaultCultureCode => "en";

    /// <summary>
    /// Gets a culture by code. Returns default culture if not found.
    /// </summary>
    public static CultureInfo GetCulture(string? cultureName)
    {
        if (string.IsNullOrWhiteSpace(cultureName))
            return DefaultCulture;

        cultureName = cultureName.ToLowerInvariant();
        
        var culture = SupportedCultures.FirstOrDefault(c => 
            c.Name.Equals(cultureName, StringComparison.OrdinalIgnoreCase));
        
        return culture ?? DefaultCulture;
    }

    /// <summary>
    /// Checks if a culture is RTL (Right-To-Left).
    /// </summary>
    public static bool IsRtlCulture(string? cultureName)
    {
        var cultureCode = (cultureName ?? DefaultCultureCode).ToLowerInvariant();
        return cultureCode == "ar"; // Arabic is RTL
    }

    /// <summary>
    /// Checks if a culture is RTL by CultureInfo.
    /// </summary>
    public static bool IsRtlCulture(CultureInfo? cultureInfo)
    {
        if (cultureInfo?.Name == null)
            return false;

        // TextInfo.IsRightToLeft is the proper way to check
        return cultureInfo.TextInfo.IsRightToLeft;
    }

    /// <summary>
    /// Gets the display name of a culture in English.
    /// </summary>
    public static string GetDisplayName(string cultureName)
    {
        return cultureName?.ToLowerInvariant() switch
        {
            "en" => "English",
            "ar" => "العربية",
            "fr" => "Français",
            "de" => "Deutsch",
            _ => "English"
        };
    }

    /// <summary>
    /// Gets all available cultures with their display information.
    /// </summary>
    public static IEnumerable<(string Code, string DisplayName, bool IsRtl)> GetAvailableCultures()
    {
        foreach (var culture in SupportedCultures)
        {
            yield return (
                Code: culture.Name,
                DisplayName: GetDisplayName(culture.Name),
                IsRtl: IsRtlCulture(culture)
            );
        }
    }
}
