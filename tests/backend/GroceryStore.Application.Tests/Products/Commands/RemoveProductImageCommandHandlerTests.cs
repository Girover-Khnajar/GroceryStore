using CQRS.CqrsResult;
using GroceryStore.Application.Products.Commands.RemoveProductImage;
using GroceryStore.Domain.Entities;
using GroceryStore.Domain.Enums;
using GroceryStore.Domain.Interfaces;
using GroceryStore.Domain.ValueObjects;

namespace GroceryStore.Application.Tests.Products.Commands;

public class RemoveProductImageCommandHandlerTests
{
    private readonly Mock<IProductRepository> _productRepo = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly RemoveProductImageCommandHandler _handler;

    public RemoveProductImageCommandHandlerTests()
    {
        _handler = new RemoveProductImageCommandHandler(_productRepo.Object, _unitOfWork.Object);
    }

    private static Product CreateProduct(Guid categoryId)
        => Product.Create(categoryId, "Apple", "apple", Money.Create(2, "CHF"), ProductUnit.Piece);

    [Fact]
    public async Task HandleAsync_ProductNotFound_ReturnsNotFound()
    {
        // Arrange
        var command = new RemoveProductImageCommand(Guid.NewGuid(), Guid.NewGuid());
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
    public async Task HandleAsync_ImageNotAttached_IsIdempotent_ReturnsSuccess()
    {
        // Arrange
        var product = CreateProduct(Guid.NewGuid());
        var command = new RemoveProductImageCommand(product.Id, Guid.NewGuid());

        _productRepo.Setup(r => r.GetByIdAsync(product.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _productRepo.Verify(r => r.Update(It.IsAny<Product>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_RemovesImage_WhenAttached()
    {
        // Arrange
        var product = CreateProduct(Guid.NewGuid());
        var imageGuid = Guid.NewGuid();
        product.AttachImage(ImageId.Create(imageGuid));

        var command = new RemoveProductImageCommand(product.Id, imageGuid);

        _productRepo.Setup(r => r.GetByIdAsync(product.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        result.IsSuccess.Should().BeTrue();
        product.ImageRefs.Should().BeEmpty();
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
