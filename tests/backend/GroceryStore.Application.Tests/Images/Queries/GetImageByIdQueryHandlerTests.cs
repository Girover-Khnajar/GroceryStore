using CQRS.CqrsResult;
using GroceryStore.Application.Images.Dtos;
using GroceryStore.Application.Images.Queries;
using GroceryStore.Domain.Entities.Media;
using GroceryStore.Domain.Interfaces;
using GroceryStore.Domain.ValueObjects;

namespace GroceryStore.Application.Tests.Images.Queries;

public class GetImageByIdHandlerTests
{
    private readonly Mock<IImageAssetRepository> _imageRepo = new();
    private readonly GetImageByIdQueryHandler _handler;

    public GetImageByIdHandlerTests()
    {
        _handler = new GetImageByIdQueryHandler(_imageRepo.Object);
    }

    private static ImageAsset CreateAsset()
    {
        var metadata = ImageMetadata.Create("photo.jpg", "image/jpeg", 1024, 800, 600);
        return ImageAsset.Create("images/photo.jpg", "https://cdn.test/photo.jpg", metadata, "Alt text");
    }

    [Fact]
    public async Task HandleAsync_ExistingImage_ReturnsDto()
    {
        // Arrange
        var asset = CreateAsset();
        _imageRepo.Setup(r => r.GetByIdAsync(It.IsAny<ImageId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(asset);

        // Act
        var result = await _handler.HandleAsync(new GetImageByIdQuery(asset.ImageId.Value));

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.ImageId.Should().Be(asset.ImageId.Value);
        result.Value.StoragePath.Should().Be("images/photo.jpg");
        result.Value.AltText.Should().Be("Alt text");
    }

    [Fact]
    public async Task HandleAsync_NonExistingImage_ReturnsNotFound()
    {
        // Arrange
        var id = Guid.NewGuid();
        _imageRepo.Setup(r => r.GetByIdAsync(It.IsAny<ImageId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ImageAsset?)null);

        // Act
        var result = await _handler.HandleAsync(new GetImageByIdQuery(id));

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainSingle()
            .Which.Type.Should().Be(ErrorType.NotFound);
    }
}
