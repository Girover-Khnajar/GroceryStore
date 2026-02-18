using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using GroceryStore.Api.Contracts.Categories;
using GroceryStore.Api.Contracts.Products;
using GroceryStore.Application.Products.Dtos;

namespace GroceryStore.Api.Tests.Endpoints;

public class ProductEndpointsTests : IClassFixture<GroceryStoreApiFactory>
{
    private readonly HttpClient _client;

    public ProductEndpointsTests(GroceryStoreApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    private async Task<Guid> CreateCategoryAsync(string name, string slug)
    {
        var request = new CreateCategoryRequest(name, slug);
        var response = await _client.PostAsJsonAsync("/api/categories", request);
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<IdResponse>();
        return result!.Id;
    }

    private async Task<Guid> CreateProductAsync(Guid categoryId, string name, string slug)
    {
        var request = new CreateProductRequest(
            categoryId, name, slug, 9.99m, "SEK", "Kg");
        var response = await _client.PostAsJsonAsync("/api/products", request);
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<IdResponse>();
        return result!.Id;
    }

    #region GET /api/products/{id}

    [Fact]
    public async Task GetProductById_WhenProductExists_ReturnsProduct()
    {
        // Arrange
        var categoryId = await CreateCategoryAsync("Product Cat GetById", "prod-cat-getbyid");
        var productId = await CreateProductAsync(categoryId, "Test Product", "test-product-getbyid");

        // Act
        var response = await _client.GetAsync($"/api/products/{productId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var product = await response.Content.ReadFromJsonAsync<ProductDto>();
        product.Should().NotBeNull();
        product!.Name.Should().Be("Test Product");
        product.Slug.Should().Be("test-product-getbyid");
        product.PriceAmount.Should().Be(9.99m);
    }

    [Fact]
    public async Task GetProductById_WhenProductDoesNotExist_ReturnsNotFound()
    {
        // Act
        var response = await _client.GetAsync($"/api/products/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region GET /api/products/by-slug/{slug}

    [Fact]
    public async Task GetProductBySlug_WhenProductExists_ReturnsProduct()
    {
        // Arrange
        var categoryId = await CreateCategoryAsync("Product Cat BySlug", "prod-cat-byslug");
        await CreateProductAsync(categoryId, "Slug Product", "slug-product-test");

        // Act
        var response = await _client.GetAsync("/api/products/by-slug/slug-product-test");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var product = await response.Content.ReadFromJsonAsync<ProductDto>();
        product.Should().NotBeNull();
        product!.Slug.Should().Be("slug-product-test");
    }

    [Fact]
    public async Task GetProductBySlug_WhenProductDoesNotExist_ReturnsNotFound()
    {
        // Act
        var response = await _client.GetAsync("/api/products/by-slug/non-existent-product");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region GET /api/products/by-category/{categoryId}

    [Fact]
    public async Task GetProductsByCategoryId_WhenProductsExist_ReturnsProducts()
    {
        // Arrange
        var categoryId = await CreateCategoryAsync("Product Cat ByCat", "prod-cat-bycat");
        await CreateProductAsync(categoryId, "CatProd1", "catprod1-test");
        await CreateProductAsync(categoryId, "CatProd2", "catprod2-test");

        // Act
        var response = await _client.GetAsync($"/api/products/by-category/{categoryId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var products = await response.Content.ReadFromJsonAsync<List<ProductDto>>();
        products.Should().NotBeNull();
        products.Should().HaveCountGreaterThanOrEqualTo(2);
    }

    [Fact]
    public async Task GetProductsByCategoryId_WhenNoProducts_ReturnsEmptyList()
    {
        // Act
        var response = await _client.GetAsync($"/api/products/by-category/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var products = await response.Content.ReadFromJsonAsync<List<ProductDto>>();
        products.Should().NotBeNull();
        products.Should().BeEmpty();
    }

    #endregion

    #region POST /api/products

    [Fact]
    public async Task CreateProduct_WithValidRequest_ReturnsCreated()
    {
        // Arrange
        var categoryId = await CreateCategoryAsync("Product Cat Create", "prod-cat-create");
        var request = new CreateProductRequest(
            categoryId, "New Product", "new-product-create",
            19.99m, "SEK", "Kg",
            SortOrder: 1,
            IsFeatured: true,
            ShortDescription: "A short desc",
            LongDescription: "A longer description");

        // Act
        var response = await _client.PostAsJsonAsync("/api/products", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Headers.Location.Should().NotBeNull();
        var created = await response.Content.ReadFromJsonAsync<IdResponse>();
        created.Should().NotBeNull();
        created!.Id.Should().NotBeEmpty();
    }

    [Fact]
    public async Task CreateProduct_WithDuplicateSlug_ReturnsConflict()
    {
        // Arrange
        var categoryId = await CreateCategoryAsync("Product Cat Dup", "prod-cat-dup");
        await CreateProductAsync(categoryId, "First Product", "duplicate-product-slug");

        var duplicateRequest = new CreateProductRequest(
            categoryId, "Second Product", "duplicate-product-slug",
            15.00m, "SEK", "Kg");

        // Act
        var response = await _client.PostAsJsonAsync("/api/products", duplicateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task CreateProduct_WithNonExistentCategory_ReturnsNotFound()
    {
        // Arrange
        var request = new CreateProductRequest(
            Guid.NewGuid(), "Orphan Product", "orphan-product",
            10.00m, "SEK", "Kg");

        // Act
        var response = await _client.PostAsJsonAsync("/api/products", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region PUT /api/products/{id}

    [Fact]
    public async Task UpdateProduct_WithValidRequest_ReturnsNoContent()
    {
        // Arrange
        var categoryId = await CreateCategoryAsync("Product Cat Update", "prod-cat-update");
        var productId = await CreateProductAsync(categoryId, "Update Me", "update-me-product");

        var updateRequest = new UpdateProductRequest(
            categoryId, "Updated Product", "update-me-product",
            29.99m, "SEK", "Kg", 5, true,
            "Updated short", "Updated long", null, null);

        // Act
        var response = await _client.PutAsJsonAsync($"/api/products/{productId}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify the update
        var getResponse = await _client.GetAsync($"/api/products/{productId}");
        var product = await getResponse.Content.ReadFromJsonAsync<ProductDto>();
        product!.Name.Should().Be("Updated Product");
        product.PriceAmount.Should().Be(29.99m);
        product.IsFeatured.Should().BeTrue();
    }

    [Fact]
    public async Task UpdateProduct_WhenProductDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        var categoryId = await CreateCategoryAsync("Product Cat UpdNF", "prod-cat-updnf");
        var updateRequest = new UpdateProductRequest(
            categoryId, "No Such Product", "no-such-product",
            10.00m, "SEK", "Kg", 0, false,
            null, null, null, null);

        // Act
        var response = await _client.PutAsJsonAsync($"/api/products/{Guid.NewGuid()}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region DELETE /api/products/{id}

    [Fact]
    public async Task DeleteProduct_WhenProductExists_ReturnsNoContent()
    {
        // Arrange
        var categoryId = await CreateCategoryAsync("Product Cat Delete", "prod-cat-delete");
        var productId = await CreateProductAsync(categoryId, "Delete Me", "delete-me-product");

        // Act
        var response = await _client.DeleteAsync($"/api/products/{productId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task DeleteProduct_WhenProductDoesNotExist_ReturnsNotFound()
    {
        // Act
        var response = await _client.DeleteAsync($"/api/products/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    private sealed record IdResponse(Guid Id);
}
