# GroceryStore.Web — MVC Migration Guide

## Table of Contents

1. [Architecture Overview](#1-architecture-overview)
2. [Project & Reference Structure](#2-project--reference-structure)
3. [Migration Phases](#3-migration-phases)
4. [Blazor → MVC Translation Patterns](#4-blazor--mvc-translation-patterns)
5. [DI, EF Core, Configuration & Auth](#5-di-ef-core-configuration--auth)
6. [API vs Direct Usage](#6-api-vs-direct-usage)
7. [Code Walkthrough](#7-code-walkthrough)
8. [Risks & Best Practices](#8-risks--best-practices)

---

## 1. Architecture Overview

```
┌─────────────────────────────────────────────────────────┐
│                    Presentation Layer                    │
│                                                         │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐  │
│  │ GroceryStore │  │ GroceryStore │  │ GroceryStore │  │
│  │     .Api     │  │    .App      │  │    .Web      │  │
│  │ (Minimal API)│  │  (Blazor)    │  │   (MVC) ★    │  │
│  └──────┬───────┘  └──────────────┘  └──────┬───────┘  │
│         │                                    │          │
└─────────┼────────────────────────────────────┼──────────┘
          │    ┌─────────────────────────┐     │
          ├───►│   GroceryStore          │◄────┤
          │    │   .Application          │     │
          │    │ (CQRS commands/queries) │     │
          │    └────────────┬────────────┘     │
          │                 │                  │
          │    ┌────────────▼────────────┐     │
          │    │   GroceryStore          │     │
          │    │   .Domain               │     │
          │    │ (Entities, VOs, Repos)  │     │
          │    └─────────────────────────┘     │
          │                                    │
          │    ┌─────────────────────────┐     │
          └───►│   GroceryStore          │◄────┘
               │   .Infrastructure       │
               │ (EF Core, SQL Server)   │
               └─────────────────────────┘
```

**Key principle:** `GroceryStore.Web` uses `IMessageDispatcher` directly — the same CQRS dispatcher that the API endpoints use. No HTTP round-trips, no business logic duplication.

---

## 2. Project & Reference Structure

### Dependencies

```
GroceryStore.Web
  └─► GroceryStore.Infrastructure  (EF Core + repo implementations)
        └─► GroceryStore.Application  (CQRS handlers, DTOs, commands, queries)
              └─► GroceryStore.Domain  (Entities, Value Objects, Interfaces)
  └─► CQRS  (custom mediator library)
```

### .csproj

```xml
<Project Sdk="Microsoft.NET.Sdk.Web">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
  </PropertyGroup>
  <ItemGroup>
    <ProjectReference Include="..\..\backend\GroceryStore.Infrastructure\..." />
    <ProjectReference Include="..\..\libs\CQRS\src\CQRS.csproj" />
  </ItemGroup>
</Project>
```

This mirrors the API's `.csproj` — both reference `Infrastructure` + `CQRS`.

### File Layout

```
src/frontend/GroceryStore.Web/
├── Program.cs                  # DI, middleware, routing
├── appsettings.json            # Connection string (same DB)
├── Controllers/
│   ├── HomeController.cs       # Landing page, featured products
│   ├── CategoriesController.cs # Browse by category
│   ├── ProductsController.cs   # Product listing & detail
│   └── AdminController.cs      # Full CRUD admin
├── ViewModels/
│   ├── HomeViewModel.cs
│   ├── CategoryDetailViewModel.cs
│   ├── ProductListViewModel.cs
│   ├── ProductDetailViewModel.cs
│   └── Admin/
│       ├── AdminDashboardViewModel.cs
│       ├── CategoryFormViewModel.cs
│       └── ProductFormViewModel.cs
├── Extensions/
│   └── ResultExtensions.cs     # CQRS Result → MVC helpers
├── Views/
│   ├── _ViewImports.cshtml
│   ├── _ViewStart.cshtml
│   ├── Shared/
│   │   ├── _Layout.cshtml      # Public layout (navbar + footer)
│   │   ├── _AdminLayout.cshtml # Admin sidebar layout
│   │   ├── _Notification.cshtml
│   │   └── Error.cshtml
│   ├── Home/Index.cshtml
│   ├── Categories/{Index,Details}.cshtml
│   ├── Products/{Index,Details}.cshtml
│   └── Admin/{Dashboard,Categories,CategoryForm,Products,ProductForm}.cshtml
└── wwwroot/
    ├── css/site.css
    └── js/site.js
```

---

## 3. Migration Phases

### Phase 1 — Foundation (✅ Done)

- [x] Create `GroceryStore.Web.csproj` with correct references
- [x] Wire `Program.cs` with `AddInfrastructure()` + `AddApplication()`
- [x] Add to solution (`GroceryStore.slnx`)
- [x] Create public-facing controllers: `Home`, `Categories`, `Products`
- [x] Create admin controller with full CRUD
- [x] Create CSS/JS assets and Razor views
- [x] Verify full solution builds (0 errors, 0 warnings)

### Phase 2 — Feature Parity

- [ ] Add image upload handling (port `IImageUploadService` from API)
- [ ] Add remaining admin pages: Banners, Brands, Gallery, Settings
- [ ] Add search overlay / global search
- [ ] Port responsive mobile navigation from Blazor `MainLayout`
- [ ] Add client-side validation scripts (`jquery.validate`)

### Phase 3 — Authentication

- [ ] Add ASP.NET Core Identity or cookie-based authentication
- [ ] Protect `/Admin/*` routes with `[Authorize]`
- [ ] Add Login/Logout views
- [ ] Replace Blazor's `MockAuthService` pattern with Identity middleware

### Phase 4 — Polish & Cutover

- [ ] Copy & adapt CSS from `GroceryStore.App/wwwroot` for visual parity
- [ ] Set up production configuration (HTTPS, HSTS, caching)
- [ ] Add health checks and logging
- [ ] Update deployment scripts
- [ ] Run side-by-side with Blazor app, then decommission when ready

---

## 4. Blazor → MVC Translation Patterns

### Page → Controller + View

| Blazor Pattern | MVC Equivalent |
|---|---|
| `@page "/categories"` | `[HttpGet("")] on CategoriesController` |
| `@inject ICategoryService` | Constructor-inject `IMessageDispatcher` |
| `@code { OnInitializedAsync() }` | `async Task<IActionResult> Index()` |
| `@foreach (var cat in _categories)` | `@foreach (var cat in Model)` in `.cshtml` |
| `<NavLink href="...">` | `<a asp-controller="..." asp-action="...">` |
| `<EditForm OnValidSubmit="Save">` | `<form asp-action="..." method="post">` + `[ValidateAntiForgeryToken]` |
| `StateHasChanged()` | Redirect-after-POST (PRG pattern) |
| `TempData` equivalent: none | `TempData["Success"]` / `TempData["Error"]` |

### Component → Partial View or View Component

| Blazor | MVC |
|---|---|
| `<CategoryCard Category="cat" />` | `@await Html.PartialAsync("_CategoryCard", cat)` or inline in view |
| `<Toast @ref="_toast">` | `@await Html.PartialAsync("_Notification")` with TempData |
| `<ConfirmDialog>` | `onclick="return confirm('...')"` or JS modal |
| `@inherits LayoutComponentBase` | `Layout = "_Layout"` in `_ViewStart.cshtml` |

### Data Flow

```
Blazor:   @inject ICategoryService → service.GetCategoriesAsync() → List<Category>
MVC:      inject IMessageDispatcher → dispatcher.QueryAsync<...>() → Result<IReadOnlyList<CategoryDto>>
```

The MVC path is actually **more direct** — it uses the Application DTOs directly instead of mapping through a second model layer.

---

## 5. DI, EF Core, Configuration & Auth

### Dependency Injection

`Program.cs` calls the **same** extension methods as the API:

```csharp
builder.Services.AddInfrastructure(connectionString);  // EF Core + repos
builder.Services.AddApplication();                      // CQRS handlers
```

This registers: `AppDbContext`, `IUnitOfWork`, `ICategoryRepository`, `IProductRepository`, `IImageAssetRepository`, `IProductsReadStore`, and all CQRS command/query handlers.

`IMessageDispatcher` is registered by `AddCqrs()` inside `AddApplication()`.

### EF Core

Both the API and MVC share the **same** `AppDbContext`, same connection string, same SQL Server database. No duplication.

⚠ If both run simultaneously pointing at the same DB, ensure you handle concurrency appropriately (optimistic concurrency is already handled at the domain level).

### Configuration

- `appsettings.json` contains the connection string
- Same structure as the API's config
- Environment-specific overrides in `appsettings.Development.json`

### Auth (Phase 3)

The Blazor app uses `MockAuthService`. For MVC, the recommended approach is:

```csharp
// Program.cs — add before app.UseAuthorization()
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Admin/Login";
        options.AccessDeniedPath = "/Admin/AccessDenied";
    });

// Protect admin routes:
[Authorize]
[Route("Admin")]
public class AdminController : Controller { ... }
```

### Routing

```csharp
// Admin routes
app.MapControllerRoute("admin", "Admin/{action=Dashboard}/{id?}",
    defaults: new { controller = "Admin" });

// Default public routes
app.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");
```

Plus attribute routing on controllers for slug-based URLs:
- `/Categories/{slug}` — category details
- `/Products/{slug}` — product details

---

## 6. API vs Direct Usage

### What stays in the API

| Concern | Reason |
|---|---|
| Mobile/SPA clients | API serves JSON to any client |
| Third-party integrations | Webhooks, partner APIs |
| OpenAPI/Scalar docs | API documentation |
| Rate limiting, API keys | API-specific middleware |
| Image upload endpoint | Can be shared or duplicated |

### What the MVC project uses directly

| Concern | Reason |
|---|---|
| Category CRUD | Commands/queries via `IMessageDispatcher` |
| Product CRUD | Commands/queries via `IMessageDispatcher` |
| Image management | Commands/queries via `IMessageDispatcher` |
| Dashboard stats | Queries via `IMessageDispatcher` |
| Search & filtering | `GetProductsQuery` with filters |

**Rule of thumb:** The MVC app never calls the API over HTTP. It uses the Application layer in-process. The API continues to exist for non-browser clients.

---

## 7. Code Walkthrough

### Program.cs (complete)

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("...");

builder.Services.AddInfrastructure(connectionString);
builder.Services.AddApplication();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute("admin", "Admin/{action=Dashboard}/{id?}",
    defaults: new { controller = "Admin" });
app.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");

app.Run();
```

### Controller Pattern (Category Create example)

```csharp
[HttpPost("Categories/Create")]
[ValidateAntiForgeryToken]
public async Task<IActionResult> CategoryCreate(CategoryFormViewModel vm, CancellationToken ct)
{
    if (!ModelState.IsValid)
    {
        vm.AvailableParents = await GetAllCategories(ct);
        return View("CategoryForm", vm);
    }

    var command = new CreateCategoryCommand(
        vm.Name, vm.Slug, vm.SortOrder, vm.ParentCategoryId,
        vm.Description, vm.ImageUrl, vm.IconName,
        vm.SeoMetaTitle, vm.SeoMetaDescription);

    var result = await _dispatcher.SendAsync<CreateCategoryCommand, Guid>(command, ct);

    if (!result.Succeeded(this))  // sets TempData["Error"] on failure
    {
        vm.AvailableParents = await GetAllCategories(ct);
        return View("CategoryForm", vm);
    }

    TempData["Success"] = "Category created successfully.";
    return RedirectToAction(nameof(Categories));  // PRG pattern
}
```

### ResultExtensions (CQRS → MVC bridge)

```csharp
public static bool Succeeded(this Result result, Controller controller)
{
    if (result.IsSuccess) return true;
    controller.TempData["Error"] = result.Errors.FirstOrDefault()?.Message ?? "An error occurred.";
    return false;
}

public static T? ValueOrDefault<T>(this Result<T> result, Controller controller)
{
    if (result.IsSuccess) return result.Value;
    controller.TempData["Error"] = result.Errors.FirstOrDefault()?.Message ?? "An error occurred.";
    return default;
}
```

### View (CategoryForm — tag helpers + model binding)

```html
<form asp-action="CategoryCreate" method="post">
    @Html.AntiForgeryToken()
    <div class="form-group">
        <label asp-for="Name" class="form-label"></label>
        <input asp-for="Name" class="form-control" />
        <span asp-validation-for="Name" class="text-danger"></span>
    </div>
    <!-- ... more fields ... -->
    <button type="submit" class="btn btn-primary">Save</button>
</form>
```

---

## 8. Risks & Best Practices

### Risks

| Risk | Mitigation |
|---|---|
| **Shared DB concurrency** | Both API and MVC point at the same DB. Use optimistic concurrency (already in domain). Avoid running migrations from both. |
| **Schema drift** | Only one project should own EF migrations (keep it in Infrastructure). |
| **Feature divergence** | Track which features exist in Blazor vs MVC. Use a feature matrix. |
| **Auth differences** | API uses token-based auth; MVC should use cookie-based. Keep auth concerns in the presentation layer, not in Application. |
| **Duplicate view models** | MVC ViewModels are intentional — they're tailored for form binding & display. Don't reuse Application DTOs as form models. |

### Best Practices

1. **Never duplicate business logic** — always go through `IMessageDispatcher` and the existing commands/queries.
2. **PRG pattern** — always redirect after POST to prevent double-submit.
3. **TempData for flash messages** — use `_Notification.cshtml` partial consistently.
4. **ViewModels ≠ DTOs** — ViewModels add `[Required]`, `[MaxLength]`, dropdown lists. DTOs are read-only data.
5. **One migration source** — EF migrations stay in `GroceryStore.Infrastructure`.
6. **Incremental migration** — run Blazor and MVC side-by-side. Migrate page by page.
7. **Test via existing tests** — the Application and Domain test projects already cover business logic. Add controller-level integration tests in a new `GroceryStore.Web.Tests` project.
8. **Share static assets** — if both frontends need the same images/CSS, serve from a CDN or a shared wwwroot.
