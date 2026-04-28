# Adding Translations to GroceryStore

This guide explains how to add new translation resources to the GroceryStore application.

## Quick Summary

1. Identify where the string will be used (controller, view, or shared)
2. Create `.resx` files in the appropriate `Resources/` folder structure
3. Add key-value pairs in each `.resx` file for supported languages
4. Reference the resource key in your code
5. Rebuild the solution

## Step-by-Step Instructions

### 1. Identify Resource Location

Choose the appropriate folder based on usage:

| Location | Use Case | Path |
|----------|----------|------|
| **Shared** | Used across multiple controllers/views | `Resources/Shared/` |
| **Controller** | Specific to a single controller | `Resources/Controllers/{ControllerName}/` |
| **View** | Specific to a single view | `Resources/Views/` |

### 2. Create Resource Files

#### For Shared Resources

Create these files in `Resources/Shared/`:

- `SharedResource.resx` (English - default)
- `SharedResource.ar.resx` (Arabic)
- `SharedResource.fr.resx` (French)
- `SharedResource.de.resx` (German)

#### For Controller-Specific Resources

Create these files in `Resources/Controllers/`:

- `{ControllerName}.resx` (English)
- `{ControllerName}.ar.resx` (Arabic)
- `{ControllerName}.fr.resx` (French)
- `{ControllerName}.de.resx` (German)

**Example for ProductController:**
```
Resources/Controllers/
├── ProductController.resx
├── ProductController.ar.resx
├── ProductController.fr.resx
└── ProductController.de.resx
```

#### For View-Specific Resources

Create these files in `Resources/Views/`:

- `{Controller}_{View}.resx` (English)
- `{Controller}_{View}.ar.resx` (Arabic)
- `{Controller}_{View}.fr.resx` (French)
- `{Controller}_{View}.de.resx` (German)

**Example for Products/Index view:**
```
Resources/Views/
├── Products_Index.resx
├── Products_Index.ar.resx
├── Products_Index.fr.resx
└── Products_Index.de.resx
```

### 3. Add Translation Key-Value Pairs

#### Using Visual Studio's RESX Editor:

1. Right-click the `.resx` file → **Open With** → **Resource Editor**
2. Click **Add Resource** button
3. Enter **Name** (the translation key)
4. Enter **Value** (the English text)
5. If there's a **Comment**, add context (optional)
6. Repeat for each key
7. Save the file

#### Editing XML Directly:

Open the `.resx` file as XML and add:

```xml
<data name="Welcome" xml:space="preserve">
    <value>Welcome to GroceryStore</value>
    <comment>Shown on homepage</comment>
</data>

<data name="Btn_AddToCart" xml:space="preserve">
    <value>Add to Cart</value>
    <comment>Button text</comment>
</data>

<data name="Price_Template" xml:space="preserve">
    <value>Price: {0}</value>
    <comment>Shows product price with placeholder</comment>
</data>
```

### 4. Naming Convention for Keys

Use prefix notation for better organization:

| Prefix | Use | Example |
|--------|-----|---------|
| `Btn_` | Button text | `Btn_Submit`, `Btn_Cancel`, `Btn_Delete` |
| `Lbl_` | Label text | `Lbl_Name`, `Lbl_Email`, `Lbl_Password` |
| `Msg_` | Messages | `Msg_Success`, `Msg_Error`, `Msg_Warning` |
| `Placeholder_` | Input placeholders | `Placeholder_Name`, `Placeholder_Email` |
| `Val_` | Validation messages | `Val_Required`, `Val_InvalidEmail` |
| `Title_` | Page titles | `Title_Products`, `Title_Cart` |
| `Err_` | Error messages | `Err_NotFound`, `Err_Unauthorized` |
| `Info_` | Information text | `Info_Loading`, `Info_NoResults` |
| `Menu_` | Navigation items | `Menu_Home`, `Menu_Products` |

### 5. Complete Example: Adding a New Product Page

#### Step 1: Create Resource Files

```
Resources/Controllers/
├── ProductController.resx (NEW)
├── ProductController.ar.resx (NEW)
├── ProductController.fr.resx (NEW)
└── ProductController.de.resx (NEW)
```

#### Step 2: Add Keys to English File (ProductController.resx)

| Name | Value | Comment |
|------|-------|---------|
| `Title_Index` | `Products` | Page title |
| `Title_Details` | `Product Details` | Detail page title |
| `Btn_AddToCart` | `Add to Cart` | Add to cart button |
| `Btn_BuyNow` | `Buy Now` | Quick purchase button |
| `Btn_ViewDetails` | `View Details` | View product details |
| `Price_OutOfStock` | `Out of Stock` | Stock status |
| `Msg_AddedToCart` | `Product added to cart` | Success message |
| `Err_ProductNotFound` | `Product not found` | Error message |

#### Step 3: Add Same Keys to Arabic File (ProductController.ar.resx)

| Name | Value | Comment |
|------|-------|---------|
| `Title_Index` | `المنتجات` | Page title |
| `Title_Details` | `تفاصيل المنتج` | Detail page title |
| `Btn_AddToCart` | `إضافة إلى السلة` | Add to cart button |
| `Btn_BuyNow` | `شراء الآن` | Quick purchase button |
| `Btn_ViewDetails` | `عرض التفاصيل` | View product details |
| `Price_OutOfStock` | `غير متوفر` | Stock status |
| `Msg_AddedToCart` | `تم إضافة المنتج إلى السلة` | Success message |
| `Err_ProductNotFound` | `المنتج غير موجود` | Error message |

#### Step 4: Add Same Keys to French and German Files

Follow the same pattern for `ProductController.fr.resx` and `ProductController.de.resx`.

#### Step 5: Use in Controller

```csharp
public class ProductController : Controller
{
    private readonly IStringLocalizer<ProductController> _localizer;

    public ProductController(IStringLocalizer<ProductController> localizer)
    {
        _localizer = localizer;
    }

    public IActionResult Index()
    {
        ViewBag.Title = _localizer["Title_Index"];
        return View();
    }

    public IActionResult Details(int id)
    {
        ViewBag.Title = _localizer["Title_Details"];
        return View();
    }

    [HttpPost]
    public IActionResult AddToCart(int productId)
    {
        // ...
        return Ok(new { message = _localizer["Msg_AddedToCart"] });
    }
}
```

#### Step 6: Use in View

```html
@using Microsoft.AspNetCore.Mvc.Localization
@inject IViewLocalizer Localizer
@inject IStringLocalizer<ProductController> ProductLocalizer

<h1>@Localizer["Title"]</h1>
<button class="btn btn-primary">@ProductLocalizer["Btn_AddToCart"]</button>
```

### 6. Language-Specific Considerations

#### Arabic (ar)
- **Direction**: RTL (Right-to-Left)
- **Text Alignment**: Right-aligned
- **Key Considerations**:
  - Dates use Hijri calendar (optional)
  - Currency might use Arabic numerals
  - Numbers read right-to-left
  - Punctuation may differ

#### French (fr)
- **Direction**: LTR (Left-to-Right)
- **Text Alignment**: Left-aligned
- **Key Considerations**:
  - Space before certain punctuation: `« text »`
  - Date format: `dd/mm/yyyy`
  - Uses Oxford comma inconsistently

#### German (de)
- **Direction**: LTR (Left-to-Right)
- **Text Alignment**: Left-aligned
- **Key Considerations**:
  - Compound words are longer
  - Capitalization rules differ
  - Date format: `dd.mm.yyyy`
  - Numbers use comma as decimal separator

### 7. Handling Pluralization

For strings that vary by count, create multiple keys:

```csharp
// In controller
int count = GetItemCount();
string key = count switch
{
    0 => "Item_Zero",    // No items
    1 => "Item_One",     // One item
    _ => "Item_Many"     // Multiple items
};

string message = _localizer[key, count];
```

Resource entries:
```xml
<data name="Item_Zero" xml:space="preserve">
    <value>No items found</value>
</data>
<data name="Item_One" xml:space="preserve">
    <value>1 item found</value>
</data>
<data name="Item_Many" xml:space="preserve">
    <value>{0} items found</value>
</data>
```

### 8. Using Parametrized Strings

For strings with replaceable values:

```csharp
// In code
string message = _localizer["Greeting_User", userName];
decimal price = 99.99m;
string priceMsg = _localizer["Price_Display", price.ToString("C")];
```

Resource entries:
```xml
<data name="Greeting_User" xml:space="preserve">
    <value>Welcome, {0}!</value>
</data>
<data name="Price_Display" xml:space="preserve">
    <value>Price: {0}</value>
</data>
```

### 9. Validation Message Localization

For data annotation validation:

```csharp
public class ProductCreateModel
{
    [Required(ErrorMessageResourceType = typeof(SharedResource), 
              ErrorMessageResourceName = "Val_ProductNameRequired")]
    public string Name { get; set; }

    [Range(0.01, decimal.MaxValue, 
           ErrorMessageResourceType = typeof(SharedResource),
           ErrorMessageResourceName = "Val_PriceRange")]
    public decimal Price { get; set; }
}
```

Resource entries in `SharedResource.resx`:
```xml
<data name="Val_ProductNameRequired" xml:space="preserve">
    <value>Product name is required</value>
</data>
<data name="Val_PriceRange" xml:space="preserve">
    <value>Price must be greater than 0</value>
</data>
```

And corresponding Arabic entries in `SharedResource.ar.resx`:
```xml
<data name="Val_ProductNameRequired" xml:space="preserve">
    <value>اسم المنتج مطلوب</value>
</data>
<data name="Val_PriceRange" xml:space="preserve">
    <value>يجب أن يكون السعر أكبر من 0</value>
</data>
```

### 10. Common Mistakes to Avoid

❌ **DON'T:**
- Have missing keys in Arabic/French/German files
- Use uppercase culture codes ("AR" instead of "ar")
- Forget to add the default English resource
- Create multiple resource files for same key
- Use spaces in key names (use underscore instead)
- Hardcode strings in views/controllers

✅ **DO:**
- Organize keys with prefixes (Btn_, Lbl_, Msg_)
- Always provide English default
- Rebuild after adding resources
- Use consistent naming across files
- Add comments explaining context
- Test each language to ensure proper display

### 11. Testing Your Translations

#### Via Query String:
```
/Product/Index?culture=ar
/Product/Index?culture=fr
```

#### Via Language Switcher:
Use the built-in language switcher to test each language.

#### Via Browser:
Change browser language settings and test Accept-Language header.

### 12. Troubleshooting

**Problem:** Key shows as `[ControllerName.KeyName]`
- **Solution:** Rebuild the project. Resource files weren't compiled.

**Problem:** Arabic text doesn't display correctly
- **Solution:** Verify font support and ensure `dir="rtl"` is set on parent elements.

**Problem:** Culture cookie isn't being set
- **Solution:** Check that middleware is registered and cookie middleware is configured.

**Problem:** Wrong culture displaying
- **Solution:** Clear browser cookies and try again. Check cookie name matches `GroceryStore.Culture`.

### 13. Exporting and Collaborating

#### For Translation Teams:
Export resource keys to CSV for translators:

1. Extract all `.resx` files
2. Use `ResXResourceReader` to read keys
3. Create CSV with columns: Key | English | Arabic | French | German
4. Send to translators for completion
5. Import back into `.resx` files

Example CSV format:
```
Key,English,Arabic,French,German
Btn_Submit,Submit,إرسال,Envoyer,Absenden
Btn_Cancel,Cancel,إلغاء,Annuler,Abbrechen
Msg_Success,Operation successful,تمت العملية بنجاح,Opération réussie,Vorgang erfolgreich
```

### 14. Automated Translation (Not Recommended for Production)

For development/testing only:
```csharp
// Use Azure Translator API or similar
var translator = new TranslatorService(apiKey);
var arabicText = await translator.TranslateAsync(englishText, "en", "ar");
```

**Note:** Always use human translators for production applications.

## References

- [Adding Resources to Resource Files](https://docs.microsoft.com/en-us/dotnet/framework/resources/creating-satellite-assemblies-for-desktop-apps)
- [RESX Editor in Visual Studio](https://docs.microsoft.com/en-us/visualstudio/ide/create-satellite-assemblies-for-desktop-apps)
- [CultureInfo Class Reference](https://docs.microsoft.com/en-us/dotnet/api/system.globalization.cultureinfo)
- [Right-to-Left Language Support](https://developer.mozilla.org/en-US/docs/Glossary/RTL)
