using GroceryStore.App.Contracts.Requests;
using GroceryStore.App.Models;
using GroceryStore.App.Services.Interfaces;

namespace GroceryStore.App.Services.Mock;

// ══════════════════════════════════════════════════════════════════════════════
//  MOCK DATABASE  —  Static in-memory store shared by all mock services.
//  All CRUD operations mutate these lists so changes persist within a session.
//  Swap these services for the Http implementations when the real API is ready.
// ══════════════════════════════════════════════════════════════════════════════
public static class MockDb
{
    // ── Categories ────────────────────────────────────────────────────────────
    public static List<Category> Categories { get; } = new( )
    {
        new() { Id = Guid.NewGuid(), Name = "Fresh Vegetables", Slug = "vegetables",
                SortOrder = 1, IsActive = true,
                ImageUrl = "https://picsum.photos/seed/category-vegetables/900/675" },
        new() { Id = Guid.NewGuid(), Name = "Fruits",            Slug = "fruits",
                SortOrder = 2, IsActive = true,
                ImageUrl = "https://picsum.photos/seed/category-fruits/900/675" },
        new() { Id = Guid.NewGuid(), Name = "Dairy Products",   Slug = "dairy",
                SortOrder = 3, IsActive = true,
                ImageUrl = "https://picsum.photos/seed/category-dairy/900/675" },
        new() { Id = Guid.NewGuid(), Name = "Beverages",         Slug = "beverages",
                SortOrder = 4, IsActive = true,
                ImageUrl = "https://picsum.photos/seed/category-beverages/900/675" },
        new() { Id = Guid.NewGuid(), Name = "Canned Goods",      Slug = "canned",
                SortOrder = 5, IsActive = true,
                ImageUrl = "https://picsum.photos/seed/category-canned/900/675" },
        new() { Id = Guid.NewGuid(), Name = "Frozen Foods",      Slug = "frozen",
                SortOrder = 6, IsActive = false,
                ImageUrl = "https://picsum.photos/seed/category-frozen/900/675" },
    };

    // ── Brands ────────────────────────────────────────────────────────────────
    public static List<Brand> Brands { get; } = new( )
    {
        new() { Id = Guid.NewGuid(), Name = "Fresh Valley",  Slug = "fresh-valley",  IsActive = true  },
        new() { Id = Guid.NewGuid(), Name = "Golden Farm",   Slug = "golden-farm",   IsActive = true  },
        new() { Id = Guid.NewGuid(), Name = "Pure Life",     Slug = "pure-life",     IsActive = true  },
        new() { Id = Guid.NewGuid(), Name = "Nature's Best", Slug = "natures-best",  IsActive = true  },
        new() { Id = Guid.NewGuid(), Name = "Local Harvest", Slug = "local-harvest", IsActive = false },
    };

    // ── Products ──────────────────────────────────────────────────────────────
    public static List<Product> Products { get; } = new( )
    {
        // Vegetables
        new() { Id = Guid.NewGuid(),  Name = "Fresh Tomatoes",    Slug = "fresh-tomatoes",
                CategoryId = Guid.NewGuid(), Brand = "Fresh Valley", CategoryName = "Fresh Vegetables", BrandName = "Fresh Valley",
                Price = 1.50m, Currency = "USD", Unit = "kg",   Sku = "VEG001",
                Description = "Locally sourced fresh tomatoes. Perfect for salads and cooking.",
                IsActive = true, IsFeatured = true,
                Images = new() { "https://picsum.photos/seed/fresh-tomatoes/900/900",
                                 "https://picsum.photos/seed/tomatoes-2/900/900" },
                CreatedAt = DateTime.UtcNow.AddDays(-30) },

        new() { Id = Guid.NewGuid(),  Name = "Fresh Cucumber",    Slug = "fresh-cucumber",
                CategoryId = Guid.NewGuid(), Brand = "Fresh Valley", CategoryName = "Fresh Vegetables", BrandName = "Fresh Valley",
                Price = 0.80m, Currency = "USD", Unit = "kg",   Sku = "VEG002",
                Description = "Crisp and refreshing cucumbers, rich in water content and vitamins.",
                IsActive = true, IsFeatured = false,
                Images = new() { "https://picsum.photos/seed/fresh-cucumber/900/900" },
                CreatedAt = DateTime.UtcNow.AddDays(-28) },

        new() { Id = Guid.NewGuid(),  Name = "Potatoes",           Slug = "potatoes",
                CategoryId = Guid.NewGuid(), Brand = "Golden Farm", CategoryName = "Fresh Vegetables", BrandName = "Golden Farm",
                Price = 0.60m, Currency = "USD", Unit = "kg",   Sku = "VEG003",
                Description = "Fresh potatoes, ideal for frying, roasting, and boiling.",
                IsActive = true, IsFeatured = false,
                Images = new() { "https://picsum.photos/seed/potatoes/900/900" },
                CreatedAt = DateTime.UtcNow.AddDays(-25) },

        new() { Id = Guid.NewGuid(),  Name = "Red Onion",          Slug = "red-onion",
                CategoryId = Guid.NewGuid(), Brand = "Golden Farm", CategoryName = "Fresh Vegetables", BrandName = "Golden Farm",
                Price = 0.70m, Currency = "USD", Unit = "kg",   Sku = "VEG004",
                Description = "Fresh red onions with a distinctive flavour.",
                IsActive = true, IsFeatured = false,
                Images = new() { "https://picsum.photos/seed/red-onion/900/900" },
                CreatedAt = DateTime.UtcNow.AddDays(-22) },

        // Fruits
        new() { Id = Guid.NewGuid(),  Name = "Red Apple",          Slug = "red-apple",
                CategoryId = Guid.NewGuid(), Brand = "Fresh Valley", CategoryName = "Fruits", BrandName = "Fresh Valley",
                Price = 2.00m, Currency = "USD", Unit = "kg",   Sku = "FRT001",
                Description = "Sweet and crunchy red apples, packed with antioxidants.",
                IsActive = true, IsFeatured = true,
                Images = new() { "https://picsum.photos/seed/red-apple/900/900",
                                 "https://picsum.photos/seed/apple-2/900/900" },
                CreatedAt = DateTime.UtcNow.AddDays(-20) },

        new() { Id = Guid.NewGuid(),  Name = "Banana",             Slug = "banana",
                CategoryId = Guid.NewGuid(), Brand = "Nature's Best", CategoryName = "Fruits", BrandName = "Nature's Best",
                Price = 1.20m, Currency = "USD", Unit = "kg",   Sku = "FRT002",
                Description = "Fresh bananas, rich in potassium and natural energy.",
                IsActive = true, IsFeatured = true,
                Images = new() { "https://picsum.photos/seed/banana/900/900" },
                CreatedAt = DateTime.UtcNow.AddDays(-18) },

        new() { Id = Guid.NewGuid(),  Name = "Orange",             Slug = "orange",
                CategoryId = Guid.NewGuid(), Brand = "Fresh Valley", CategoryName = "Fruits", BrandName = "Fresh Valley",
                Price = 1.50m, Currency = "USD", Unit = "kg",   Sku = "FRT003",
                Description = "Juicy oranges packed with vitamin C. Great for juicing.",
                IsActive = true, IsFeatured = false,
                Images = new() { "https://picsum.photos/seed/orange/900/900" },
                CreatedAt = DateTime.UtcNow.AddDays(-15) },

        new() { Id = Guid.NewGuid(),  Name = "Strawberries",       Slug = "strawberries",
                CategoryId = Guid.NewGuid(), Brand = "Nature's Best", CategoryName = "Fruits", BrandName = "Nature's Best",
                Price = 3.20m, Currency = "USD", Unit = "pkt",  Sku = "FRT004",
                Description = "Fresh strawberries, perfect for desserts and smoothies.",
                IsActive = true, IsFeatured = true,
                Images = new() { "https://picsum.photos/seed/strawberries/900/900" },
                CreatedAt = DateTime.UtcNow.AddDays(-13) },

        // Dairy
        new() { Id = Guid.NewGuid(),  Name = "Whole Milk",         Slug = "whole-milk",
                CategoryId = Guid.NewGuid(), Brand = "Pure Life", CategoryName = "Dairy Products", BrandName = "Pure Life",
                Price = 1.80m, Currency = "USD", Unit = "ltr",  Sku = "DRY001",
                Description = "Full-fat fresh milk, rich in calcium and protein.",
                IsActive = true, IsFeatured = true,
                Images = new() { "https://picsum.photos/seed/fresh-milk/900/900" },
                CreatedAt = DateTime.UtcNow.AddDays(-12) },

        new() { Id = Guid.NewGuid(), Name = "White Cheese",       Slug = "white-cheese",
                CategoryId = Guid.NewGuid(), Brand = "Pure Life", CategoryName = "Dairy Products", BrandName = "Pure Life",
                Price = 3.50m, Currency = "USD", Unit = "kg",   Sku = "DRY002",
                Description = "Fresh high-quality white cheese.",
                IsActive = true, IsFeatured = false,
                Images = new() { "https://picsum.photos/seed/white-cheese/900/900" },
                CreatedAt = DateTime.UtcNow.AddDays(-10) },

        new() { Id = Guid.NewGuid(), Name = "Greek Yogurt",       Slug = "greek-yogurt",
                CategoryId = Guid.NewGuid(), Brand = "Nature's Best", CategoryName = "Dairy Products", BrandName = "Nature's Best",
                Price = 2.20m, Currency = "USD", Unit = "pkt",  Sku = "DRY003",
                Description = "Thick and creamy authentic Greek yogurt.",
                IsActive = true, IsFeatured = true,
                Images = new() { "https://picsum.photos/seed/greek-yogurt/900/900" },
                CreatedAt = DateTime.UtcNow.AddDays(-8) },

        // Beverages
        new() { Id = Guid.NewGuid(), Name = "Orange Juice",       Slug = "orange-juice",
                CategoryId = Guid.NewGuid(), Brand = "Golden Farm", CategoryName = "Beverages", BrandName = "Golden Farm",
                Price = 2.80m, Currency = "USD", Unit = "ltr",  Sku = "BEV001",
                Description = "100% natural orange juice, no preservatives or additives.",
                IsActive = true, IsFeatured = true,
                Images = new() { "https://picsum.photos/seed/orange-juice/900/900",
                                 "https://picsum.photos/seed/juice-2/900/900" },
                CreatedAt = DateTime.UtcNow.AddDays(-6) },

        new() { Id = Guid.NewGuid(), Name = "Mineral Water",      Slug = "mineral-water",
                CategoryId = Guid.NewGuid(), Brand = "Pure Life", CategoryName = "Beverages", BrandName = "Pure Life",
                Price = 0.50m, Currency = "USD", Unit = "btl",  Sku = "BEV002",
                Description = "Pure natural spring water.",
                IsActive = true, IsFeatured = false,
                Images = new() { "https://picsum.photos/seed/mineral-water/900/900" },
                CreatedAt = DateTime.UtcNow.AddDays(-4) },

        // Canned
        new() { Id = Guid.NewGuid(), Name = "Canned Tuna",        Slug = "canned-tuna",
                CategoryId = Guid.NewGuid(), Brand = "Nature's Best", CategoryName = "Canned Goods", BrandName = "Nature's Best",
                Price = 1.90m, Currency = "USD", Unit = "can",  Sku = "CAN001",
                Description = "Tuna in olive oil, rich in protein and Omega-3 fatty acids.",
                IsActive = true, IsFeatured = false,
                Images = new() { "https://picsum.photos/seed/canned-tuna/900/900" },
                CreatedAt = DateTime.UtcNow.AddDays(-2) },

        new() { Id = Guid.NewGuid(), Name = "Kidney Beans",       Slug = "kidney-beans",
                CategoryId = Guid.NewGuid(), Brand = "Golden Farm", CategoryName = "Canned Goods", BrandName = "Golden Farm",
                Price = 1.10m, Currency = "USD", Unit = "can",  Sku = "CAN002",
                Description = "Ready-to-use canned kidney beans.",
                IsActive = true, IsFeatured = false,
                Images = new() { "https://picsum.photos/seed/kidney-beans/900/900" },
                CreatedAt = DateTime.UtcNow.AddDays(-1) },
    };

    // ── Banners ───────────────────────────────────────────────────────────────
    public static List<Banner> Banners { get; } = new( )
    {
        new() { Id = Guid.NewGuid(), Title = "This Week's Deals 🛒",     Placement = "HomeHero",
                DisplayOrder = 1, IsActive = true, LinkUrl = "products",
                ImageUrl = "https://picsum.photos/seed/banner-weekly-1/1200/400",
                Images = new() { "https://picsum.photos/seed/banner-weekly-1/1200/400",
                                 "https://picsum.photos/seed/banner-weekly-2/1200/400",
                                 "https://picsum.photos/seed/banner-weekly-3/1200/400" },
                StartDate = DateTime.UtcNow.AddDays(-7) },

        new() { Id = Guid.NewGuid(), Title = "Fresh Products Daily 🥦",  Placement = "HomeHero",
                DisplayOrder = 2, IsActive = true, LinkUrl = "categories",
                ImageUrl = "https://picsum.photos/seed/banner-fresh-1/1200/400",
                Images = new() { "https://picsum.photos/seed/banner-fresh-1/1200/400",
                                 "https://picsum.photos/seed/banner-fresh-2/1200/400" },
                StartDate = DateTime.UtcNow.AddDays(-3) },

        new() { Id = Guid.NewGuid(), Title = "20% Off All Fruits 🍎",    Placement = "HomeHero",
                DisplayOrder = 3, IsActive = false, LinkUrl = "categories/fruits",
                ImageUrl = "https://picsum.photos/seed/banner-fruit-sale/1200/400",
                Images = new() { "https://picsum.photos/seed/banner-fruit-sale/1200/400" },
                StartDate = DateTime.UtcNow, EndDate = DateTime.UtcNow.AddDays(7) },
    };

    // ── Settings ──────────────────────────────────────────────────────────────
    public static StoreSettings Settings { get; set; } = new( )
    {
        StoreName = "Fresh Grocery Store",
        Phone = "+964 750 123 4567",
        WhatsappNumber = "+9647501234567",
        Email = "info@freshgrocery.com",
        Address = "Erbil, Kurdistan Region, Iraq",
        Currency = "USD",
        OpeningHours = "Saturday – Thursday: 8:00 AM – 10:00 PM\nFriday: 9:00 AM – 11:00 PM",
        GoogleMapsUrl = "https://www.google.com/maps/embed?pb=!1m18!1m12!1m3!1d51234.1!2d44.0094!3d36.1900!2m3!1f0!2f0!3f0!3m2!1i1024!2i768!4f13.1!3m3!1m2!1s0x0%3A0x0!2zMzbCsDExJzI0LjAiTiA0NMKwMDAnMzMuOCJF!5e0!3m2!1sen!2siq!4v1234567890"
    };

    // ── Next IDs (auto-increment) ─────────────────────────────────────────────
    public static Guid NextProductId => Guid.NewGuid();
    public static Guid NextCategoryId => Guid.NewGuid();
    public static Guid NextBrandId => Guid.NewGuid();
    public static Guid NextBannerId => Guid.NewGuid();
}

// ══════════════════════════════════════════════════════════════════════════════
//  MockProductService
// ══════════════════════════════════════════════════════════════════════════════
public sealed class MockProductService : IProductService
{
    public Task<PagedResult<Product>> GetProductsAsync(GetProductsRequest q)
    {
        var query = MockDb.Products.AsEnumerable( );

        if (q.IsActive.HasValue)
            query = query.Where(p => p.IsActive == q.IsActive);
        if (q.IsFeatured.HasValue)
            query = query.Where(p => p.IsFeatured == q.IsFeatured);
        if (q.CategoryId.HasValue)
            query = query.Where(p => p.CategoryId == q.CategoryId);
        // if (!string.IsNullOrWhiteSpace(q.Brand))
        //     query = query.Where(p => p.BrandId == q.Brand);

        if (!string.IsNullOrWhiteSpace(q.Search))
        {
            var s = q.Search.ToLower( );
            query = query.Where(p =>
                p.Name.ToLower( ).Contains(s) ||
                p.Sku.ToLower( ).Contains(s) ||
                (p.Description?.ToLower( ).Contains(s) ?? false));
        }

        query = q.Sort switch
        {
            "price-asc" => query.OrderBy(p => p.Price),
            "price-desc" => query.OrderByDescending(p => p.Price),
            "name" => query.OrderBy(p => p.Name),
            _ => query.OrderByDescending(p => p.CreatedAt)
        };

        var list = query.ToList( );
        foreach (var p in list)
        {
            p.CategoryName = MockDb.Categories.FirstOrDefault(c => c.Id == p.CategoryId)?.Name;
            p.BrandName = p.Brand;
        }

        var total = list.Count;
        var paged = list.Skip((q.Page - 1) * q.PageSize).Take(q.PageSize).ToList( );

        return Task.FromResult(new PagedResult<Product>
        {
            Items = paged,
            TotalCount = total,
            Page = q.Page,
            PageSize = q.PageSize
        });
    }

    public Task<Product?> GetProductByIdAsync(Guid id)
        => Task.FromResult(MockDb.Products.FirstOrDefault(p => p.Id == id));

    public Task<Product?> GetProductBySlugAsync(string slug)
        => Task.FromResult(MockDb.Products.FirstOrDefault(p => p.Slug == slug));

    public Task<List<Product>> GetProductsByCategoryAsync(Guid categoryId)
        => Task.FromResult(MockDb.Products.Where(p => p.CategoryId == categoryId && p.IsActive).ToList( ));

    public Task<List<Product>> GetFeaturedProductsAsync(int limit = 6)
        => Task.FromResult(MockDb.Products.Where(p => p.IsFeatured && p.IsActive).Take(limit).ToList( ));

    public Task<List<Product>> SearchProductsAsync(string query)
    {
        var s = query.ToLower( );
        return Task.FromResult(MockDb.Products.Where(p => p.IsActive && (
            p.Name.ToLower( ).Contains(s) ||
            p.Sku.ToLower( ).Contains(s) ||
            (p.Description?.ToLower( ).Contains(s) ?? false))).Take(20).ToList( ));
    }

    public Task<Product> CreateProductAsync(Product product)
    {
        product.Id = MockDb.NextProductId;
        product.CreatedAt = DateTime.UtcNow;
        MockDb.Products.Add(product);
        return Task.FromResult(product);
    }

    public Task<Product> UpdateProductAsync(Product product)
    {
        var idx = MockDb.Products.FindIndex(p => p.Id == product.Id);
        if (idx >= 0)
            MockDb.Products[idx] = product;
        return Task.FromResult(product);
    }

    public Task<bool> DeleteProductAsync(Guid id)
        => Task.FromResult(MockDb.Products.RemoveAll(p => p.Id == id) > 0);
}

// ══════════════════════════════════════════════════════════════════════════════
//  MockCategoryService
// ══════════════════════════════════════════════════════════════════════════════
public sealed class MockCategoryService : ICategoryService
{
    public Task<List<Category>> GetCategoriesAsync()
        => Task.FromResult(MockDb.Categories.Where(c => c.IsActive).OrderBy(c => c.SortOrder).ToList( ));

    public Task<List<Category>> GetAllCategoriesAsync()
    {
        return Task.FromResult(MockDb.Categories.OrderBy(c => c.SortOrder).ToList( ));
    }

    public Task<Category?> GetCategoryByIdAsync(Guid id)
        => Task.FromResult(MockDb.Categories.FirstOrDefault(c => c.Id == id));

    public Task<Category?> GetCategoryBySlugAsync(string slug)
        => Task.FromResult(MockDb.Categories.FirstOrDefault(c => c.Slug == slug));

    public Task<Category> CreateCategoryAsync(Category category)
    {
        category.Id = MockDb.NextCategoryId;
        MockDb.Categories.Add(category);
        return Task.FromResult(category);
    }

    public Task<Category> UpdateCategoryAsync(Category category)
    {
        var idx = MockDb.Categories.FindIndex(c => c.Id == category.Id);
        if (idx >= 0)
            MockDb.Categories[idx] = category;
        return Task.FromResult(category);
    }

    public Task<bool> DeleteCategoryAsync(Guid id)
        => Task.FromResult(MockDb.Categories.RemoveAll(c => c.Id == id) > 0);
}

// ══════════════════════════════════════════════════════════════════════════════
//  MockBrandService
// ══════════════════════════════════════════════════════════════════════════════
public sealed class MockBrandService : IBrandService
{
    public Task<List<Brand>> GetBrandsAsync()
        => Task.FromResult(MockDb.Brands.Where(b => b != null && b.IsActive).ToList());

    public Task<List<Brand>> GetAllBrandsAsync()
        => Task.FromResult(MockDb.Brands.ToList( ));

    public Task<Brand?> GetBrandByIdAsync(Guid id)
        => Task.FromResult(MockDb.Brands.FirstOrDefault(b => b.Id == id));

    public Task<Brand> CreateBrandAsync(Brand brand)
    {
        brand.Id = MockDb.NextBrandId;
        MockDb.Brands.Add(brand);
        return Task.FromResult(brand);
    }

    public Task<Brand> UpdateBrandAsync(Brand brand)
    {
        var idx = MockDb.Brands.FindIndex(b => b.Id == brand.Id);
        if (idx >= 0)
            MockDb.Brands[idx] = brand;
        return Task.FromResult(brand);
    }

    public Task<bool> DeleteBrandAsync(Guid id)
        => Task.FromResult(MockDb.Brands.RemoveAll(b => b.Id == id) > 0);
}

// ══════════════════════════════════════════════════════════════════════════════
//  MockBannerService
// ══════════════════════════════════════════════════════════════════════════════
public sealed class MockBannerService : IBannerService
{
    public Task<List<Banner>> GetBannersAsync()
        => Task.FromResult(MockDb.Banners.Where(b => b.IsActive).OrderBy(b => b.DisplayOrder).ToList( ));

    public Task<List<Banner>> GetAllBannersAsync()
        => Task.FromResult(MockDb.Banners.OrderBy(b => b.DisplayOrder).ToList( ));

    public Task<Banner?> GetBannerByIdAsync(Guid id)
        => Task.FromResult(MockDb.Banners.FirstOrDefault(b => b.Id == id));

    public Task<Banner> CreateBannerAsync(Banner banner)
    {
        banner.Id = MockDb.NextBannerId;
        MockDb.Banners.Add(banner);
        return Task.FromResult(banner);
    }

    public Task<Banner> UpdateBannerAsync(Banner banner)
    {
        var idx = MockDb.Banners.FindIndex(b => b.Id == banner.Id);
        if (idx >= 0)
            MockDb.Banners[idx] = banner;
        return Task.FromResult(banner);
    }

    public Task<bool> DeleteBannerAsync(Guid id)
        => Task.FromResult(MockDb.Banners.RemoveAll(b => b.Id == id) > 0);
}

// ══════════════════════════════════════════════════════════════════════════════
//  MockSettingsService
// ══════════════════════════════════════════════════════════════════════════════
public sealed class MockSettingsService : ISettingsService
{
    public Task<StoreSettings?> GetSettingsAsync()
        => Task.FromResult<StoreSettings?>(MockDb.Settings);

    public Task<StoreSettings> SaveSettingsAsync(StoreSettings settings)
    {
        MockDb.Settings = settings;
        return Task.FromResult(settings);
    }
}

// ══════════════════════════════════════════════════════════════════════════════
//  MockDashboardService
// ══════════════════════════════════════════════════════════════════════════════
public sealed class MockDashboardService : IDashboardService
{
    public Task<DashboardStats> GetStatsAsync()
        => Task.FromResult(new DashboardStats
        {
            TotalProducts = MockDb.Products.Count,
            TotalCategories = MockDb.Categories.Count,
            TotalBrands = MockDb.Brands.Count,
            TotalBanners = MockDb.Banners.Count,
            ActiveProducts = MockDb.Products.Count(p => p.IsActive),
            FeaturedProducts = MockDb.Products.Count(p => p.IsFeatured)
        });
}

// ══════════════════════════════════════════════════════════════════════════════
//  MockAuthService
// ══════════════════════════════════════════════════════════════════════════════
public sealed class MockAuthService : IAuthService
{
    private const string AdminUser = "admin";
    private const string AdminPass = "admin123";

    private bool _isAuthenticated;

    public bool IsAuthenticated => _isAuthenticated;
    public event Action? AuthStateChanged;

    public Task<string?> LoginAsync(string username,string password)
    {
        if (username == AdminUser && password == AdminPass)
        {
            _isAuthenticated = true;
            AuthStateChanged?.Invoke( );
            return Task.FromResult<string?>("mock-jwt-token");
        }
        return Task.FromResult<string?>(null);
    }

    public Task LogoutAsync()
    {
        _isAuthenticated = false;
        AuthStateChanged?.Invoke( );
        return Task.CompletedTask;
    }
}
