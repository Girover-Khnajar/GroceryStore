using FluentAssertions;
using GroceryStore.App.Models;

namespace GroceryStore.App.Tests.Models;

// ══════════════════════════════════════════════════════════════════════════════
//  ProductModelTests  —  Tests computed properties and model correctness
// ══════════════════════════════════════════════════════════════════════════════
public class ProductModelTests
{
    // ── PrimaryImage ──────────────────────────────────────────────────────────

    [Fact]
    public void PrimaryImage_WithImages_ReturnsFirstImage()
    {
        var product = new Product
        {
            Images = new( ) { "https://example.com/img1.jpg","https://example.com/img2.jpg" }
        };

        product.PrimaryImage.Should( ).Be("https://example.com/img1.jpg");
    }

    [Fact]
    public void PrimaryImage_EmptyImages_ReturnsNull()
    {
        var product = new Product { Images = new( ) };

        product.PrimaryImage.Should( ).BeNull( );
    }

    // ── FormattedPrice ────────────────────────────────────────────────────────

    [Fact]
    public void FormattedPrice_WholeNumber_StripsDecimalZeros()
    {
        var product = new Product { Price = 2.00m,Currency = "USD",Unit = "kg" };

        product.FormattedPrice.Should( ).Be("2 USD / kg");
    }

    [Fact]
    public void FormattedPrice_WithCents_IncludesDecimals()
    {
        var product = new Product { Price = 1.50m,Currency = "USD",Unit = "kg" };

        product.FormattedPrice.Should( ).Be("1.5 USD / kg");
    }

    [Fact]
    public void FormattedPrice_ZeroPrice_ShowsZero()
    {
        var product = new Product { Price = 0m,Currency = "EUR",Unit = "pkt" };

        product.FormattedPrice.Should( ).Be("0 EUR / pkt");
    }

    [Fact]
    public void FormattedPrice_ContainsCurrencyAndUnit()
    {
        var product = new Product { Price = 3.99m,Currency = "IQD",Unit = "ltr" };

        product.FormattedPrice.Should( ).Contain("IQD");
        product.FormattedPrice.Should( ).Contain("ltr");
    }

    // ── Defaults ──────────────────────────────────────────────────────────────

    [Fact]
    public void NewProduct_DefaultIsActive_IsTrue()
    {
        var product = new Product( );

        product.IsActive.Should( ).BeTrue( );
    }

    [Fact]
    public void NewProduct_DefaultIsFeatured_IsFalse()
    {
        var product = new Product( );

        product.IsFeatured.Should( ).BeFalse( );
    }

    [Fact]
    public void NewProduct_DefaultCurrency_IsUsd()
    {
        var product = new Product( );

        product.Currency.Should( ).Be("USD");
    }

    [Fact]
    public void NewProduct_ImagesCollection_IsInitialized()
    {
        var product = new Product( );

        product.Images.Should( ).NotBeNull( );
        product.Images.Should( ).BeEmpty( );
    }
}

// ══════════════════════════════════════════════════════════════════════════════
//  PagedResultTests
// ══════════════════════════════════════════════════════════════════════════════
public class PagedResultTests
{
    [Theory]
    [InlineData(10,3,4)]   // 10 total, page size 3 → 4 pages
    [InlineData(12,12,1)]  // exact fit → 1 page
    [InlineData(13,12,2)]  // one extra → 2 pages
    [InlineData(0,10,0)]   // empty → 0 pages
    public void TotalPages_CalculatesCorrectly(int total,int pageSize,int expected)
    {
        var result = new PagedResult<Product>
        {
            TotalCount = total,
            PageSize = pageSize
        };

        result.TotalPages.Should( ).Be(expected);
    }

    [Fact]
    public void NewPagedResult_Items_IsInitialized()
    {
        var result = new PagedResult<Product>( );

        result.Items.Should( ).NotBeNull( );
        result.Items.Should( ).BeEmpty( );
    }
}

// ══════════════════════════════════════════════════════════════════════════════
//  ProductQueryDefaultsTests
// ══════════════════════════════════════════════════════════════════════════════
public class ProductQueryDefaultsTests
{
    [Fact]
    public void NewProductQuery_DefaultPage_IsOne()
    {
        var query = new ProductQuery( );

        query.Page.Should( ).Be(1);
    }

    [Fact]
    public void NewProductQuery_DefaultPageSize_IsTwelve()
    {
        var query = new ProductQuery( );

        query.PageSize.Should( ).Be(12);
    }

    [Fact]
    public void NewProductQuery_DefaultSortBy_IsNewest()
    {
        var query = new ProductQuery( );

        query.SortBy.Should( ).Be("newest");
    }

    [Fact]
    public void NewProductQuery_DefaultIsActive_IsTrue()
    {
        var query = new ProductQuery( );

        query.IsActive.Should( ).BeTrue( );
    }

    [Fact]
    public void NewProductQuery_OptionalFilters_AreNull()
    {
        var query = new ProductQuery( );

        query.CategoryId.Should( ).BeNull( );
        query.BrandId.Should( ).BeNull( );
        query.Search.Should( ).BeNull( );
        query.IsFeatured.Should( ).BeNull( );
    }
}

// ══════════════════════════════════════════════════════════════════════════════
//  BannerModelTests
// ══════════════════════════════════════════════════════════════════════════════
public class BannerModelTests
{
    [Theory]
    [InlineData("#products","products")]
    [InlineData("products","products")]
    [InlineData("#categories/fruits","categories/fruits")]
    [InlineData(null,null)]
    [InlineData("",null)]   // IsNullOrWhiteSpace("") == true → returns null
    public void NavRoute_StripsLeadingHash(string? linkUrl,string? expectedRoute)
    {
        var banner = new Banner { LinkUrl = linkUrl };

        banner.NavRoute.Should( ).Be(expectedRoute);
    }

    [Fact]
    public void NewBanner_DefaultIsActive_IsTrue()
    {
        var banner = new Banner( );

        banner.IsActive.Should( ).BeTrue( );
    }

    [Fact]
    public void NewBanner_ImagesCollection_IsInitialized()
    {
        var banner = new Banner( );

        banner.Images.Should( ).NotBeNull( );
        banner.Images.Should( ).BeEmpty( );
    }
}
