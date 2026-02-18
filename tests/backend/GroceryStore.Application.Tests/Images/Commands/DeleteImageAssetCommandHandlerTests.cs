using CQRS.CqrsResult;
using GroceryStore.Application.Images.Commands;
using GroceryStore.Domain.Entities.Media;
using GroceryStore.Domain.Interfaces;
using GroceryStore.Domain.ValueObjects;

namespace GroceryStore.Application.Tests.Images.Commands;

public class DeleteImageAssetHandlerTests
{
    private readonly Mock<IImageAssetRepository> _imageRepo = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly DeleteImageAssetCommandHandler _handler;

    public DeleteImageAssetHandlerTests()
    {
        _handler = new DeleteImageAssetCommandHandler(_imageRepo.Object, _unitOfWork.Object);
    }

    private static ImageAsset CreateAsset()
    {
        var metadata = ImageMetadata.Create("photo.jpg", "image/jpeg", 1024, 800, 600);
        return ImageAsset.Create("images/photo.jpg", "https://cdn.test/photo.jpg", metadata);
    }

    [Fact]
    public async Task HandleAsync_ExistingAsset_SoftDeletesAndReturnsSuccess()
    {
        // Arrange
        var asset = CreateAsset();
        _imageRepo.Setup(r => r.GetByIdAsync(It.IsAny<ImageId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(asset);

        // Act
        var result = await _handler.HandleAsync(new DeleteImageAssetCommand(asset.ImageId.Value));

        // Assert
        result.IsSuccess.Should().BeTrue();
        asset.IsDeleted.Should().BeTrue();
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
        var result = await _handler.HandleAsync(new DeleteImageAssetCommand(id));

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainSingle()
            .Which.Type.Should().Be(ErrorType.NotFound);
        _imageRepo.Verify(r => r.Update(It.IsAny<ImageAsset>()), Times.Never);
    }
}
