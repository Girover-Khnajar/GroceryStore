using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using GroceryStore.Api.Contracts.Categories;
using GroceryStore.Application.Categories.Dtos;

namespace GroceryStore.Api.Tests.Endpoints;

public class CategoryEndpointsTests : IClassFixture<GroceryStoreApiFactory>
{
    private readonly HttpClient _client;

    public CategoryEndpointsTests(GroceryStoreApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    #region GET /api/categories

    [Fact]
    public async Task GetAllActiveCategories_WhenNoCategoriesExist_ReturnsEmptyList()
    {
        // Act
        var response = await _client.GetAsync("/api/categories");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var categories = await response.Content.ReadFromJsonAsync<List<CategoryDto>>();
        categories.Should().NotBeNull();
    }

    [Fact]
    public async Task GetAllActiveCategories_WhenCategoriesExist_ReturnsActiveCategories()
    {
        // Arrange
        var request = new CreateCategoryRequest("Active Category", "active-category-getall");
        var createResponse = await _client.PostAsJsonAsync("/api/categories", request);
        createResponse.EnsureSuccessStatusCode();

        // Act
        var response = await _client.GetAsync("/api/categories");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var categories = await response.Content.ReadFromJsonAsync<List<CategoryDto>>();
        categories.Should().NotBeNull();
        categories.Should().Contain(c => c.Slug == "active-category-getall");
    }

    #endregion

    #region GET /api/categories/{id}

    [Fact]
    public async Task GetCategoryById_WhenCategoryExists_ReturnsCategory()
    {
        // Arrange
        var request = new CreateCategoryRequest("By Id Category", "by-id-category");
        var createResponse = await _client.PostAsJsonAsync("/api/categories", request);
        createResponse.EnsureSuccessStatusCode();
        var created = await createResponse.Content.ReadFromJsonAsync<IdResponse>();

        // Act
        var response = await _client.GetAsync($"/api/categories/{created!.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var category = await response.Content.ReadFromJsonAsync<CategoryDto>();
        category.Should().NotBeNull();
        category!.Name.Should().Be("By Id Category");
        category.Slug.Should().Be("by-id-category");
    }

    [Fact]
    public async Task GetCategoryById_WhenCategoryDoesNotExist_ReturnsNotFound()
    {
        // Act
        var response = await _client.GetAsync($"/api/categories/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region GET /api/categories/by-slug/{slug}

    [Fact]
    public async Task GetCategoryBySlug_WhenCategoryExists_ReturnsCategory()
    {
        // Arrange
        var request = new CreateCategoryRequest("Slug Category", "slug-category-test");
        var createResponse = await _client.PostAsJsonAsync("/api/categories", request);
        createResponse.EnsureSuccessStatusCode();

        // Act
        var response = await _client.GetAsync("/api/categories/by-slug/slug-category-test");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var category = await response.Content.ReadFromJsonAsync<CategoryDto>();
        category.Should().NotBeNull();
        category!.Slug.Should().Be("slug-category-test");
    }

    [Fact]
    public async Task GetCategoryBySlug_WhenCategoryDoesNotExist_ReturnsNotFound()
    {
        // Act
        var response = await _client.GetAsync("/api/categories/by-slug/non-existent-slug");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region GET /api/categories/by-parent/{parentId}

    [Fact]
    public async Task GetCategoriesByParentId_WhenChildrenExist_ReturnsChildren()
    {
        // Arrange – Create parent
        var parentRequest = new CreateCategoryRequest("Parent Cat", "parent-cat-test");
        var parentResponse = await _client.PostAsJsonAsync("/api/categories", parentRequest);
        parentResponse.EnsureSuccessStatusCode();
        var parentCreated = await parentResponse.Content.ReadFromJsonAsync<IdResponse>();

        // Create child
        var childRequest = new CreateCategoryRequest(
            "Child Cat", "child-cat-test", ParentCategoryId: parentCreated!.Id);
        var childResponse = await _client.PostAsJsonAsync("/api/categories", childRequest);
        childResponse.EnsureSuccessStatusCode();

        // Act
        var response = await _client.GetAsync($"/api/categories/by-parent/{parentCreated.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var children = await response.Content.ReadFromJsonAsync<List<CategoryDto>>();
        children.Should().NotBeNull();
        children.Should().Contain(c => c.Slug == "child-cat-test");
    }

    [Fact]
    public async Task GetCategoriesByParentId_WhenNoChildren_ReturnsEmptyList()
    {
        // Act
        var response = await _client.GetAsync($"/api/categories/by-parent/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var children = await response.Content.ReadFromJsonAsync<List<CategoryDto>>();
        children.Should().NotBeNull();
        children.Should().BeEmpty();
    }

    #endregion

    #region POST /api/categories

    [Fact]
    public async Task CreateCategory_WithValidRequest_ReturnsCreatedWithId()
    {
        // Arrange
        var request = new CreateCategoryRequest(
            "New Category",
            "new-category-create",
            SortOrder: 5,
            Description: "A test category",
            SeoMetaTitle: "Seo Title",
            SeoMetaDescription: "Seo Description");

        // Act
        var response = await _client.PostAsJsonAsync("/api/categories", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Headers.Location.Should().NotBeNull();
        var created = await response.Content.ReadFromJsonAsync<IdResponse>();
        created.Should().NotBeNull();
        created!.Id.Should().NotBeEmpty();
    }

    [Fact]
    public async Task CreateCategory_WithDuplicateSlug_ReturnsConflict()
    {
        // Arrange
        var request = new CreateCategoryRequest("First Category", "duplicate-slug-test");
        var firstResponse = await _client.PostAsJsonAsync("/api/categories", request);
        firstResponse.EnsureSuccessStatusCode();

        var duplicateRequest = new CreateCategoryRequest("Second Category", "duplicate-slug-test");

        // Act
        var response = await _client.PostAsJsonAsync("/api/categories", duplicateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task CreateCategory_WithParentCategory_ReturnsCreated()
    {
        // Arrange – Create parent
        var parentRequest = new CreateCategoryRequest("Parent for Create", "parent-for-create-test");
        var parentResponse = await _client.PostAsJsonAsync("/api/categories", parentRequest);
        parentResponse.EnsureSuccessStatusCode();
        var parentCreated = await parentResponse.Content.ReadFromJsonAsync<IdResponse>();

        var childRequest = new CreateCategoryRequest(
            "Child for Create", "child-for-create-test",
            ParentCategoryId: parentCreated!.Id);

        // Act
        var response = await _client.PostAsJsonAsync("/api/categories", childRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    #endregion

    #region PUT /api/categories/{id}

    [Fact]
    public async Task UpdateCategory_WithValidRequest_ReturnsNoContent()
    {
        // Arrange
        var createRequest = new CreateCategoryRequest("Update Me", "update-me-cat");
        var createResponse = await _client.PostAsJsonAsync("/api/categories", createRequest);
        createResponse.EnsureSuccessStatusCode();
        var created = await createResponse.Content.ReadFromJsonAsync<IdResponse>();

        var updateRequest = new UpdateCategoryRequest(
            "Updated Name", "update-me-cat", 10, null,
            "Updated desc", null, null, null, null);

        // Act
        var response = await _client.PutAsJsonAsync($"/api/categories/{created!.Id}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify the update
        var getResponse = await _client.GetAsync($"/api/categories/{created.Id}");
        var category = await getResponse.Content.ReadFromJsonAsync<CategoryDto>();
        category!.Name.Should().Be("Updated Name");
        category.SortOrder.Should().Be(10);
    }

    [Fact]
    public async Task UpdateCategory_WhenCategoryDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        var updateRequest = new UpdateCategoryRequest(
            "Updated Name", "non-existent-cat", 0, null,
            null, null, null, null, null);

        // Act
        var response = await _client.PutAsJsonAsync($"/api/categories/{Guid.NewGuid()}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region DELETE /api/categories/{id}

    [Fact]
    public async Task DeleteCategory_WhenCategoryExists_ReturnsNoContent()
    {
        // Arrange
        var createRequest = new CreateCategoryRequest("Delete Me", "delete-me-cat");
        var createResponse = await _client.PostAsJsonAsync("/api/categories", createRequest);
        createResponse.EnsureSuccessStatusCode();
        var created = await createResponse.Content.ReadFromJsonAsync<IdResponse>();

        // Act
        var response = await _client.DeleteAsync($"/api/categories/{created!.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task DeleteCategory_WhenCategoryDoesNotExist_ReturnsNotFound()
    {
        // Act
        var response = await _client.DeleteAsync($"/api/categories/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    private sealed record IdResponse(Guid Id);
}
