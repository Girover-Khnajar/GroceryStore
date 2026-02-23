// ╔══════════════════════════════════════════════════════════════════════════╗
// ║                    FRESH GROCERY STORE — BLAZOR SERVER                  ║
// ║                           Program.cs                                    ║
// ╠══════════════════════════════════════════════════════════════════════════╣
// ║  HOW TO SWITCH BETWEEN MOCK AND REAL API SERVICES                       ║
// ║  ─────────────────────────────────────────────────────────────────────  ║
// ║  STEP 1 ► Open this file.                                               ║
// ║  STEP 2 ► Find the section labelled "SERVICE REGISTRATION" below.       ║
// ║  STEP 3 ► Comment out the MOCK block and uncomment the REAL block.      ║
// ║                                                                          ║
// ║  That is the ONLY change needed — all Razor components and pages        ║
// ║  depend on the interfaces, not the implementations.                     ║
// ╚══════════════════════════════════════════════════════════════════════════╝

using GroceryStore.App.Services.Interfaces;
using GroceryStore.App.Services.Mock;

// using GroceryStore.Services.Http;  // ← Uncomment when switching to real API

var builder = WebApplication.CreateBuilder(args);

// ── Blazor Server ─────────────────────────────────────────────────────────────
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

// ════════════════════════════════════════════════════════════════════════════
//  SERVICE REGISTRATION
// ════════════════════════════════════════════════════════════════════════════

// ┌─────────────────────────────────────────────────────────────────────────┐
// │  ✅ MOCK  (no backend needed — hardcoded in-memory data)                │
// │  ── Active right now ──────────────────────────────────────────────────  │
// └─────────────────────────────────────────────────────────────────────────┘
builder.Services.AddSingleton<IProductService,   MockProductService>();
builder.Services.AddSingleton<ICategoryService,  MockCategoryService>();
builder.Services.AddSingleton<IBrandService,     MockBrandService>();
builder.Services.AddSingleton<IBannerService,    MockBannerService>();
builder.Services.AddSingleton<ISettingsService,  MockSettingsService>();
builder.Services.AddSingleton<IDashboardService, MockDashboardService>();
builder.Services.AddScoped   <IAuthService,      MockAuthService>();

// ┌─────────────────────────────────────────────────────────────────────────┐
// │  🔌 REAL API  (comment the MOCK block above, uncomment this block)      │
// │                                                                          │
// │  Also:                                                                   │
// │   1. Add NuGet: Microsoft.AspNetCore.Components.Authorization            │
// │   2. Set "ApiSettings:BaseUrl" in appsettings.json                       │
// │   3. In AdminLayout.razor, uncomment RestoreSessionAsync() call          │
// └─────────────────────────────────────────────────────────────────────────┘
/*
builder.Services.AddDataProtection();   // required for ProtectedSessionStorage

var apiBaseUrl = builder.Configuration["ApiSettings:BaseUrl"]
                 ?? throw new InvalidOperationException(
                        "ApiSettings:BaseUrl is not configured in appsettings.json");

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
*/

// ── Middleware Pipeline ───────────────────────────────────────────────────────
var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
