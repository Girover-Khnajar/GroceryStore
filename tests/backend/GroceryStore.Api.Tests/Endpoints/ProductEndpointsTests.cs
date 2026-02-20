using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using GroceryStore.Api.Contracts.Categories;
using GroceryStore.Api.Contracts.Images;
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

    private async Task<Guid> CreateImageAsync(string fileName = "test.jpg")
    {
        var request = new CreateImageAssetRequest(
            StoragePath: $"/images/{fileName}",
            Url: $"https://cdn.example.com/images/{fileName}",
            FileName: fileName,
            ContentType: "image/jpeg",
            FileSizeBytes: 102400,
            Width: 800,
            Height: 600,
            AltText: "A test image");

        var response = await _client.PostAsJsonAsync("/api/images", request);
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

    #region POST /api/products/{id}/images

    [Fact]
    public async Task AssignProductImage_WithValidRequest_ReturnsNoContent_AndProductContainsImageRef()
    {
        // Arrange
        var categoryId = await CreateCategoryAsync("Product Cat Img", "prod-cat-img");
        var productId = await CreateProductAsync(categoryId, "Product With Image", "product-with-image");
        var imageId = await CreateImageAsync("assigned.jpg");

        var assignRequest = new AssignProductImageRequest(
            ImageId: imageId,
            MakePrimary: true,
            SortOrder: 1,
            AltText: "Hero image");

        // Act
        var response = await _client.PostAsJsonAsync($"/api/products/{productId}/images", assignRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var getResponse = await _client.GetAsync($"/api/products/{productId}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var product = await getResponse.Content.ReadFromJsonAsync<ProductDto>();
        product.Should().NotBeNull();
        product!.ImageRefs.Should().ContainSingle(r => r.ImageId == imageId);
        product.ImageRefs.Single(r => r.ImageId == imageId).IsPrimary.Should().BeTrue();
    }

    [Fact]
    public async Task AssignProductImage_WhenImageAlreadyAssigned_IsIdempotent_AndDoesNotDuplicate()
    {
        // Arrange
        var categoryId = await CreateCategoryAsync("Product Cat Img2", "prod-cat-img2");
        var productId = await CreateProductAsync(categoryId, "Product With Image 2", "product-with-image-2");
        var imageId = await CreateImageAsync("assigned2.jpg");

        var assignRequest = new AssignProductImageRequest(ImageId: imageId);

        // Act
        var response1 = await _client.PostAsJsonAsync($"/api/products/{productId}/images", assignRequest);
        var response2 = await _client.PostAsJsonAsync($"/api/products/{productId}/images", assignRequest);

        // Assert
        response1.StatusCode.Should().Be(HttpStatusCode.NoContent);
        response2.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var getResponse = await _client.GetAsync($"/api/products/{productId}");
        var product = await getResponse.Content.ReadFromJsonAsync<ProductDto>();
        product!.ImageRefs.Count(r => r.ImageId == imageId).Should().Be(1);
    }

    [Fact]
    public async Task AssignProductImage_WhenProductDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        var imageId = await CreateImageAsync("orphan.jpg");
        var assignRequest = new AssignProductImageRequest(ImageId: imageId);

        // Act
        var response = await _client.PostAsJsonAsync($"/api/products/{Guid.NewGuid()}/images", assignRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task AssignProductImage_WhenImageDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        var categoryId = await CreateCategoryAsync("Product Cat ImgNF", "prod-cat-imgnf");
        var productId = await CreateProductAsync(categoryId, "Product No Image", "product-no-image");
        var assignRequest = new AssignProductImageRequest(ImageId: Guid.NewGuid());

        // Act
        var response = await _client.PostAsJsonAsync($"/api/products/{productId}/images", assignRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task SetPrimaryProductImage_WhenImageIsAttached_SetsItAsPrimary()
    {
        // Arrange
        var categoryId = await CreateCategoryAsync("Product Cat Prim", "prod-cat-prim");
        var productId = await CreateProductAsync(categoryId, "Product Primary", "product-primary");

        var imageId1 = await CreateImageAsync("prim-1.jpg");
        var imageId2 = await CreateImageAsync("prim-2.jpg");

        (await _client.PostAsJsonAsync($"/api/products/{productId}/images", new AssignProductImageRequest(imageId1)))
            .StatusCode.Should().Be(HttpStatusCode.NoContent);
        (await _client.PostAsJsonAsync($"/api/products/{productId}/images", new AssignProductImageRequest(imageId2)))
            .StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Act
        var response = await _client.PutAsync($"/api/products/{productId}/images/{imageId2}/primary", content: null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var getResponse = await _client.GetAsync($"/api/products/{productId}");
        var product = await getResponse.Content.ReadFromJsonAsync<ProductDto>();
        product!.ImageRefs.Single(r => r.ImageId == imageId2).IsPrimary.Should().BeTrue();
        product.ImageRefs.Count(r => r.IsPrimary).Should().Be(1);
    }

    [Fact]
    public async Task RemoveProductImage_WhenRemovingPrimary_PromotesRemainingToPrimary()
    {
        // Arrange
        var categoryId = await CreateCategoryAsync("Product Cat Rem", "prod-cat-rem");
        var productId = await CreateProductAsync(categoryId, "Product Remove", "product-remove");

        var imageId1 = await CreateImageAsync("rem-1.jpg");
        var imageId2 = await CreateImageAsync("rem-2.jpg");

        await _client.PostAsJsonAsync($"/api/products/{productId}/images", new AssignProductImageRequest(imageId1, SortOrder: 1));
        await _client.PostAsJsonAsync($"/api/products/{productId}/images", new AssignProductImageRequest(imageId2, SortOrder: 2));

        var before = await (await _client.GetAsync($"/api/products/{productId}")).Content.ReadFromJsonAsync<ProductDto>();
        before!.ImageRefs.Single(r => r.ImageId == imageId1).IsPrimary.Should().BeTrue();

        // Act
        var response = await _client.DeleteAsync($"/api/products/{productId}/images/{imageId1}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var after = await (await _client.GetAsync($"/api/products/{productId}")).Content.ReadFromJsonAsync<ProductDto>();
        after!.ImageRefs.Should().ContainSingle(r => r.ImageId == imageId2);
        after.ImageRefs.Single(r => r.ImageId == imageId2).IsPrimary.Should().BeTrue();
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
