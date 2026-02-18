using CQRS.CqrsResult;
using GroceryStore.Application.Images.Commands;
using GroceryStore.Domain.Entities.Media;
using GroceryStore.Domain.Interfaces;
using GroceryStore.Domain.ValueObjects;

namespace GroceryStore.Application.Tests.Images.Commands;

public class UpdateImageAltTextHandlerTests
{
    private readonly Mock<IImageAssetRepository> _imageRepo = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly UpdateImageAltTextCommandHandler _handler;

    public UpdateImageAltTextHandlerTests()
    {
        _handler = new UpdateImageAltTextCommandHandler(_imageRepo.Object, _unitOfWork.Object);
    }

    private static ImageAsset CreateAsset()
    {
        var metadata = ImageMetadata.Create("photo.jpg", "image/jpeg", 1024, 800, 600);
        return ImageAsset.Create("images/photo.jpg", "https://cdn.test/photo.jpg", metadata, "Old alt");
    }

    [Fact]
    public async Task HandleAsync_ExistingAsset_ReturnsSuccess()
    {
        // Arrange
        var asset = CreateAsset();
        _imageRepo.Setup(r => r.GetByIdAsync(It.IsAny<ImageId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(asset);

        // Act
        var result = await _handler.HandleAsync(new UpdateImageAltTextCommand(asset.ImageId.Value, "New alt text"));

        // Assert
        result.IsSuccess.Should().BeTrue();
        asset.AltText.Should().Be("New alt text");
        _imageRepo.Verify(r => r.Update(asset), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_AssetNotFound_ReturnsNotFound()
    {
        // Arrange
        var id = Guid.NewGuid();
        _imageRepo.Setup(r => r.GetByIdAsync(It.IsAny<ImageId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ImageAsset?)null);

        // Act
        var result = await _handler.HandleAsync(new UpdateImageAltTextCommand(id, "New alt"));

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainSingle()
            .Which.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task HandleAsync_NullAltText_ClearsAltText()
    {
        // Arrange
        var asset = CreateAsset();
        _imageRepo.Setup(r => r.GetByIdAsync(It.IsAny<ImageId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(asset);

        // Act
        var result = await _handler.HandleAsync(new UpdateImageAltTextCommand(asset.ImageId.Value, null));

        // Assert
        result.IsSuccess.Should().BeTrue();
        asset.AltText.Should().BeNull();
    }
}
