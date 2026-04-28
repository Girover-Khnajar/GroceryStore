using GroceryStore.Application;
using GroceryStore.Infrastructure;
using GroceryStore.Web.Services;
using GroceryStore.Web.Services.Localization;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

// ── MVC + Razor Views ──────────────────────────────────────────────────
builder.Services.AddControllersWithViews()
    // Localization services for IStringLocalizer and IViewLocalizer
    .AddViewLocalization()
    .AddDataAnnotationsLocalization();

// ── Session (guest shopping cart) ───────────────────────────────────────
builder.Services.AddDistributedMemoryCache();
builder.Services.AddHttpContextAccessor();
builder.Services.AddSession(options =>
{
    options.Cookie.Name = "GroceryStore.Cart";
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.IdleTimeout = TimeSpan.FromDays(7);
});

// ── Cookie Authentication ───────────────────────────────────────────────
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath        = "/Account/Login";
        options.LogoutPath       = "/Account/Logout";
        options.AccessDeniedPath = "/Account/Login";
        options.ExpireTimeSpan   = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
        options.Cookie.Name      = "GroceryStore.Admin";
        options.Cookie.HttpOnly  = true;
        options.Cookie.SameSite  = SameSiteMode.Strict;
    });

// ── Connection string (same DB as GroceryStore.Api) ────────────────────
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

// ── Reuse shared layers (no business-logic duplication) ────────────────
builder.Services.AddInfrastructure(connectionString);   // EF Core, repos, UoW
builder.Services.AddApplication();                       // CQRS handlers, validation, logging
builder.Services.AddScoped<IStoreSettingsService, StoreSettingsService>();
builder.Services.AddScoped<ICartService, SessionCartService>();

// ── Localization Services ──────────────────────────────────────────────
// Support for multilingual application with English, Arabic, French, German
builder.Services.AddLocalization(options => 
    options.ResourcesPath = "Resources");

// Configure localization options: cultures, default culture, providers
builder.Services.Configure<RequestLocalizationOptions>(options =>
    options.ConfigureLocalization());

var app = builder.Build();

// ── Pipeline ───────────────────────────────────────────────────────────
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseStatusCodePagesWithReExecute("/Error/{0}");

// ── Localization Middleware ────────────────────────────────────────────
// Must be before routing. Detects culture from query string, cookie, or Accept-Language header
var localizationOptions = app.Services.GetRequiredService<IOptions<RequestLocalizationOptions>>().Value;
app.UseRequestLocalization(localizationOptions);

app.UseRouting();
app.UseSession();

app.UseAuthentication();
app.UseAuthorization();

// ── Routes ─────────────────────────────────────────────────────────────
app.MapControllerRoute(
    name: "admin",
    pattern: "Admin/{action=Dashboard}/{id?}",
    defaults: new { controller = "Admin" });

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
