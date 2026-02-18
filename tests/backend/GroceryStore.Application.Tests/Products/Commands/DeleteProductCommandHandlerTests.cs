using CQRS.CqrsResult;
using GroceryStore.Application.Products.Commands;
using GroceryStore.Domain.Entities;
using GroceryStore.Domain.Enums;
using GroceryStore.Domain.Interfaces;
using GroceryStore.Domain.ValueObjects;

namespace GroceryStore.Application.Tests.Products.Commands;

public class DeleteProductHandlerTests
{
    private readonly Mock<IProductRepository> _productRepo = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly DeleteProductCommandHandler _handler;

    public DeleteProductHandlerTests()
    {
        _handler = new DeleteProductCommandHandler(_productRepo.Object, _unitOfWork.Object);
    }

    [Fact]
    public async Task HandleAsync_ExistingProduct_ReturnsSuccess()
    {
        // Arrange
        var product = Product.Create(Guid.NewGuid(), "Apple", "apple", Money.Create(2, "CHF"), ProductUnit.Piece);
        _productRepo.Setup(r => r.GetByIdAsync(product.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        // Act
        var result = await _handler.HandleAsync(new DeleteProductCommand(product.Id));

        // Assert
        result.IsSuccess.Should().BeTrue();
        _productRepo.Verify(r => r.Remove(product), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ProductNotFound_ReturnsNotFound()
    {
        // Arrange
        var id = Guid.NewGuid();
        _productRepo.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);

        // Act
        var result = await _handler.HandleAsync(new DeleteProductCommand(id));

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainSingle()
            .Which.Type.Should().Be(ErrorType.NotFound);
        _productRepo.Verify(r => r.Remove(It.IsAny<Product>()), Times.Never);
    }
}
