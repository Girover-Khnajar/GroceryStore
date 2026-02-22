# Fresh Grocery Store — Blazor Server

A fully functional grocery store frontend built with **Blazor Server (.NET 8)**.  
Clean Architecture · IHttpClientFactory · Mock / Real service switching.

---

## Quick Start

```bash
cd GroceryStore
dotnet run
```

Navigate to `https://localhost:5001`  
Admin panel: `https://localhost:5001/admin/login`  
Admin credentials: **admin / admin123**

---

## Mock vs Real API — How to Switch

Open **`Program.cs`** — it is the **only file you need to change**.

### ✅ Currently Active: MOCK (no backend required)

```csharp
builder.Services.AddSingleton<IProductService,   MockProductService>();
builder.Services.AddSingleton<ICategoryService,  MockCategoryService>();
builder.Services.AddSingleton<IBrandService,     MockBrandService>();
builder.Services.AddSingleton<IBannerService,    MockBannerService>();
builder.Services.AddSingleton<ISettingsService,  MockSettingsService>();
builder.Services.AddSingleton<IDashboardService, MockDashboardService>();
builder.Services.AddScoped   <IAuthService,      MockAuthService>();
```

All data lives in `Services/Mock/MockServices.cs → MockDb`.  
CRUD operations mutate the in-memory lists (data resets on app restart).

---

### 🔌 Switch to Real API

**Step 1** — In `Program.cs`, comment out the MOCK block and uncomment the REAL block:

```csharp
// Comment out:
// builder.Services.AddSingleton<IProductService, MockProductService>();
// ... (all Mock lines)

// Uncomment:
builder.Services.AddDataProtection();

var apiBaseUrl = builder.Configuration["ApiSettings:BaseUrl"]
                 ?? throw new InvalidOperationException("ApiSettings:BaseUrl not set");

builder.Services.AddHttpClient("ApiClient", client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
    client.Timeout     = TimeSpan.FromSeconds(30);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});

builder.Services.AddScoped<IProductService,   HttpProductService>();
builder.Services.AddScoped<ICategoryService,  HttpCategoryService>();
builder.Services.AddScoped<IBrandService,     HttpBrandService>();
builder.Services.AddScoped<IBannerService,    HttpBannerService>();
builder.Services.AddScoped<ISettingsService,  HttpSettingsService>();
builder.Services.AddScoped<IDashboardService, HttpDashboardService>();
builder.Services.AddScoped<IAuthService,      HttpAuthService>();
```

**Step 2** — Set your API base URL in `appsettings.json`:

```json
"ApiSettings": {
    "BaseUrl": "https://api.yourdomain.com/"
}
```

**Step 3** — In `Components/Layout/AdminLayout.razor`, uncomment the session restore call:

```csharp
// Uncomment this block in OnAfterRenderAsync:
if (AuthService is GroceryStore.Services.Http.HttpAuthService concrete)
    await concrete.RestoreSessionAsync();
```

**Step 4** — Run:

```bash
dotnet run
```

---

## Expected API Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/products` | List products (query params: page, pageSize, sortBy, categoryId, brandId, search, isFeatured, isActive) |
| GET | `/api/products/{id}` | Get by ID |
| GET | `/api/products/slug/{slug}` | Get by slug |
| POST | `/api/products` | Create |
| PUT | `/api/products/{id}` | Update |
| DELETE | `/api/products/{id}` | Delete |
| GET | `/api/categories` | List categories |
| GET | `/api/categories/slug/{slug}` | Get by slug |
| POST/PUT/DELETE | `/api/categories/{id}` | CRUD |
| GET | `/api/brands` | List brands |
| POST/PUT/DELETE | `/api/brands/{id}` | CRUD |
| GET | `/api/banners` | List banners |
| POST/PUT/DELETE | `/api/banners/{id}` | CRUD |
| GET | `/api/settings` | Get store settings |
| PUT | `/api/settings` | Save store settings |
| POST | `/api/auth/login` | Login → returns `{ "token": "..." }` |
| GET | `/api/dashboard/stats` | Returns `DashboardStats` object |

---

## Project Structure

```
GroceryStore/
├── Models/
│   ├── Product.cs
│   └── Models.cs              (Category, Brand, Banner, StoreSettings, etc.)
├── Services/
│   ├── Interfaces/
│   │   └── IServices.cs       (All 7 service interfaces)
│   ├── Mock/
│   │   └── MockServices.cs    (Hardcoded data + in-memory CRUD)
│   └── Http/
│       └── HttpServices.cs    (Real API implementations via IHttpClientFactory)
├── Components/
│   ├── Layout/
│   │   ├── MainLayout.razor
│   │   └── AdminLayout.razor
│   ├── Shared/
│   │   ├── Toast.razor
│   │   ├── ProductCard.razor
│   │   ├── CategoryCard.razor
│   │   └── BannerSlider.razor
│   └── Admin/
│       └── ConfirmDialog.razor
├── Pages/
│   ├── Public/
│   │   ├── Home.razor
│   │   ├── Categories.razor
│   │   ├── CategoryProducts.razor
│   │   ├── Products.razor
│   │   ├── ProductDetail.razor
│   │   └── Contact.razor
│   └── Admin/
│       ├── Login.razor
│       ├── Dashboard.razor
│       ├── ManageProducts.razor
│       ├── ManageCategories.razor
│       ├── ManageBrands.razor
│       ├── ManageBanners.razor
│       └── AdminSettings.razor
├── wwwroot/
│   ├── css/style.css
│   ├── css/admin.css
│   └── js/ui.js
├── Program.cs                 ← ONLY file to change for Mock ↔ Real
├── appsettings.json
└── _Imports.razor
```

---

## Author
**Juan Khanjar** · jan4ma@gmail.com
