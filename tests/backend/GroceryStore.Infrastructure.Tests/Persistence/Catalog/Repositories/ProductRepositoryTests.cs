using FluentAssertions;
using GroceryStore.Domain.Entities;
using GroceryStore.Domain.Enums;
using GroceryStore.Domain.ValueObjects;
using GroceryStore.Infrastructure.Persistence.Catalog.Repositories;
using GroceryStore.Infrastructure.Tests.Helpers;

namespace GroceryStore.Infrastructure.Tests.Persistence.Catalog.Repositories;

public class ProductRepositoryTests : IDisposable
{
    private readonly TestDbContext _testDb;
    private readonly GroceryStore.Infrastructure.Persistence.AppDbContext _dbContext;
    private readonly ProductRepository _sut;
    private readonly CategoryRepository _categoryRepo;

    public ProductRepositoryTests()
    {
        _testDb = new TestDbContext();
        _dbContext = _testDb.Context;
        _sut = new ProductRepository(_dbContext);
        _categoryRepo = new CategoryRepository(_dbContext);
    }

    public void Dispose()
    {
        _testDb.Dispose();
    }

    // ─── Helpers ───

    private async Task<Category> SeedCategoryAsync(string name = "Fruits", string slug = "fruits")
    {
        var category = Category.Create(name, slug);
        await _categoryRepo.AddAsync(category);
        await _dbContext.SaveChangesAsync();
        return category;
    }

    private static Product CreateProduct(
        Guid categoryId,
        string name = "Apple",
        string slug = "apple",
        decimal priceAmount = 2.50m,
        string currency = "CHF",
        ProductUnit unit = ProductUnit.Piece,
        int sortOrder = 0)
        => Product.Create(
            categoryId,
            name,
            slug,
            Money.Create(priceAmount, currency),
            unit,
            sortOrder);

    private async Task<Product> SeedProductAsync(
        Guid categoryId,
        string name = "Apple",
        string slug = "apple",
        decimal priceAmount = 2.50m,
        int sortOrder = 0)
    {
        var product = CreateProduct(categoryId, name, slug, priceAmount, sortOrder: sortOrder);
        await _sut.AddAsync(product);
        await _dbContext.SaveChangesAsync();
        return product;
    }

    // ═══════════════════════════════════════════ AddAsync

    [Fact]
    public async Task AddAsync_ValidProduct_PersistsToDatabase()
    {
        // Arrange
        var category = await SeedCategoryAsync();
        var product = CreateProduct(category.Id);

        // Act
        await _sut.AddAsync(product);
        await _dbContext.SaveChangesAsync();

        // Assert
        var found = await _dbContext.Products.FindAsync(product.Id);
        found.Should().NotBeNull();
        found!.Name.Should().Be("Apple");
    }

    // ═══════════════════════════════════════════ GetByIdAsync

    [Fact]
    public async Task GetByIdAsync_ExistingId_ReturnsProduct()
    {
        // Arrange
        var category = await SeedCategoryAsync();
        var product = await SeedProductAsync(category.Id);

        // Act
        var result = await _sut.GetByIdAsync(product.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(product.Id);
        result.Name.Should().Be("Apple");
        result.CategoryId.Should().Be(category.Id);
    }

    [Fact]
    public async Task GetByIdAsync_NonExistingId_ReturnsNull()
    {
        // Act
        var result = await _sut.GetByIdAsync(Guid.NewGuid());

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdAsync_IncludesImageRefs()
    {
        // Arrange
        var category = await SeedCategoryAsync();
        var product = CreateProduct(category.Id);
        product.AttachImage(ImageId.CreateNew(), makePrimary: true, altText: "Main image");
        await _sut.AddAsync(product);
        await _dbContext.SaveChangesAsync();

        // Detach so EF re-fetches from the database
        _dbContext.ChangeTracker.Clear();

        // Act
        var result = await _sut.GetByIdAsync(product.Id);

        // Assert
        result.Should().NotBeNull();
        result!.ImageRefs.Should().HaveCount(1);
        result.ImageRefs.First().IsPrimary.Should().BeTrue();
    }

    // ═══════════════════════════════════════════ GetBySlugAsync

    [Fact]
    public async Task GetBySlugAsync_ExistingSlug_ReturnsProduct()
    {
        // Arrange
        var category = await SeedCategoryAsync();
        await SeedProductAsync(category.Id, name: "Organic Apple", slug: "organic-apple");

        // Act
        var result = await _sut.GetBySlugAsync("organic-apple");

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Organic Apple");
    }

    [Fact]
    public async Task GetBySlugAsync_NonExistingSlug_ReturnsNull()
    {
        // Act
        var result = await _sut.GetBySlugAsync("non-existing");

        // Assert
        result.Should().BeNull();
    }

    // ═══════════════════════════════════════════ GetByCategoryIdAsync

    [Fact]
    public async Task GetByCategoryIdAsync_ReturnsProductsInCategory()
    {
        // Arrange
        var cat1 = await SeedCategoryAsync(name: "Fruits", slug: "fruits");
        var cat2 = await SeedCategoryAsync(name: "Drinks", slug: "drinks");
        await SeedProductAsync(cat1.Id, name: "Apple", slug: "apple");
        await SeedProductAsync(cat1.Id, name: "Banana", slug: "banana");
        await SeedProductAsync(cat2.Id, name: "Juice", slug: "juice");

        // Act
        var result = await _sut.GetByCategoryIdAsync(cat1.Id);

        // Assert
        result.Should().HaveCount(2);
        result.Select(p => p.Name).Should().Contain(new[] { "Apple", "Banana" });
    }

    [Fact]
    public async Task GetByCategoryIdAsync_NoProducts_ReturnsEmpty()
    {
        // Arrange
        var category = await SeedCategoryAsync();

        // Act
        var result = await _sut.GetByCategoryIdAsync(category.Id);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetByCategoryIdAsync_ReturnsOrderedBySortOrder()
    {
        // Arrange
        var category = await SeedCategoryAsync();
        await SeedProductAsync(category.Id, name: "Banana", slug: "banana", sortOrder: 2);
        await SeedProductAsync(category.Id, name: "Apple", slug: "apple", sortOrder: 1);

        // Act
        var result = await _sut.GetByCategoryIdAsync(category.Id);

        // Assert
        result[0].Name.Should().Be("Apple");
        result[1].Name.Should().Be("Banana");
    }

    // ═══════════════════════════════════════════ Update

    [Fact]
    public async Task Update_ModifiesProduct_PersistsChanges()
    {
        // Arrange
        var category = await SeedCategoryAsync();
        var product = await SeedProductAsync(category.Id);
        product.Rename("Red Apple");

        // Act
        _sut.Update(product);
        await _dbContext.SaveChangesAsync();

        // Assert
        var updated = await _sut.GetByIdAsync(product.Id);
        updated!.Name.Should().Be("Red Apple");
    }

    // ═══════════════════════════════════════════ Remove

    [Fact]
    public async Task Remove_ExistingProduct_RemovesFromDatabase()
    {
        // Arrange
        var category = await SeedCategoryAsync();
        var product = await SeedProductAsync(category.Id);

        // Act
        _sut.Remove(product);
        await _dbContext.SaveChangesAsync();

        // Assert
        var found = await _sut.GetByIdAsync(product.Id);
        found.Should().BeNull();
    }

    // ═══════════════════════════════════════════ Value Object Persistence

    [Fact]
    public async Task Persistence_PriceValueObject_RoundTrips()
    {
        // Arrange
        var category = await SeedCategoryAsync();
        var product = CreateProduct(category.Id, priceAmount: 12.99m, currency: "EUR");
        await _sut.AddAsync(product);
        await _dbContext.SaveChangesAsync();
        _dbContext.ChangeTracker.Clear();

        // Act
        var result = await _sut.GetByIdAsync(product.Id);

        // Assert
        result!.Price.Amount.Should().Be(12.99m);
        result.Price.Currency.Should().Be("EUR");
    }

    [Fact]
    public async Task Persistence_SlugValueObject_RoundTrips()
    {
        // Arrange
        var category = await SeedCategoryAsync();
        var product = CreateProduct(category.Id, slug: "green-apple");
        await _sut.AddAsync(product);
        await _dbContext.SaveChangesAsync();
        _dbContext.ChangeTracker.Clear();

        // Act
        var result = await _sut.GetByIdAsync(product.Id);

        // Assert
        result!.Slug.Value.Should().Be("green-apple");
    }

    // ═══════════════════════════════════════════ CancellationToken

    [Fact]
    public async Task GetByIdAsync_CancellationRequested_ThrowsOperationCanceledException()
    {
        // Arrange
        var category = await SeedCategoryAsync();
        var product = await SeedProductAsync(category.Id);
        var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act & Assert
        var act = () => _sut.GetByIdAsync(product.Id, cts.Token);
        await act.Should().ThrowAsync<OperationCanceledException>();
    }
}
