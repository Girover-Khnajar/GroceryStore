using FluentAssertions;
using GroceryStore.Models;
using GroceryStore.Services.Mock;
using GroceryStore.Tests.Helpers;

namespace GroceryStore.Tests.Services;

// ══════════════════════════════════════════════════════════════════════════════
//  MockProductServiceTests
//  ─────────────────────────────────────────────────────────────────────────────
//  Tests every public method of MockProductService.
//  Each test calls MockDbFactory.Reset() first so mutations from previous
//  tests do not bleed through (MockDb is a static in-memory store).
// ══════════════════════════════════════════════════════════════════════════════

public class MockProductServiceTests
{
    private readonly MockProductService _sut = new( );

    // ── GetProductsAsync ─────────────────────────────────────────────────────

    [Fact]
    public async Task GetProducts_DefaultQuery_ReturnsActiveProducts()
    {
        MockDbFactory.Reset( );

        var result = await _sut.GetProductsAsync(new ProductQuery( ));

        // Default query sets IsActive = true → only 3 of 4 seed products qualify
        result.Items.Should( ).HaveCount(3);
        result.Items.Should( ).OnlyContain(p => p.IsActive);
    }

    [Fact]
    public async Task GetProducts_NullIsActive_ReturnsAllProducts()
    {
        MockDbFactory.Reset( );

        var result = await _sut.GetProductsAsync(new ProductQuery { IsActive = null });

        result.TotalCount.Should( ).Be(4);
    }

    [Fact]
    public async Task GetProducts_FilterByCategory_ReturnsMatchingProducts()
    {
        MockDbFactory.Reset( );

        var result = await _sut.GetProductsAsync(new ProductQuery
        {
            CategoryId = 1,  // Vegetables
            IsActive = null
        });

        result.Items.Should( ).HaveCount(2);
        result.Items.Should( ).OnlyContain(p => p.CategoryId == 1);
    }

    [Fact]
    public async Task GetProducts_FilterByBrand_ReturnsMatchingProducts()
    {
        MockDbFactory.Reset( );

        var result = await _sut.GetProductsAsync(new ProductQuery
        {
            BrandId = 2,   // Brand B (only Banana)
            IsActive = null
        });

        result.Items.Should( ).HaveCount(1);
        result.Items.First( ).Name.Should( ).Be("Banana");
    }

    [Fact]
    public async Task GetProducts_SearchByName_ReturnsCaseInsensitiveMatch()
    {
        MockDbFactory.Reset( );

        var result = await _sut.GetProductsAsync(new ProductQuery
        {
            Search = "TOMATO",
            IsActive = null
        });

        result.Items.Should( ).HaveCount(1);
        result.Items.First( ).Slug.Should( ).Be("tomatoes");
    }

    [Fact]
    public async Task GetProducts_SearchBySku_ReturnsMatch()
    {
        MockDbFactory.Reset( );

        var result = await _sut.GetProductsAsync(new ProductQuery
        {
            Search = "FRT001",
            IsActive = null
        });

        result.Items.Should( ).ContainSingle(p => p.Sku == "FRT001");
    }

    [Fact]
    public async Task GetProducts_SearchNoMatch_ReturnsEmpty()
    {
        MockDbFactory.Reset( );

        var result = await _sut.GetProductsAsync(new ProductQuery { Search = "XYZ_NOT_EXIST" });

        result.Items.Should( ).BeEmpty( );
        result.TotalCount.Should( ).Be(0);
    }

    [Fact]
    public async Task GetProducts_FilterByFeatured_ReturnsOnlyFeatured()
    {
        MockDbFactory.Reset( );

        var result = await _sut.GetProductsAsync(new ProductQuery
        {
            IsFeatured = true,
            IsActive = null
        });

        result.Items.Should( ).HaveCount(2);
        result.Items.Should( ).OnlyContain(p => p.IsFeatured);
    }

    // ── Sort ──────────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetProducts_SortByPriceAscending_CorrectOrder()
    {
        MockDbFactory.Reset( );

        var result = await _sut.GetProductsAsync(new ProductQuery
        {
            SortBy = "price-asc",
            IsActive = null
        });

        var prices = result.Items.Select(p => p.Price).ToList( );
        prices.Should( ).BeInAscendingOrder( );
    }

    [Fact]
    public async Task GetProducts_SortByPriceDescending_CorrectOrder()
    {
        MockDbFactory.Reset( );

        var result = await _sut.GetProductsAsync(new ProductQuery
        {
            SortBy = "price-desc",
            IsActive = null
        });

        var prices = result.Items.Select(p => p.Price).ToList( );
        prices.Should( ).BeInDescendingOrder( );
    }

    [Fact]
    public async Task GetProducts_SortByName_AlphabeticOrder()
    {
        MockDbFactory.Reset( );

        var result = await _sut.GetProductsAsync(new ProductQuery
        {
            SortBy = "name",
            IsActive = null
        });

        var names = result.Items.Select(p => p.Name).ToList( );
        names.Should( ).BeInAscendingOrder( );
    }

    [Fact]
    public async Task GetProducts_DefaultSort_NewestFirst()
    {
        MockDbFactory.Reset( );

        var result = await _sut.GetProductsAsync(new ProductQuery
        {
            SortBy = "newest",
            IsActive = null
        });

        var dates = result.Items.Select(p => p.CreatedAt).ToList( );
        dates.Should( ).BeInDescendingOrder( );
    }

    // ── Pagination ────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetProducts_PageSize2_ReturnsCorrectPage()
    {
        MockDbFactory.Reset( );

        var page1 = await _sut.GetProductsAsync(new ProductQuery
        {
            Page = 1,
            PageSize = 2,
            IsActive = null
        });
        var page2 = await _sut.GetProductsAsync(new ProductQuery
        {
            Page = 2,
            PageSize = 2,
            IsActive = null
        });

        page1.Items.Should( ).HaveCount(2);
        page2.Items.Should( ).HaveCount(2);
        page1.Items.Select(p => p.Id).Should( ).NotIntersectWith(page2.Items.Select(p => p.Id));
    }

    [Fact]
    public async Task GetProducts_PageBeyondTotal_ReturnsEmpty()
    {
        MockDbFactory.Reset( );

        var result = await _sut.GetProductsAsync(new ProductQuery
        {
            Page = 99,
            PageSize = 10,
            IsActive = null
        });

        result.Items.Should( ).BeEmpty( );
        result.TotalCount.Should( ).Be(4); // total count unchanged
    }

    [Fact]
    public async Task GetProducts_TotalPagesCalculated_Correctly()
    {
        MockDbFactory.Reset( );

        var result = await _sut.GetProductsAsync(new ProductQuery
        {
            Page = 1,
            PageSize = 3,
            IsActive = null
        });

        result.TotalCount.Should( ).Be(4);
        result.TotalPages.Should( ).Be(2);
    }

    // ── Category Name Population ──────────────────────────────────────────────

    [Fact]
    public async Task GetProducts_PopulatesCategoryName_FromMockDb()
    {
        MockDbFactory.Reset( );

        var result = await _sut.GetProductsAsync(new ProductQuery { IsActive = null });

        result.Items.Should( ).OnlyContain(p => p.CategoryName != null);
        result.Items.First(p => p.CategoryId == 1).CategoryName.Should( ).Be("Vegetables");
    }

    // ── GetProductByIdAsync ───────────────────────────────────────────────────

    [Fact]
    public async Task GetProductById_ExistingId_ReturnsProduct()
    {
        MockDbFactory.Reset( );

        var product = await _sut.GetProductByIdAsync(1);

        product.Should( ).NotBeNull( );
        product!.Name.Should( ).Be("Tomatoes");
    }

    [Fact]
    public async Task GetProductById_NonExistingId_ReturnsNull()
    {
        MockDbFactory.Reset( );

        var product = await _sut.GetProductByIdAsync(9999);

        product.Should( ).BeNull( );
    }

    // ── GetProductBySlugAsync ─────────────────────────────────────────────────

    [Fact]
    public async Task GetProductBySlug_ExistingSlug_ReturnsProduct()
    {
        MockDbFactory.Reset( );

        var product = await _sut.GetProductBySlugAsync("red-apple");

        product.Should( ).NotBeNull( );
        product!.Id.Should( ).Be(3);
    }

    [Fact]
    public async Task GetProductBySlug_UnknownSlug_ReturnsNull()
    {
        MockDbFactory.Reset( );

        var result = await _sut.GetProductBySlugAsync("does-not-exist");

        result.Should( ).BeNull( );
    }

    // ── GetProductsByCategoryAsync ────────────────────────────────────────────

    [Fact]
    public async Task GetProductsByCategory_ReturnsOnlyActiveInCategory()
    {
        MockDbFactory.Reset( );

        var products = await _sut.GetProductsByCategoryAsync(2); // Fruits

        // Banana (id=4) is inactive → excluded
        products.Should( ).HaveCount(1);
        products.First( ).Slug.Should( ).Be("red-apple");
    }

    // ── GetFeaturedProductsAsync ──────────────────────────────────────────────

    [Fact]
    public async Task GetFeaturedProducts_ReturnsOnlyFeaturedAndActive()
    {
        MockDbFactory.Reset( );

        var featured = await _sut.GetFeaturedProductsAsync( );

        // Tomatoes (featured+active), RedApple (featured+active) = 2
        featured.Should( ).HaveCount(2);
        featured.Should( ).OnlyContain(p => p.IsFeatured && p.IsActive);
    }

    [Fact]
    public async Task GetFeaturedProducts_Limit_IsRespected()
    {
        MockDbFactory.Reset( );

        var featured = await _sut.GetFeaturedProductsAsync(limit: 1);

        featured.Should( ).HaveCount(1);
    }

    // ── SearchProductsAsync ───────────────────────────────────────────────────

    [Fact]
    public async Task SearchProducts_MatchesNameCaseInsensitive()
    {
        MockDbFactory.Reset( );

        var results = await _sut.SearchProductsAsync("tomato");

        results.Should( ).ContainSingle(p => p.Slug == "tomatoes");
    }

    [Fact]
    public async Task SearchProducts_OnlyReturnsActiveProducts()
    {
        MockDbFactory.Reset( );

        // "banana" is inactive → should not appear
        var results = await _sut.SearchProductsAsync("banana");

        results.Should( ).BeEmpty( );
    }

    // ── CreateProductAsync ────────────────────────────────────────────────────

    [Fact]
    public async Task CreateProduct_AssignsNewId_AndPersists()
    {
        MockDbFactory.Reset( );
        var before = MockDb.Products.Count;

        var newProduct = new Product
        {
            Name = "Spinach",
            Slug = "spinach",
            Sku = "VEG099",
            Price = 1.10m,
            Currency = "USD",
            Unit = "kg",
            CategoryId = 1,
            BrandId = 1,
            IsActive = true
        };

        var created = await _sut.CreateProductAsync(newProduct);

        created.Id.Should( ).BeGreaterThan(0);
        MockDb.Products.Should( ).HaveCount(before + 1);
        MockDb.Products.Should( ).Contain(p => p.Slug == "spinach");
    }

    [Fact]
    public async Task CreateProduct_SetsCreatedAt_ToNow()
    {
        MockDbFactory.Reset( );
        var before = DateTime.UtcNow.AddSeconds(-1);

        var created = await _sut.CreateProductAsync(new Product
        {
            Name = "Lettuce",
            Slug = "lettuce",
            Sku = "VEG100",
            Price = 0.90m,
            CategoryId = 1,
            BrandId = 1
        });

        created.CreatedAt.Should( ).BeAfter(before);
    }

    // ── UpdateProductAsync ────────────────────────────────────────────────────

    [Fact]
    public async Task UpdateProduct_ChangesExistingRecord()
    {
        MockDbFactory.Reset( );

        var updated = MockDb.Products.First(p => p.Id == 1) ;
        updated.Name = "Cherry Tomatoes";
        updated.Price = 2.50m;

        await _sut.UpdateProductAsync(updated);

        var inDb = MockDb.Products.First(p => p.Id == 1);
        inDb.Name.Should( ).Be("Cherry Tomatoes");
        inDb.Price.Should( ).Be(2.50m);
    }

    [Fact]
    public async Task UpdateProduct_UnknownId_DoesNotThrow()
    {
        MockDbFactory.Reset( );
        var ghost = new Product { Id = 9999,Name = "Ghost",Sku = "X" };

        var act = async () => await _sut.UpdateProductAsync(ghost);

        await act.Should( ).NotThrowAsync( );
    }

    // ── DeleteProductAsync ────────────────────────────────────────────────────

    [Fact]
    public async Task DeleteProduct_ExistingId_RemovesFromDb()
    {
        MockDbFactory.Reset( );
        var before = MockDb.Products.Count;

        var result = await _sut.DeleteProductAsync(1);

        result.Should( ).BeTrue( );
        MockDb.Products.Should( ).HaveCount(before - 1);
        MockDb.Products.Should( ).NotContain(p => p.Id == 1);
    }

    [Fact]
    public async Task DeleteProduct_UnknownId_ReturnsFalse()
    {
        MockDbFactory.Reset( );

        var result = await _sut.DeleteProductAsync(9999);

        result.Should( ).BeFalse( );
    }
}
