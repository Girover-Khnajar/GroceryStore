using GroceryStore.Application.Products.Queries;
using GroceryStore.Domain.Entities;
using GroceryStore.Domain.Enums;
using GroceryStore.Domain.Interfaces;
using GroceryStore.Domain.ValueObjects;

namespace GroceryStore.Application.Tests.Products.Queries;

public class GetProductsByCategoryIdHandlerTests
{
    private readonly Mock<IProductRepository> _productRepo = new();
    private readonly GetProductsByCategoryIdQueryHandler _handler;

    public GetProductsByCategoryIdHandlerTests()
    {
        _handler = new GetProductsByCategoryIdQueryHandler(_productRepo.Object);
    }

    [Fact]
    public async Task HandleAsync_ReturnsProducts()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var products = new List<Product>
        {
            Product.Create(categoryId, "Apple", "apple", Money.Create(2, "CHF"), ProductUnit.Piece),
            Product.Create(categoryId, "Banana", "banana", Money.Create(1.50m, "CHF"), ProductUnit.Kg)
        };
        _productRepo.Setup(r => r.GetByCategoryIdAsync(categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(products);

        // Act
        var result = await _handler.HandleAsync(new GetProductsByCategoryIdQuery(categoryId));

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
    }

    [Fact]
    public async Task HandleAsync_NoProducts_ReturnsEmptyList()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        _productRepo.Setup(r => r.GetByCategoryIdAsync(categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Product>());

        // Act
        var result = await _handler.HandleAsync(new GetProductsByCategoryIdQuery(categoryId));

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }
}
