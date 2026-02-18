using FluentAssertions;
using GroceryStore.Domain.Entities.Media;
using GroceryStore.Domain.ValueObjects;
using GroceryStore.Infrastructure.Persistence.Media.Repositories;
using GroceryStore.Infrastructure.Tests.Helpers;

namespace GroceryStore.Infrastructure.Tests.Persistence.Media.Repositories;

public class ImageAssetRepositoryTests : IDisposable
{
    private readonly TestDbContext _testDb;
    private readonly GroceryStore.Infrastructure.Persistence.AppDbContext _dbContext;
    private readonly ImageAssetRepository _sut;

    public ImageAssetRepositoryTests()
    {
        _testDb = new TestDbContext();
        _dbContext = _testDb.Context;
        _sut = new ImageAssetRepository(_dbContext);
    }

    public void Dispose()
    {
        _testDb.Dispose();
    }

    // ─── Helpers ───

    private static ImageMetadata CreateMetadata(
        string fileName = "photo.jpg",
        string contentType = "image/jpeg",
        long fileSize = 50_000,
        int? width = 800,
        int? height = 600)
        => ImageMetadata.Create(fileName, contentType, fileSize, width, height);

    private static ImageAsset CreateImageAsset(
        string storagePath = "images/2024/photo.jpg",
        string url = "https://cdn.example.com/photo.jpg",
        string? altText = "A photo")
        => ImageAsset.Create(storagePath, url, CreateMetadata(), altText);

    private async Task<ImageAsset> SeedImageAssetAsync(
        string storagePath = "images/2024/photo.jpg",
        string url = "https://cdn.example.com/photo.jpg",
        string? altText = "A photo")
    {
        var asset = CreateImageAsset(storagePath, url, altText);
        await _sut.AddAsync(asset);
        await _dbContext.SaveChangesAsync();
        return asset;
    }

    // ═══════════════════════════════════════════ AddAsync

    [Fact]
    public async Task AddAsync_ValidImageAsset_PersistsToDatabase()
    {
        // Arrange
        var asset = CreateImageAsset();

        // Act
        await _sut.AddAsync(asset);
        await _dbContext.SaveChangesAsync();

        // Assert
        var found = await _dbContext.ImageAssets.FindAsync(asset.Id);
        found.Should().NotBeNull();
        found!.StoragePath.Should().Be("images/2024/photo.jpg");
    }

    // ═══════════════════════════════════════════ GetByIdAsync

    [Fact]
    public async Task GetByIdAsync_ExistingId_ReturnsImageAsset()
    {
        // Arrange
        var asset = await SeedImageAssetAsync();
        _dbContext.ChangeTracker.Clear();

        // Act
        var result = await _sut.GetByIdAsync(asset.ImageId);

        // Assert
        result.Should().NotBeNull();
        result!.ImageId.Should().Be(asset.ImageId);
        result.Url.Should().Be("https://cdn.example.com/photo.jpg");
    }

    [Fact]
    public async Task GetByIdAsync_NonExistingId_ReturnsNull()
    {
        // Act
        var result = await _sut.GetByIdAsync(ImageId.CreateNew());

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdAsync_SoftDeletedAsset_ReturnsNull()
    {
        // Arrange
        var asset = await SeedImageAssetAsync();
        asset.MarkDeleted();
        _sut.Update(asset);
        await _dbContext.SaveChangesAsync();
        _dbContext.ChangeTracker.Clear();

        // Act
        var result = await _sut.GetByIdAsync(asset.ImageId);

        // Assert
        result.Should().BeNull();
    }

    // ═══════════════════════════════════════════ GetByIdsAsync

    [Fact]
    public async Task GetByIdsAsync_MultipleIds_ReturnsAll()
    {
        // Arrange
        var asset1 = await SeedImageAssetAsync("images/1.jpg", "https://cdn.example.com/1.jpg", "one");
        var asset2 = await SeedImageAssetAsync("images/2.jpg", "https://cdn.example.com/2.jpg", "two");
        await SeedImageAssetAsync("images/3.jpg", "https://cdn.example.com/3.jpg", "three");
        _dbContext.ChangeTracker.Clear();

        // Act
        var result = await _sut.GetByIdsAsync([asset1.ImageId, asset2.ImageId]);

        // Assert
        result.Should().HaveCount(2);
        result.Select(a => a.AltText).Should().Contain(new[] { "one", "two" });
    }

    [Fact]
    public async Task GetByIdsAsync_EmptyList_ReturnsEmpty()
    {
        // Act
        var result = await _sut.GetByIdsAsync([]);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetByIdsAsync_NonExistingIds_ReturnsEmpty()
    {
        // Act
        var result = await _sut.GetByIdsAsync([ImageId.CreateNew(), ImageId.CreateNew()]);

        // Assert
        result.Should().BeEmpty();
    }

    // ═══════════════════════════════════════════ Update

    [Fact]
    public async Task Update_ModifiesAltText_PersistsChanges()
    {
        // Arrange
        var asset = await SeedImageAssetAsync();
        asset.ChangeAltText("Updated alt text");

        // Act
        _sut.Update(asset);
        await _dbContext.SaveChangesAsync();
        _dbContext.ChangeTracker.Clear();

        // Assert
        var updated = await _sut.GetByIdAsync(asset.ImageId);
        updated!.AltText.Should().Be("Updated alt text");
    }

    // ═══════════════════════════════════════════ Remove

    [Fact]
    public async Task Remove_ExistingAsset_RemovesFromDatabase()
    {
        // Arrange
        var asset = await SeedImageAssetAsync();

        // Act
        _sut.Remove(asset);
        await _dbContext.SaveChangesAsync();

        // Assert — query ignoring global filter since it's physically removed
        var found = await _dbContext.ImageAssets.FindAsync(asset.Id);
        found.Should().BeNull();
    }

    // ═══════════════════════════════════════════ ExistsAsync

    [Fact]
    public async Task ExistsAsync_ExistingId_ReturnsTrue()
    {
        // Arrange
        var asset = await SeedImageAssetAsync();

        // Act
        var exists = await _sut.ExistsAsync(asset.ImageId);

        // Assert
        exists.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsAsync_NonExistingId_ReturnsFalse()
    {
        // Act
        var exists = await _sut.ExistsAsync(ImageId.CreateNew());

        // Assert
        exists.Should().BeFalse();
    }

    [Fact]
    public async Task ExistsAsync_SoftDeletedAsset_ReturnsFalse()
    {
        // Arrange
        var asset = await SeedImageAssetAsync();
        asset.MarkDeleted();
        _sut.Update(asset);
        await _dbContext.SaveChangesAsync();
        _dbContext.ChangeTracker.Clear();

        // Act
        var exists = await _sut.ExistsAsync(asset.ImageId);

        // Assert
        exists.Should().BeFalse();
    }

    // ═══════════════════════════════════════════ Metadata persistence

    [Fact]
    public async Task Persistence_ImageMetadata_RoundTrips()
    {
        // Arrange
        var asset = CreateImageAsset();
        await _sut.AddAsync(asset);
        await _dbContext.SaveChangesAsync();
        _dbContext.ChangeTracker.Clear();

        // Act
        var result = await _sut.GetByIdAsync(asset.ImageId);

        // Assert
        result.Should().NotBeNull();
        result!.Metadata.OriginalFileName.Should().Be("photo.jpg");
        result.Metadata.ContentType.Should().Be("image/jpeg");
        result.Metadata.FileSizeBytes.Should().Be(50_000);
        result.Metadata.WidthPx.Should().Be(800);
        result.Metadata.HeightPx.Should().Be(600);
    }

    // ═══════════════════════════════════════════ CancellationToken

    [Fact]
    public async Task GetByIdAsync_CancellationRequested_ThrowsOperationCanceledException()
    {
        // Arrange
        var asset = await SeedImageAssetAsync();
        var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act & Assert
        var act = () => _sut.GetByIdAsync(asset.ImageId, cts.Token);
        await act.Should().ThrowAsync<OperationCanceledException>();
    }
}
