using GroceryStore.Web.Services.Localization;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;

namespace GroceryStore.Web.Controllers;

/// <summary>
/// Controller for managing culture/language selection and switching.
/// </summary>
[Route("[controller]/[action]")]
public class CultureController : Controller
{
    /// <summary>
    /// Sets the language/culture and redirects back to the referer URL.
    /// Usage: /Culture/SetCulture?culture=ar&returnUrl=%2Fproducts
    /// </summary>
    [HttpGet]
    public IActionResult SetCulture(string culture, string? returnUrl = null)
    {
        // Validate culture
        if (!CultureHelper.IsSupportedCulture(culture))
            culture = CultureHelper.DefaultCultureCode;

        // Set the culture cookie
        Response.SetCultureCookie(culture);

        // Redirect to referer or home
        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl);

        return Redirect("/");
    }

    /// <summary>
    /// Gets a JSON response with available cultures.
    /// Useful for AJAX requests or building dynamic language switchers.
    /// </summary>
    [HttpGet]
    [Produces("application/json")]
    public IActionResult GetAvailableCultures()
    {
        var cultures = CultureHelper.GetAvailableCultures()
            .Select(c => new
            {
                c.Code,
                c.DisplayName,
                c.IsRtl,
                IsActive = c.Code == HttpContext.GetCurrentCulture()
            })
            .ToList();

        return Json(cultures);
    }
}
