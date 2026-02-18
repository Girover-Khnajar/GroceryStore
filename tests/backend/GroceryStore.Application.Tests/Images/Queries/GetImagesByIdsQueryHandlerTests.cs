using GroceryStore.Application.Images.Queries;
using GroceryStore.Domain.Entities.Media;
using GroceryStore.Domain.Interfaces;
using GroceryStore.Domain.ValueObjects;

namespace GroceryStore.Application.Tests.Images.Queries;

public class GetImagesByIdsHandlerTests
{
    private readonly Mock<IImageAssetRepository> _imageRepo = new();
    private readonly GetImagesByIdsQueryHandler _handler;

    public GetImagesByIdsHandlerTests()
    {
        _handler = new GetImagesByIdsQueryHandler(_imageRepo.Object);
    }

    private static ImageAsset CreateAsset(string fileName = "photo.jpg")
    {
        var metadata = ImageMetadata.Create(fileName, "image/jpeg", 1024, 800, 600);
        return ImageAsset.Create($"images/{fileName}", $"https://cdn.test/{fileName}", metadata);
    }

    [Fact]
    public async Task HandleAsync_MultipleIds_ReturnsMatchingDtos()
    {
        // Arrange
        var asset1 = CreateAsset("a.jpg");
        var asset2 = CreateAsset("b.jpg");
        var ids = new List<Guid> { asset1.ImageId.Value, asset2.ImageId.Value };

        _imageRepo.Setup(r => r.GetByIdsAsync(It.IsAny<IEnumerable<ImageId>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ImageAsset> { asset1, asset2 });

        // Act
        var result = await _handler.HandleAsync(new GetImagesByIdsQuery(ids));

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
    }

    [Fact]
    public async Task HandleAsync_EmptyIds_ReturnsEmptyList()
    {
        // Arrange
        _imageRepo.Setup(r => r.GetByIdsAsync(It.IsAny<IEnumerable<ImageId>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ImageAsset>());

        // Act
        var result = await _handler.HandleAsync(new GetImagesByIdsQuery(new List<Guid>()));

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task HandleAsync_SomeNotFound_ReturnsOnlyExisting()
    {
        // Arrange
        var asset = CreateAsset();
        var ids = new List<Guid> { asset.ImageId.Value, Guid.NewGuid() };

        _imageRepo.Setup(r => r.GetByIdsAsync(It.IsAny<IEnumerable<ImageId>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ImageAsset> { asset });

        // Act
        var result = await _handler.HandleAsync(new GetImagesByIdsQuery(ids));

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
    }
}
