# Localization Quick Start Guide

Get up and running with multilingual support in 5 minutes!

## 🚀 Quick Setup (Already Done)

The localization infrastructure is already configured in `Program.cs`. You just need to:

1. Create `.resx` files
2. Add translations
3. Use `IStringLocalizer` in code
4. Reference keys in views

## 📋 5-Minute Example

### Step 1: Create Resource Files (2 min)

Create these files in `Resources/Controllers/`:

**HomeController.resx** (English):
```xml
<?xml version="1.0" encoding="utf-8"?>
<root>
  <data name="Welcome" xml:space="preserve">
    <value>Welcome to GroceryStore</value>
  </data>
  <data name="Btn_Shop" xml:space="preserve">
    <value>Start Shopping</value>
  </data>
</root>
```

**HomeController.ar.resx** (Arabic):
```xml
<?xml version="1.0" encoding="utf-8"?>
<root>
  <data name="Welcome" xml:space="preserve">
    <value>مرحبا بك في متجر البقالة</value>
  </data>
  <data name="Btn_Shop" xml:space="preserve">
    <value>ابدأ التسوق</value>
  </data>
</root>
```

### Step 2: Update Controller (1 min)

```csharp
using Microsoft.Extensions.Localization;

public class HomeController : Controller
{
    private readonly IStringLocalizer<HomeController> _localizer;

    public HomeController(IStringLocalizer<HomeController> localizer)
    {
        _localizer = localizer;
    }

    public IActionResult Index()
    {
        ViewBag.Welcome = _localizer["Welcome"];
        ViewBag.ShopButtonLabel = _localizer["Btn_Shop"];
        return View();
    }
}
```

### Step 3: Update View (1 min)

```html
@using Microsoft.AspNetCore.Mvc.Localization
@inject IViewLocalizer Localizer

<h1>@ViewBag.Welcome</h1>
<button>@Localizer["Btn_Shop"]</button>

<!-- Or with direct injection: -->
<p>@Localizer["Welcome"]</p>
```

### Step 4: Rebuild and Test (1 min)

1. **Build** the solution (`Ctrl+Shift+B`)
2. **Run** the application
3. **Test** languages:
   - `http://localhost:5000/?culture=en`
   - `http://localhost:5000/?culture=ar`

✅ **Done!** Your first translation works!

## 📝 Common Tasks

### Task: Localize a Form

```csharp
// Controller
ViewBag.NameLabel = _localizer["Lbl_Name"];
ViewBag.EmailLabel = _localizer["Lbl_Email"];
ViewBag.SubmitText = _localizer["Btn_Submit"];
```

**Resources:**
```xml
<!-- English -->
<data name="Lbl_Name" xml:space="preserve"><value>Full Name</value></data>
<data name="Lbl_Email" xml:space="preserve"><value>Email Address</value></data>
<data name="Btn_Submit" xml:space="preserve"><value>Submit</value></data>

<!-- Arabic -->
<data name="Lbl_Name" xml:space="preserve"><value>الاسم الكامل</value></data>
<data name="Lbl_Email" xml:space="preserve"><value>عنوان البريد الإلكتروني</value></data>
<data name="Btn_Submit" xml:space="preserve"><value>إرسال</value></data>
```

### Task: Localize Error Messages

```csharp
public class ProductValidator
{
    private readonly IStringLocalizer<SharedResource> _localizer;

    public ProductValidator(IStringLocalizer<SharedResource> localizer)
    {
        _localizer = localizer;
    }

    public string ValidatePrice(decimal price)
    {
        if (price <= 0)
            return _localizer["Val_PricePositive"];
        return string.Empty;
    }
}
```

### Task: Format Currency by Language

```csharp
// In controller
var culture = System.Globalization.CultureInfo.CurrentCulture;
decimal price = 99.99m;

ViewBag.FormattedPrice = price.ToString("C", culture);
// English: $99.99
// Arabic: ر.س 99,99 (depends on system locale)
// French: 99,99€

// Or use CultureInfo explicitly
var arCulture = new System.Globalization.CultureInfo("ar-SA");
ViewBag.ArabicPrice = price.ToString("C", arCulture);
```

### Task: Show RTL-Specific Styles

```html
@{
    var isRtl = System.Globalization.CultureInfo.CurrentUICulture.Name.StartsWith("ar");
}

<div style="text-align: @(isRtl ? "right" : "left"); direction: @(isRtl ? "rtl" : "ltr");">
    @Localizer["SomeText"]
</div>
```

Or use CSS classes:
```css
html[dir="rtl"] .my-element {
    text-align: right;
    margin-left: 0;
    margin-right: 20px;
}
```

### Task: Switch Language Programmatically

```html
<form method="post" action="/Culture/SetCulture">
    <select name="culture" onchange="this.form.submit();">
        <option value="en" @(ViewBag.CurrentCulture == "en" ? "selected" : "")>English</option>
        <option value="ar" @(ViewBag.CurrentCulture == "ar" ? "selected" : "")>العربية</option>
        <option value="fr" @(ViewBag.CurrentCulture == "fr" ? "selected" : "")>Français</option>
        <option value="de" @(ViewBag.CurrentCulture == "de" ? "selected" : "")>Deutsch</option>
    </select>
</form>
```

### Task: Use Parametrized Strings

```csharp
// In code
int itemCount = 5;
string message = _localizer["Items_Count", itemCount];

// Resource
// <data name="Items_Count"><value>{0} items in cart</value></data>
// Arabic: <value>{0} عنصر في السلة</value>
// Result: "5 items in cart" or "5 عنصر في السلة"
```

## 🎯 Quick Reference

| Task | Code |
|------|------|
| Inject localizer | `IStringLocalizer<MyController> localizer` |
| Get string | `_localizer["KeyName"]` |
| With parameter | `_localizer["KeyName", param1, param2]` |
| Current culture | `CultureInfo.CurrentCulture.Name` |
| Format currency | `value.ToString("C")` |
| Format date | `date.ToString("d")` |
| Check if RTL | `CultureInfo.CurrentCulture.Name.StartsWith("ar")` |
| Set culture cookie | `Response.SetCultureCookie("ar")` |

## 📂 File Structure Reference

```
Resources/
├── Shared/
│   ├── SharedResource.cs
│   ├── SharedResource.resx
│   ├── SharedResource.ar.resx
│   ├── SharedResource.fr.resx
│   └── SharedResource.de.resx
└── Controllers/
    ├── HomeController.resx
    ├── HomeController.ar.resx
    ├── HomeController.fr.resx
    └── HomeController.de.resx
```

## 🆎 Naming Conventions

| Prefix | Example | Usage |
|--------|---------|-------|
| `Btn_` | `Btn_Submit` | Buttons |
| `Lbl_` | `Lbl_Name` | Labels |
| `Msg_` | `Msg_Success` | Messages |
| `Placeholder_` | `Placeholder_Email` | Input placeholders |
| `Val_` | `Val_Required` | Validation errors |
| `Title_` | `Title_HomePage` | Page titles |
| `Err_` | `Err_NotFound` | Error messages |
| `Menu_` | `Menu_Products` | Navigation items |

## ✅ Testing Checklist

- [ ] English version displays correctly
- [ ] Arabic version displays correctly and is RTL
- [ ] French and German display correctly
- [ ] All button labels are translated
- [ ] All form labels are translated
- [ ] Error messages are translated
- [ ] Page titles are translated
- [ ] Dates format correctly for each language
- [ ] Currency formats correctly for each language
- [ ] Language switcher works
- [ ] Culture cookie persists across page reloads
- [ ] Accept-Language header works as fallback

## 🔗 Where Resources Are Used

```
Program.cs
    ↓
Program.cs extension (ConfigureLocalization)
    ↓
CultureProvider (Query string, cookie, Accept-Language)
    ↓
IStringLocalizer (Controllers)
    ↓
IViewLocalizer (Razor views)
    ↓
CultureController.SetCulture (Language switcher)
```

## 💡 Pro Tips

1. **Always start with English** - Create `.resx` (no culture code) first
2. **Consistent naming** - Helps with searching and maintenance
3. **Test RTL layout** - Arabic needs special CSS attention
4. **Check text length** - Translations differ in length
5. **Use resource preview** - Open `.resx` in Resource Editor to see all keys
6. **Culture first** - Users see culture preference from cookie, then Accept-Language

## 🚨 Common Issues & Fixes

| Issue | Cause | Fix |
|-------|-------|-----|
| `[ControllerName.KeyName]` appears | Resources not compiled | Rebuild solution |
| Arabic shows left-to-right | Missing `dir="rtl"` | Add to HTML or CSS |
| Culture not persisting | Cookie not set | Check `SetCultureCookie` call |
| Wrong culture displays | Accept-Language override | Check query string first |

## 📚 Need More Help?

1. **Examples**: Check `LocalizationExampleController.cs` and views
2. **Full Guide**: Read `LOCALIZATION_GUIDE.md`
3. **Adding Translations**: Follow `ADDING_TRANSLATIONS.md`
4. **Docs**: Visit `/localization-example`

---

**Ready to add more languages?** Follow the 5-minute example template for each new feature!
