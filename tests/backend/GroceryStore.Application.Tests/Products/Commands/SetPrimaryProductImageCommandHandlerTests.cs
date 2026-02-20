using CQRS.CqrsResult;
using GroceryStore.Application.Products.Commands.SetPrimaryProductImage;
using GroceryStore.Domain.Entities;
using GroceryStore.Domain.Entities.Media;
using GroceryStore.Domain.Enums;
using GroceryStore.Domain.Interfaces;
using GroceryStore.Domain.ValueObjects;

namespace GroceryStore.Application.Tests.Products.Commands;

public class SetPrimaryProductImageCommandHandlerTests
{
    private readonly Mock<IProductRepository> _productRepo = new();
    private readonly Mock<IImageAssetRepository> _imageRepo = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly SetPrimaryProductImageCommandHandler _handler;

    public SetPrimaryProductImageCommandHandlerTests()
    {
        _handler = new SetPrimaryProductImageCommandHandler(
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
        var command = new SetPrimaryProductImageCommand(Guid.NewGuid(), Guid.NewGuid());
        _productRepo.Setup(r => r.GetByIdAsync(command.ProductId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainSingle().Which.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task HandleAsync_NoImagesAttached_ReturnsValidation()
    {
        // Arrange
        var product = CreateProduct(Guid.NewGuid());
        var command = new SetPrimaryProductImageCommand(product.Id, Guid.NewGuid());

        _productRepo.Setup(r => r.GetByIdAsync(product.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainSingle().Which.Type.Should().Be(ErrorType.Validation);
    }

    [Fact]
    public async Task HandleAsync_ImageNotAttached_ReturnsValidation()
    {
        // Arrange
        var product = CreateProduct(Guid.NewGuid());
        product.AttachImage(ImageId.Create(Guid.NewGuid()));

        var command = new SetPrimaryProductImageCommand(product.Id, Guid.NewGuid());

        _productRepo.Setup(r => r.GetByIdAsync(product.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainSingle().Which.Type.Should().Be(ErrorType.Validation);
    }

    [Fact]
    public async Task HandleAsync_ImageAssetNotFound_ReturnsNotFound()
    {
        // Arrange
        var product = CreateProduct(Guid.NewGuid());
        var imageGuid = Guid.NewGuid();
        product.AttachImage(ImageId.Create(imageGuid));

        var command = new SetPrimaryProductImageCommand(product.Id, imageGuid);

        _productRepo.Setup(r => r.GetByIdAsync(product.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);
        _imageRepo.Setup(r => r.GetByIdAsync(It.IsAny<ImageId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ImageAsset?)null);

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainSingle().Which.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task HandleAsync_ImageAssetDeleted_ReturnsNotFound()
    {
        // Arrange
        var product = CreateProduct(Guid.NewGuid());
        var imageGuid = Guid.NewGuid();
        product.AttachImage(ImageId.Create(imageGuid));

        var command = new SetPrimaryProductImageCommand(product.Id, imageGuid);

        _productRepo.Setup(r => r.GetByIdAsync(product.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);
        _imageRepo.Setup(r => r.GetByIdAsync(It.IsAny<ImageId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateImageAsset(deleted: true));

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainSingle().Which.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task HandleAsync_AttachedImage_SetsPrimary_AndSaves()
    {
        // Arrange
        var product = CreateProduct(Guid.NewGuid());
        var imageGuid1 = Guid.NewGuid();
        var imageGuid2 = Guid.NewGuid();
        product.AttachImage(ImageId.Create(imageGuid1));
        product.AttachImage(ImageId.Create(imageGuid2));

        var command = new SetPrimaryProductImageCommand(product.Id, imageGuid2);

        _productRepo.Setup(r => r.GetByIdAsync(product.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);
        _imageRepo.Setup(r => r.GetByIdAsync(It.IsAny<ImageId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateImageAsset());

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        result.IsSuccess.Should().BeTrue();
        product.ImageRefs.Single(r => r.ImageId.Value == imageGuid2).IsPrimary.Should().BeTrue();
        product.ImageRefs.Count(r => r.IsPrimary).Should().Be(1);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _productRepo.Verify(r => r.Update(It.IsAny<Product>()), Times.Never);
    }
}
