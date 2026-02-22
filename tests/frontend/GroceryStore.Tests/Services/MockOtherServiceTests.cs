using FluentAssertions;
using GroceryStore.Models;
using GroceryStore.Services.Mock;
using GroceryStore.Tests.Helpers;

namespace GroceryStore.Tests.Services;

// ══════════════════════════════════════════════════════════════════════════════
//  MockBrandServiceTests
// ══════════════════════════════════════════════════════════════════════════════
public class MockBrandServiceTests
{
    private readonly MockBrandService _sut = new( );

    [Fact]
    public async Task GetBrands_ReturnsOnlyActiveBrands()
    {
        MockDbFactory.Reset( );

        var brands = await _sut.GetBrandsAsync( );

        brands.Should( ).HaveCount(1);   // Brand B is inactive
        brands.Should( ).OnlyContain(b => b.IsActive);
    }

    [Fact]
    public async Task GetAllBrands_ReturnsAll_IncludingInactive()
    {
        MockDbFactory.Reset( );

        var brands = await _sut.GetAllBrandsAsync( );

        brands.Should( ).HaveCount(2);
    }

    [Fact]
    public async Task GetBrandById_ExistingId_ReturnsBrand()
    {
        MockDbFactory.Reset( );

        var brand = await _sut.GetBrandByIdAsync(1);

        brand.Should( ).NotBeNull( );
        brand!.Name.Should( ).Be("Brand A");
    }

    [Fact]
    public async Task GetBrandById_UnknownId_ReturnsNull()
    {
        MockDbFactory.Reset( );

        var brand = await _sut.GetBrandByIdAsync(999);

        brand.Should( ).BeNull( );
    }

    [Fact]
    public async Task CreateBrand_AssignsId_AndPersists()
    {
        MockDbFactory.Reset( );
        var before = MockDb.Brands.Count;

        var created = await _sut.CreateBrandAsync(new Brand
        {
            Name = "Brand C",
            Slug = "brand-c",
            IsActive = true
        });

        created.Id.Should( ).BeGreaterThan(0);
        MockDb.Brands.Should( ).HaveCount(before + 1);
    }

    [Fact]
    public async Task UpdateBrand_ChangesExistingRecord()
    {
        MockDbFactory.Reset( );

        await _sut.UpdateBrandAsync(new Brand { Id = 1,Name = "Brand A Updated",Slug = "brand-a",IsActive = true });

        MockDb.Brands.First(b => b.Id == 1).Name.Should( ).Be("Brand A Updated");
    }

    [Fact]
    public async Task DeleteBrand_ExistingId_RemovesAndReturnsTrue()
    {
        MockDbFactory.Reset( );

        var result = await _sut.DeleteBrandAsync(1);

        result.Should( ).BeTrue( );
        MockDb.Brands.Should( ).NotContain(b => b.Id == 1);
    }

    [Fact]
    public async Task DeleteBrand_UnknownId_ReturnsFalse()
    {
        MockDbFactory.Reset( );

        var result = await _sut.DeleteBrandAsync(999);

        result.Should( ).BeFalse( );
    }
}

// ══════════════════════════════════════════════════════════════════════════════
//  MockBannerServiceTests
// ══════════════════════════════════════════════════════════════════════════════
public class MockBannerServiceTests
{
    private readonly MockBannerService _sut = new( );

    [Fact]
    public async Task GetBanners_ReturnsOnlyActiveBanners()
    {
        MockDbFactory.Reset( );

        var banners = await _sut.GetBannersAsync( );

        banners.Should( ).HaveCount(1);
        banners.Should( ).OnlyContain(b => b.IsActive);
    }

    [Fact]
    public async Task GetBanners_ReturnsSortedByDisplayOrder()
    {
        MockDbFactory.Reset( );
        // Add a second active banner with lower DisplayOrder to verify sorting
        MockDb.Banners.Add(new Banner
        {
            Id = 3,
            Title = "Priority Banner",
            Placement = "HomeHero",
            DisplayOrder = 0,
            IsActive = true
        });

        var banners = await _sut.GetBannersAsync( );

        banners.Select(b => b.DisplayOrder).Should( ).BeInAscendingOrder( );
    }

    [Fact]
    public async Task GetAllBanners_ReturnsAll_IncludingInactive()
    {
        MockDbFactory.Reset( );

        var banners = await _sut.GetAllBannersAsync( );

        banners.Should( ).HaveCount(2);
    }

    [Fact]
    public async Task GetBannerById_ExistingId_ReturnsBanner()
    {
        MockDbFactory.Reset( );

        var banner = await _sut.GetBannerByIdAsync(1);

        banner.Should( ).NotBeNull( );
        banner!.Title.Should( ).Be("Summer Sale");
    }

    [Fact]
    public async Task GetBannerById_UnknownId_ReturnsNull()
    {
        MockDbFactory.Reset( );

        var banner = await _sut.GetBannerByIdAsync(999);

        banner.Should( ).BeNull( );
    }

    [Fact]
    public async Task CreateBanner_AssignsId_AndPersists()
    {
        MockDbFactory.Reset( );
        var before = MockDb.Banners.Count;

        var created = await _sut.CreateBannerAsync(new Banner
        {
            Title = "Flash Sale",
            Placement = "HomeHero",
            IsActive = true,
            DisplayOrder = 3
        });

        created.Id.Should( ).BeGreaterThan(0);
        MockDb.Banners.Should( ).HaveCount(before + 1);
    }

    [Fact]
    public async Task UpdateBanner_ChangesExistingRecord()
    {
        MockDbFactory.Reset( );

        await _sut.UpdateBannerAsync(new Banner { Id = 1,Title = "Updated Sale",Placement = "HomeHero",IsActive = true,DisplayOrder = 1 });

        MockDb.Banners.First(b => b.Id == 1).Title.Should( ).Be("Updated Sale");
    }

    [Fact]
    public async Task DeleteBanner_ExistingId_RemovesAndReturnsTrue()
    {
        MockDbFactory.Reset( );

        var result = await _sut.DeleteBannerAsync(1);

        result.Should( ).BeTrue( );
        MockDb.Banners.Should( ).NotContain(b => b.Id == 1);
    }

    [Fact]
    public async Task DeleteBanner_UnknownId_ReturnsFalse()
    {
        MockDbFactory.Reset( );

        var result = await _sut.DeleteBannerAsync(999);

        result.Should( ).BeFalse( );
    }
}

// ══════════════════════════════════════════════════════════════════════════════
//  MockSettingsServiceTests
// ══════════════════════════════════════════════════════════════════════════════
public class MockSettingsServiceTests
{
    private readonly MockSettingsService _sut = new( );

    [Fact]
    public async Task GetSettings_ReturnsCurrentSettings()
    {
        MockDbFactory.Reset( );

        var settings = await _sut.GetSettingsAsync( );

        settings.Should( ).NotBeNull( );
        settings!.StoreName.Should( ).Be("Test Store");
    }

    [Fact]
    public async Task SaveSettings_PersistsNewValues()
    {
        MockDbFactory.Reset( );

        var updated = new StoreSettings
        {
            StoreName = "My New Store",
            Phone = "+999 888 7777",
            WhatsappNumber = "+9998887777",
            Email = "new@store.com",
            Address = "New City",
            Currency = "EUR",
            OpeningHours = "9-5"
        };

        await _sut.SaveSettingsAsync(updated);

        var saved = await _sut.GetSettingsAsync( );
        saved!.StoreName.Should( ).Be("My New Store");
        saved.Currency.Should( ).Be("EUR");
    }

    [Fact]
    public async Task SaveSettings_ReturnsTheSavedObject()
    {
        MockDbFactory.Reset( );

        var input = new StoreSettings { StoreName = "Returned Store",Currency = "GBP" };
        var result = await _sut.SaveSettingsAsync(input);

        result.StoreName.Should( ).Be("Returned Store");
        result.Currency.Should( ).Be("GBP");
    }
}

// ══════════════════════════════════════════════════════════════════════════════
//  MockDashboardServiceTests
// ══════════════════════════════════════════════════════════════════════════════
public class MockDashboardServiceTests
{
    private readonly MockDashboardService _sut = new( );

    [Fact]
    public async Task GetStats_TotalCounts_MatchMockDb()
    {
        MockDbFactory.Reset( );

        var stats = await _sut.GetStatsAsync( );

        stats.TotalProducts.Should( ).Be(MockDb.Products.Count);
        stats.TotalCategories.Should( ).Be(MockDb.Categories.Count);
        stats.TotalBrands.Should( ).Be(MockDb.Brands.Count);
        stats.TotalBanners.Should( ).Be(MockDb.Banners.Count);
    }

    [Fact]
    public async Task GetStats_ActiveProducts_CountsOnlyActive()
    {
        MockDbFactory.Reset( );

        var stats = await _sut.GetStatsAsync( );

        var expectedActive = MockDb.Products.Count(p => p.IsActive);
        stats.ActiveProducts.Should( ).Be(expectedActive);
    }

    [Fact]
    public async Task GetStats_FeaturedProducts_CountsOnlyFeatured()
    {
        MockDbFactory.Reset( );

        var stats = await _sut.GetStatsAsync( );

        var expectedFeatured = MockDb.Products.Count(p => p.IsFeatured);
        stats.FeaturedProducts.Should( ).Be(expectedFeatured);
    }

    [Fact]
    public async Task GetStats_AfterAddingProduct_TotalIncreases()
    {
        MockDbFactory.Reset( );
        var before = (await _sut.GetStatsAsync( )).TotalProducts;

        MockDb.Products.Add(new Product { Id = 99,Name = "Extra",Sku = "X",IsActive = true });

        var after = (await _sut.GetStatsAsync( )).TotalProducts;
        after.Should( ).Be(before + 1);
    }
}
