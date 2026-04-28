using Microsoft.AspNetCore.Localization;
using System.Globalization;

namespace GroceryStore.Web.Services.Localization;

/// <summary>
/// Extension methods for localization configuration.
/// </summary>
public static class LocalizationExtensions
{
    /// <summary>
    /// Configures localization with QueryString, Cookie, and Accept-Language providers.
    /// </summary>
    public static RequestLocalizationOptions ConfigureLocalization(
        this RequestLocalizationOptions options)
    {
        var supportedCultures = CultureHelper.SupportedCultures.ToArray();
        
        options.DefaultRequestCulture = new RequestCulture(
            culture: CultureHelper.DefaultCulture,
            uiCulture: CultureHelper.DefaultCulture);

        options.SupportedCultures = supportedCultures;
        options.SupportedUICultures = supportedCultures;

        // Providers in order of priority: Query String > Cookie > Accept-Language header
        options.RequestCultureProviders = new IRequestCultureProvider[]
        {
            new QueryStringRequestCultureProvider { QueryStringKey = "culture", UIQueryStringKey = "ui-culture" },
            new CookieRequestCultureProvider { CookieName = "GroceryStore.Culture" },
            new AcceptLanguageHeaderRequestCultureProvider(),
        };

        return options;
    }

    /// <summary>
    /// Sets the culture cookie in the HTTP response.
    /// Cookie name: GroceryStore.Culture
    /// Cookie format: c={cultureName}
    /// </summary>
    public static void SetCultureCookie(this HttpResponse response, string cultureName)
    {
        if (!CultureHelper.IsSupportedCulture(cultureName))
            cultureName = CultureHelper.DefaultCultureCode;

        var cookieValue = CookieRequestCultureProvider.MakeCookieValue(
            new RequestCulture(cultureName));

        response.Cookies.Append(
            "GroceryStore.Culture",
            cookieValue,
            new CookieOptions
            {
                Expires = DateTimeOffset.UtcNow.AddYears(1),
                HttpOnly = true,
                SameSite = SameSiteMode.Lax,
                IsEssential = true
            });
    }

    /// <summary>
    /// Gets the current culture from the HttpContext.
    /// </summary>
    public static string GetCurrentCulture(this HttpContext httpContext)
    {
        var feature = httpContext.Features.Get<IRequestCultureFeature>();
        return feature?.RequestCulture.Culture.Name ?? CultureHelper.DefaultCultureCode;
    }

    /// <summary>
    /// Gets the current UI culture from the HttpContext.
    /// </summary>
    public static CultureInfo GetCurrentCultureInfo(this HttpContext httpContext)
    {
        var feature = httpContext.Features.Get<IRequestCultureFeature>();
        return feature?.RequestCulture.Culture ?? CultureHelper.DefaultCulture;
    }

    /// <summary>
    /// Checks if the current culture is RTL.
    /// </summary>
    public static bool IsCurrentCultureRtl(this HttpContext httpContext)
    {
        var cultureInfo = httpContext.GetCurrentCultureInfo();
        return CultureHelper.IsRtlCulture(cultureInfo);
    }
}
