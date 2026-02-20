using CQRS.CqrsResult;
using GroceryStore.Application.Products.Commands.AssignImageAssetToProduct;
using GroceryStore.Domain.Entities;
using GroceryStore.Domain.Entities.Media;
using GroceryStore.Domain.Enums;
using GroceryStore.Domain.Interfaces;
using GroceryStore.Domain.ValueObjects;

namespace GroceryStore.Application.Tests.Products.Commands;

public class AssignImageAssetToProductCommandHandlerTests
{
    private readonly Mock<IProductRepository> _productRepo = new();
    private readonly Mock<IImageAssetRepository> _imageRepo = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly AssignImageAssetToProductCommandHandler _handler;

    public AssignImageAssetToProductCommandHandlerTests()
    {
        _handler = new AssignImageAssetToProductCommandHandler(
            _productRepo.Object,
            _imageRepo.Object,
            _unitOfWork.Object);
    }

    private static Product CreateProduct(Guid categoryId)
        => Product.Create(categoryId, "Apple", "apple", Money.Create(2, "CHF"), ProductUnit.Piece);

    private static ImageAsset CreateImageAsset(bool deleted = false)
    {
        var metadata = ImageMetadata.Create("photo.jpg", "image/jpeg", 1024, 800, 600);
        var asset = ImageAsset.Create("images/photo.jpg", "https://cdn.test/photo.jpg", metadata);
        if (deleted)
            asset.MarkDeleted();
        return asset;
    }

    [Fact]
    public async Task HandleAsync_ProductNotFound_ReturnsNotFound()
    {
        // Arrange
        var command = new AssignImageAssetToProductCommand(Guid.NewGuid(), Guid.NewGuid());
        _productRepo.Setup(r => r.GetByIdAsync(command.ProductId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainSingle().Which.Type.Should().Be(ErrorType.NotFound);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_ImageNotFound_ReturnsNotFound()
    {
        // Arrange
        var product = CreateProduct(Guid.NewGuid());
        var command = new AssignImageAssetToProductCommand(product.Id, Guid.NewGuid());

        _productRepo.Setup(r => r.GetByIdAsync(product.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);
        _imageRepo.Setup(r => r.GetByIdAsync(It.IsAny<ImageId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ImageAsset?)null);

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainSingle().Which.Type.Should().Be(ErrorType.NotFound);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_ImageDeleted_ReturnsNotFound()
    {
        // Arrange
        var product = CreateProduct(Guid.NewGuid());
        var command = new AssignImageAssetToProductCommand(product.Id, Guid.NewGuid());

        _productRepo.Setup(r => r.GetByIdAsync(product.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);
        _imageRepo.Setup(r => r.GetByIdAsync(It.IsAny<ImageId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateImageAsset(deleted: true));

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainSingle().Which.Type.Should().Be(ErrorType.NotFound);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_InvalidSortOrder_ReturnsValidation()
    {
        // Arrange
        var product = CreateProduct(Guid.NewGuid());
        var command = new AssignImageAssetToProductCommand(product.Id, Guid.NewGuid(), SortOrder: -1);

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainSingle().Which.Type.Should().Be(ErrorType.Validation);
    }

    [Fact]
    public async Task HandleAsync_AltTextTooLong_ReturnsValidation()
    {
        // Arrange
        var product = CreateProduct(Guid.NewGuid());
        var altText = new string('a', 201);
        var command = new AssignImageAssetToProductCommand(product.Id, Guid.NewGuid(), AltText: altText);

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainSingle().Which.Type.Should().Be(ErrorType.Validation);
    }

    [Fact]
    public async Task HandleAsync_NewImage_AttachesToProduct_AndSaves()
    {
        // Arrange
        var product = CreateProduct(Guid.NewGuid());
        var imageGuid = Guid.NewGuid();
        var command = new AssignImageAssetToProductCommand(product.Id, imageGuid, MakePrimary: true, SortOrder: 2, AltText: "Hero");

        _productRepo.Setup(r => r.GetByIdAsync(product.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);
        _imageRepo.Setup(r => r.GetByIdAsync(It.IsAny<ImageId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateImageAsset());

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        result.IsSuccess.Should().BeTrue();
        product.ImageRefs.Should().ContainSingle(r => r.ImageId.Value == imageGuid);
        product.ImageRefs.Single(r => r.ImageId.Value == imageGuid).IsPrimary.Should().BeTrue();
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _productRepo.Verify(r => r.Update(It.IsAny<Product>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_ImageAlreadyAttached_IsIdempotent_AndCanPromoteToPrimary()
    {
        // Arrange
        var product = CreateProduct(Guid.NewGuid());
        var imageGuid = Guid.NewGuid();
        var imageId = ImageId.Create(imageGuid);
        product.AttachImage(imageId, makePrimary: false);

        var command = new AssignImageAssetToProductCommand(product.Id, imageGuid, MakePrimary: true);

        _productRepo.Setup(r => r.GetByIdAsync(product.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);
        _imageRepo.Setup(r => r.GetByIdAsync(It.IsAny<ImageId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateImageAsset());

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        result.IsSuccess.Should().BeTrue();
        product.ImageRefs.Count(r => r.ImageId.Value == imageGuid).Should().Be(1);
        product.ImageRefs.Single(r => r.ImageId.Value == imageGuid).IsPrimary.Should().BeTrue();
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
