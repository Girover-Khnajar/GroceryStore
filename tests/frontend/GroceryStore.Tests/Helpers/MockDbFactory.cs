using GroceryStore.Models;
using GroceryStore.Services.Mock;

namespace GroceryStore.Tests.Helpers;

public static class MockDbFactory
{
    /// <summary>
    /// Replaces all MockDb lists with a fresh, predictable set of seed data.
    /// Call at the start of any test that creates, updates, or deletes records.
    /// </summary>
    public static void Reset()
    {
        MockDb.Products.Clear( );
        MockDb.Categories.Clear( );
        MockDb.Brands.Clear( );
        MockDb.Banners.Clear( );

        // ── Seed Categories ───────────────────────────────────────────────────
        MockDb.Categories.AddRange(new[ ]
        {
            new Category { Id = 1, Name = "Vegetables", Slug = "vegetables", DisplayOrder = 1, IsActive = true  },
            new Category { Id = 2, Name = "Fruits",     Slug = "fruits",     DisplayOrder = 2, IsActive = true  },
            new Category { Id = 3, Name = "Dairy",      Slug = "dairy",      DisplayOrder = 3, IsActive = false },
        });

        // ── Seed Brands ───────────────────────────────────────────────────────
        MockDb.Brands.AddRange(new[ ]
        {
            new Brand { Id = 1, Name = "Brand A", Slug = "brand-a", IsActive = true  },
            new Brand { Id = 2, Name = "Brand B", Slug = "brand-b", IsActive = false },
        });

        // ── Seed Products ─────────────────────────────────────────────────────
        MockDb.Products.AddRange(new[ ]
        {
            new Product { Id = 1, Name = "Tomatoes",   Slug = "tomatoes",   CategoryId = 1, BrandId = 1,
                          Price = 1.50m, Currency = "USD", Unit = "kg", Sku = "VEG001",
                          IsActive = true,  IsFeatured = true,
                          Images = new() { "https://example.com/tomatoes.jpg" },
                          CreatedAt = DateTime.UtcNow.AddDays(-10) },

            new Product { Id = 2, Name = "Cucumber",   Slug = "cucumber",   CategoryId = 1, BrandId = 1,
                          Price = 0.80m, Currency = "USD", Unit = "kg", Sku = "VEG002",
                          IsActive = true,  IsFeatured = false,
                          Images = new(),
                          CreatedAt = DateTime.UtcNow.AddDays(-8) },

            new Product { Id = 3, Name = "Red Apple",  Slug = "red-apple",  CategoryId = 2, BrandId = 1,
                          Price = 2.00m, Currency = "USD", Unit = "kg", Sku = "FRT001",
                          IsActive = true,  IsFeatured = true,
                          Images = new() { "https://example.com/apple.jpg" },
                          CreatedAt = DateTime.UtcNow.AddDays(-6) },

            new Product { Id = 4, Name = "Banana",     Slug = "banana",     CategoryId = 2, BrandId = 2,
                          Price = 1.20m, Currency = "USD", Unit = "kg", Sku = "FRT002",
                          IsActive = false, IsFeatured = false,
                          Images = new(),
                          CreatedAt = DateTime.UtcNow.AddDays(-3) },
        });

        // ── Seed Banners ──────────────────────────────────────────────────────
        MockDb.Banners.AddRange(new[ ]
        {
            new Banner { Id = 1, Title = "Summer Sale",   Placement = "HomeHero", DisplayOrder = 1,
                         IsActive = true,  Images = new() { "https://example.com/banner1.jpg" } },
            new Banner { Id = 2, Title = "New Arrivals",  Placement = "HomeHero", DisplayOrder = 2,
                         IsActive = false, Images = new() { "https://example.com/banner2.jpg" } },
        });

        MockDb.Settings = new( )
        {
            StoreName = "Test Store",
            Phone = "+1 555 0000",
            WhatsappNumber = "+15550000",
            Email = "test@store.com",
            Address = "Test City",
            Currency = "USD",
            OpeningHours = "Mon-Fri: 9am-6pm",
        };
    }
}
