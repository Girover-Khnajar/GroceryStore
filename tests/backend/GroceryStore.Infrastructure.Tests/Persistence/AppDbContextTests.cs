using FluentAssertions;
using GroceryStore.Domain.Entities;
using GroceryStore.Infrastructure.Persistence;
using GroceryStore.Infrastructure.Tests.Helpers;

namespace GroceryStore.Infrastructure.Tests.Persistence;

public class AppDbContextTests : IDisposable
{
    private readonly TestDbContext _testDb;
    private readonly AppDbContext _dbContext;

    public AppDbContextTests()
    {
        _testDb = new TestDbContext();
        _dbContext = _testDb.Context;
    }

    public void Dispose()
    {
        _testDb.Dispose();
    }

    [Fact]
    public void AppDbContext_ImplementsIUnitOfWork()
    {
        // Assert
        _dbContext.Should().BeAssignableTo<GroceryStore.Domain.Interfaces.IUnitOfWork>();
    }

    [Fact]
    public async Task SaveChangesAsync_NoChanges_ReturnsZero()
    {
        // Act
        var result = await _dbContext.SaveChangesAsync();

        // Assert
        result.Should().Be(0);
    }

    [Fact]
    public async Task SaveChangesAsync_WithNewEntity_ReturnsOne()
    {
        // Arrange
        var category = Category.Create("Fruits", "fruits");
        await _dbContext.Categories.AddAsync(category);

        // Act
        var result = await _dbContext.SaveChangesAsync();

        // Assert
        result.Should().Be(1);
    }

    [Fact]
    public async Task SaveChangesAsync_WithPendingChanges_CancellationRequested_ThrowsOperationCanceledException()
    {
        // Arrange
        var category = Category.Create("Fruits", "fruits");
        await _dbContext.Categories.AddAsync(category);
        await _dbContext.SaveChangesAsync(); // persist first

        var cts = new CancellationTokenSource();
        cts.Cancel();

        // Adding new item and saving with canceled token
        var another = Category.Create("Vegetables", "vegetables");
        await _dbContext.Categories.AddAsync(another);

        // Act & Assert
        var act = () => _dbContext.SaveChangesAsync(cts.Token);
        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public void Categories_DbSet_IsNotNull()
    {
        _dbContext.Categories.Should().NotBeNull();
    }

    [Fact]
    public void Products_DbSet_IsNotNull()
    {
        _dbContext.Products.Should().NotBeNull();
    }

    [Fact]
    public void ImageAssets_DbSet_IsNotNull()
    {
        _dbContext.ImageAssets.Should().NotBeNull();
    }
}
