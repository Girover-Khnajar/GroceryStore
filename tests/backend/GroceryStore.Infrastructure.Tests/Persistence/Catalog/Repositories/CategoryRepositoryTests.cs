using FluentAssertions;
using GroceryStore.Domain.Entities;
using GroceryStore.Domain.ValueObjects;
using GroceryStore.Infrastructure.Persistence.Catalog.Repositories;
using GroceryStore.Infrastructure.Tests.Helpers;

namespace GroceryStore.Infrastructure.Tests.Persistence.Catalog.Repositories;

public class CategoryRepositoryTests : IDisposable
{
    private readonly TestDbContext _testDb;
    private readonly GroceryStore.Infrastructure.Persistence.AppDbContext _dbContext;
    private readonly CategoryRepository _sut;

    public CategoryRepositoryTests()
    {
        _testDb = new TestDbContext();
        _dbContext = _testDb.Context;
        _sut = new CategoryRepository(_dbContext);
    }

    public void Dispose()
    {
        _testDb.Dispose();
    }

    // ─── Helpers ───

    private static Category CreateCategory(
        string name = "Fruits",
        string slug = "fruits",
        int sortOrder = 0,
        Guid? parentCategoryId = null)
        => Category.Create(name, slug, sortOrder, parentCategoryId);

    private async Task<Category> SeedCategoryAsync(
        string name = "Fruits",
        string slug = "fruits",
        int sortOrder = 0,
        Guid? parentCategoryId = null)
    {
        var category = CreateCategory(name, slug, sortOrder, parentCategoryId);
        await _sut.AddAsync(category);
        await _dbContext.SaveChangesAsync();
        return category;
    }

    // ═══════════════════════════════════════════ AddAsync

    [Fact]
    public async Task AddAsync_ValidCategory_PersistsToDatabase()
    {
        // Arrange
        var category = CreateCategory();

        // Act
        await _sut.AddAsync(category);
        await _dbContext.SaveChangesAsync();

        // Assert
        var found = await _dbContext.Categories.FindAsync(category.Id);
        found.Should().NotBeNull();
        found!.Name.Should().Be("Fruits");
    }

    // ═══════════════════════════════════════════ GetByIdAsync

    [Fact]
    public async Task GetByIdAsync_ExistingId_ReturnsCategory()
    {
        // Arrange
        var category = await SeedCategoryAsync();

        // Act
        var result = await _sut.GetByIdAsync(category.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(category.Id);
        result.Name.Should().Be("Fruits");
    }

    [Fact]
    public async Task GetByIdAsync_NonExistingId_ReturnsNull()
    {
        // Act
        var result = await _sut.GetByIdAsync(Guid.NewGuid());

        // Assert
        result.Should().BeNull();
    }

    // ═══════════════════════════════════════════ GetBySlugAsync

    [Fact]
    public async Task GetBySlugAsync_ExistingSlug_ReturnsCategory()
    {
        // Arrange
        await SeedCategoryAsync(name: "Dairy Products", slug: "dairy-products");

        // Act
        var result = await _sut.GetBySlugAsync("dairy-products");

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Dairy Products");
    }

    [Fact]
    public async Task GetBySlugAsync_NonExistingSlug_ReturnsNull()
    {
        // Act
        var result = await _sut.GetBySlugAsync("non-existing");

        // Assert
        result.Should().BeNull();
    }

    // ═══════════════════════════════════════════ GetByParentIdAsync

    [Fact]
    public async Task GetByParentIdAsync_WithChildren_ReturnsChildren()
    {
        // Arrange
        var parent = await SeedCategoryAsync(name: "Food", slug: "food");
        await SeedCategoryAsync(name: "Fruits", slug: "fruits", parentCategoryId: parent.Id);
        await SeedCategoryAsync(name: "Vegetables", slug: "vegetables", parentCategoryId: parent.Id);

        // Act
        var result = await _sut.GetByParentIdAsync(parent.Id);

        // Assert
        result.Should().HaveCount(2);
        result.Select(c => c.Name).Should().Contain(new[] { "Fruits", "Vegetables" });
    }

    [Fact]
    public async Task GetByParentIdAsync_RootCategories_ReturnsRootOnly()
    {
        // Arrange
        var root1 = await SeedCategoryAsync(name: "Food", slug: "food");
        var root2 = await SeedCategoryAsync(name: "Drinks", slug: "drinks");
        await SeedCategoryAsync(name: "Fruits", slug: "fruits", parentCategoryId: root1.Id);

        // Act
        var result = await _sut.GetByParentIdAsync(null);

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetByParentIdAsync_NoChildren_ReturnsEmpty()
    {
        // Arrange
        var parent = await SeedCategoryAsync();

        // Act
        var result = await _sut.GetByParentIdAsync(parent.Id);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetByParentIdAsync_ReturnsOrderedBySortOrder()
    {
        // Arrange
        var parent = await SeedCategoryAsync(name: "Food", slug: "food");
        await SeedCategoryAsync(name: "Zucchini", slug: "zucchini", sortOrder: 2, parentCategoryId: parent.Id);
        await SeedCategoryAsync(name: "Apples", slug: "apples", sortOrder: 1, parentCategoryId: parent.Id);

        // Act
        var result = await _sut.GetByParentIdAsync(parent.Id);

        // Assert
        result.Should().HaveCount(2);
        result[0].Name.Should().Be("Apples");
        result[1].Name.Should().Be("Zucchini");
    }

    // ═══════════════════════════════════════════ GetAllActiveAsync

    [Fact]
    public async Task GetAllActiveAsync_ReturnsOnlyActiveCategories()
    {
        // Arrange
        var active = await SeedCategoryAsync(name: "Active", slug: "active");
        var inactive = await SeedCategoryAsync(name: "Inactive", slug: "inactive");
        inactive.Deactivate();
        _sut.Update(inactive);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _sut.GetAllActiveAsync();

        // Assert
        result.Should().HaveCount(1);
        result[0].Name.Should().Be("Active");
    }

    [Fact]
    public async Task GetAllActiveAsync_ReturnsOrderedBySortOrder()
    {
        // Arrange
        await SeedCategoryAsync(name: "B", slug: "b-cat", sortOrder: 2);
        await SeedCategoryAsync(name: "A", slug: "a-cat", sortOrder: 1);

        // Act
        var result = await _sut.GetAllActiveAsync();

        // Assert
        result[0].Name.Should().Be("A");
        result[1].Name.Should().Be("B");
    }

    // ═══════════════════════════════════════════ Update

    [Fact]
    public async Task Update_ModifiesCategory_PersistsChanges()
    {
        // Arrange
        var category = await SeedCategoryAsync();
        category.Rename("Updated Fruits");

        // Act
        _sut.Update(category);
        await _dbContext.SaveChangesAsync();

        // Assert
        var updated = await _sut.GetByIdAsync(category.Id);
        updated!.Name.Should().Be("Updated Fruits");
    }

    // ═══════════════════════════════════════════ Remove

    [Fact]
    public async Task Remove_ExistingCategory_RemovesFromDatabase()
    {
        // Arrange
        var category = await SeedCategoryAsync();

        // Act
        _sut.Remove(category);
        await _dbContext.SaveChangesAsync();

        // Assert
        var found = await _sut.GetByIdAsync(category.Id);
        found.Should().BeNull();
    }

    // ═══════════════════════════════════════════ ExistsAsync

    [Fact]
    public async Task ExistsAsync_ExistingId_ReturnsTrue()
    {
        // Arrange
        var category = await SeedCategoryAsync();

        // Act
        var exists = await _sut.ExistsAsync(category.Id);

        // Assert
        exists.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsAsync_NonExistingId_ReturnsFalse()
    {
        // Act
        var exists = await _sut.ExistsAsync(Guid.NewGuid());

        // Assert
        exists.Should().BeFalse();
    }

    // ═══════════════════════════════════════════ CancellationToken

    [Fact]
    public async Task GetByIdAsync_CancellationRequested_ThrowsOperationCanceledException()
    {
        // Arrange
        var category = await SeedCategoryAsync();
        var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act & Assert
        var act = () => _sut.GetByIdAsync(category.Id, cts.Token);
        await act.Should().ThrowAsync<OperationCanceledException>();
    }
}
