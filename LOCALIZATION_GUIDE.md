/** LOCALIZATION DOCUMENTATION **/

# Multilingual Support Configuration

This document explains the multilingual functionality implemented in the GroceryStore application.

## Supported Languages

- **English (en)** - Default language, LTR
- **Arabic (ar)** - RTL (Right-to-Left) support
- **French (fr)** - LTR
- **German (de)** - LTR

## 1. Configuration

### Program.cs Setup

The localization is configured in Program.cs with:

```csharp
// Localization services for IStringLocalizer and IViewLocalizer
builder.Services.AddControllersWithViews()
    .AddViewLocalization()
    .AddDataAnnotationsLocalization();

// Localization resource folder
builder.Services.AddLocalization(options => 
    options.ResourcesPath = "Resources");

// Configure cultures and providers
builder.Services.Configure<RequestLocalizationOptions>(options =>
    options.ConfigureLocalization());

// Middleware (must be before routing)
var localizationOptions = app.Services.GetRequiredService<IOptions<RequestLocalizationOptions>>().Value;
app.UseRequestLocalization(localizationOptions);
```

### Culture Provider Priority

1. **Query String**: `?culture=ar` or `?ui-culture=ar`
2. **Cookie**: `GroceryStore.Culture`
3. **Accept-Language Header**: Browser language preference
4. **Default**: English (en)

## 2. Resource File Structure

Resources are located in the `Resources/` folder:

```
Resources/
├── Shared/
│   ├── SharedResource.cs              (Main class)
│   ├── SharedResource.resx            (English)
│   ├── SharedResource.ar.resx         (Arabic)
│   ├── SharedResource.fr.resx         (French)
│   └── SharedResource.de.resx         (German)
├── Controllers/
│   ├── HomeController.resx            (English)
│   ├── HomeController.ar.resx         (Arabic)
│   ├── HomeController.fr.resx         (French)
│   └── HomeController.de.resx         (German)
└── Views/
    ├── Home_Index.resx                (English)
    ├── Home_Index.ar.resx             (Arabic)
    ├── Home_Index.fr.resx             (French)
    └── Home_Index.de.resx             (German)
```

**Naming Convention:**
- Shared resources: `SharedResource.{culture}.resx`
- Controller resources: `{ControllerName}.{culture}.resx`
- View resources: `{Area}_{Controller}_{View}.{culture}.resx` or `{Controller}_{View}.{culture}.resx`

## 3. Using Localization in Controllers

### Inject IStringLocalizer

```csharp
using Microsoft.AspNetCore.Mvc.Localization;

public class YourController : Controller
{
    private readonly IStringLocalizer<YourController> _localizer;
    private readonly IStringLocalizer<SharedResource> _sharedLocalizer;

    public YourController(
        IStringLocalizer<YourController> localizer,
        IStringLocalizer<SharedResource> sharedLocalizer)
    {
        _localizer = localizer;
        _sharedLocalizer = sharedLocalizer;
    }

    public IActionResult Index()
    {
        string title = _localizer["PageTitle_Index"];
        string buttonLabel = _sharedLocalizer["Btn_Submit"];
        
        return View();
    }
}
```

## 4. Using Localization in Razor Views

### Inject IViewLocalizer

```html
@using Microsoft.AspNetCore.Mvc.Localization
@inject IViewLocalizer Localizer
@inject IStringLocalizer<SharedResource> SharedLocalizer

<h1>@Localizer["PageTitle_Index"]</h1>
<button>@SharedLocalizer["Btn_Submit"]</button>
```

### Language Tag Helpers

The lang attribute and dir attribute are automatically set in `_Layout.cshtml`:

```html
<html lang="@currentCulture" dir="@(isRtl ? "rtl" : "ltr")">
```

## 5. Culture Helper Class

Located in `Services/Localization/CultureHelper.cs`

### Key Methods:

```csharp
// Check if culture is supported
CultureHelper.IsSupportedCulture("ar"); // true

// Get supported culture codes
var codes = CultureHelper.SupportedCultureCodes; // ["en", "ar", "fr", "de"]

// Check if RTL
CultureHelper.IsRtlCulture("ar"); // true

// Get display name
CultureHelper.GetDisplayName("ar"); // "العربية"

// Get all available cultures
foreach (var (code, displayName, isRtl) in CultureHelper.GetAvailableCultures())
{
    Console.WriteLine($"{code}: {displayName} (RTL: {isRtl})");
}
```

## 6. Localization Extensions

Located in `Services/Localization/LocalizationExtensions.cs`

### Key Methods:

```csharp
// Set culture cookie
Response.SetCultureCookie("ar");

// Get current culture
string culture = HttpContext.GetCurrentCulture(); // "en" or "ar"

// Get current culture info
CultureInfo cultureInfo = HttpContext.GetCurrentCultureInfo();

// Check if current culture is RTL
bool isRtl = HttpContext.IsCurrentCultureRtl();
```

## 7. Language Switcher

The language switcher is integrated in `_Layout.cshtml`:

```html
<div class="language-switcher">
    <button class="language-toggle" id="languageToggle">
        <i class="fas fa-globe"></i>
        <span>EN</span>
    </button>
    <div class="language-dropdown">
        <a href="@Url.Action("SetCulture", "Culture", new { culture = "ar" })">
            العربية
        </a>
        <!-- More languages -->
    </div>
</div>
```

### Culture Controller

The `CultureController` handles language switching:

```csharp
public class CultureController : Controller
{
    [HttpGet]
    public IActionResult SetCulture(string culture, string? returnUrl = null)
    {
        // Validates culture and sets cookie
        Response.SetCultureCookie(culture);
        return Redirect(returnUrl ?? "/");
    }
}
```

## 8. RTL Support for Arabic

### HTML Direction

The `<html>` tag includes `dir` attribute:

```html
<html lang="@currentCulture" dir="@(isRtl ? "rtl" : "ltr")">
```

### RTL Stylesheet

When Arabic is active, `rtl.css` is loaded:

```html
@if (isRtl)
{
    <link rel="stylesheet" href="~/css/rtl.css" />
}
```

### RTL CSS Handling

All RTL adjustments use `html[dir="rtl"]` selector:

```css
html[dir="rtl"] {
    direction: rtl;
    text-align: right;
}

html[dir="rtl"] .nav-wrapper {
    flex-direction: row-reverse;
}

html[dir="rtl"] input[type="text"] {
    direction: rtl;
    text-align: right;
}
```

## 9. Adding New Translations

### Step 1: Create Resource Files

Add `.resx` files in the appropriate folder:
- `Resources/Controllers/YourController.resx` (English)
- `Resources/Controllers/YourController.ar.resx` (Arabic)
- `Resources/Controllers/YourController.fr.resx` (French)
- `Resources/Controllers/YourController.de.resx` (German)

### Step 2: Add Key-Value Pairs

In Visual Studio's RESX editor or directly in XML:

```xml
<data name="WelcomeMessage" xml:space="preserve">
    <value>Welcome</value>
</data>
```

### Step 3: Use in Code

```csharp
string message = _localizer["WelcomeMessage"];
```

## 10. Data Annotations Localization

Validation messages can be localized:

```csharp
public class ProductViewModel
{
    [Required(ErrorMessageResourceType = typeof(SharedResource), 
              ErrorMessageResourceName = "Validation_Required")]
    public string Name { get; set; }
}
```

Create resource entries:
```xml
<data name="Validation_Required" xml:space="preserve">
    <value>This field is required</value>
</data>
```

## 11. Best Practices

1. **Always provide default (English) resources** - Use `.resx` file without culture code
2. **Use semantic key names** - Prefix with context: `Nav_`, `Btn_`, `Lbl_`, `Msg_`
3. **Centralize shared strings** - Use `SharedResource` for common UI elements
4. **Keep resources organized** - Use separate resource files for each controller/view
5. **Test all languages** - Ensure text fits UI in each language
6. **RTL considerations** - Test layout with Arabic to ensure proper alignment
7. **Culture-aware formatting** - Use `CultureInfo` for dates, numbers, currency:

```csharp
string formattedDate = dateValue.ToString("d", CultureInfo.CurrentCulture);
string formattedCurrency = price.ToString("C", CultureInfo.CurrentCulture);
```

## 12. URL Structure with Cultures

The application supports culture detection via:

- **Query String**: `/products?culture=ar`
- **Cookie**: Persisted across requests
- **Accept-Language**: Browser setting

Routes don't include culture prefix (e.g., `/ar/products`), but this can be implemented with:

```csharp
app.MapControllerRoute(
    name: "culture-default",
    pattern: "{culture=en}/{controller=Home}/{action=Index}/{id?}");
```

## 13. CSS Classes for Localization-Based Styling

```css
html[lang="ar"] { /* Arabic-specific styles */ }
html[dir="rtl"] { /* RTL styles */ }
```

## 14. Common Gotchas

1. **Resource files must be compiled** - Rebuild project after adding resources
2. **Culture codes are case-sensitive** - Use lowercase: "ar", not "AR"
3. **Cookie name is case-sensitive** - "GroceryStore.Culture" not variant
4. **Accept-Language header format** - Browser sends "en-US", app matches on "en"
5. **View localization requires injection** - Remember `@inject IViewLocalizer`
6. **RTL affects layout significantly** - Test flexbox, grid, and margins thoroughly

## 15. Example: Complete Localization Flow

```csharp
// 1. User clicks language switcher dropdown
// 2. Navigates to: /Culture/SetCulture?culture=ar&returnUrl=/products

// 3. CultureController.SetCulture() is called
// 4. Sets cookie: GroceryStore.Culture=c=ar

// 5. Request middleware reads cookie, sets culture to "ar"
// 6. View renders with: <html lang="ar" dir="rtl">
// 7. RTL CSS is loaded
// 8. Resource keys resolve to Arabic values
// 9. User sees Arabic interface with RTL layout

// 10. Next request includes cookie automatically
// 11. Culture persists until cookie expires (1 year)
```

## References

- [ASP.NET Core Localization](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/localization)
- [CultureInfo Class](https://docs.microsoft.com/en-us/dotnet/api/system.globalization.cultureinfo)
- [Right-to-Left Languages](https://web.dev/bidi-html/)
- [RESX File Format](https://docs.microsoft.com/en-us/dotnet/framework/resources/working-with-resx-files-programmatically)
