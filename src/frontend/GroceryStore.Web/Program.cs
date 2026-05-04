using GroceryStore.Application;
using GroceryStore.Domain.Entities;
using GroceryStore.Domain.Interfaces;
using GroceryStore.Infrastructure;
using GroceryStore.Infrastructure.Persistence;
using GroceryStore.Web.Services;
using GroceryStore.Web.Services.Localization;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
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
builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
builder.Services.AddScoped<ImageUrlHelper>();

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

// ── Seed admin user ────────────────────────────────────────────────────
using (var scope = app.Services.CreateScope())
{
    var db       = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var users    = scope.ServiceProvider.GetRequiredService<IUserRepository>();
    var hasher   = scope.ServiceProvider.GetRequiredService<IPasswordHasher<User>>();

    await db.Database.MigrateAsync();

    if (!await users.AnyAsync())
    {
        var adminUsername = builder.Configuration["AdminCredentials:Username"] ?? "admin";
        var adminPassword = builder.Configuration["AdminCredentials:Password"] ?? "Admin@1234";

        var admin = new User { Username = adminUsername };
        admin.PasswordHash = hasher.HashPassword(admin, adminPassword);
        await users.AddAsync(admin);
    }
}

app.Run();
