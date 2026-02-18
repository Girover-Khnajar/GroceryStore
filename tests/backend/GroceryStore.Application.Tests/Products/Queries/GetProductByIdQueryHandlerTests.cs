using CQRS.CqrsResult;
using GroceryStore.Application.Products.Dtos;
using GroceryStore.Application.Products.Queries;
using GroceryStore.Domain.Entities;
using GroceryStore.Domain.Enums;
using GroceryStore.Domain.Interfaces;
using GroceryStore.Domain.ValueObjects;

namespace GroceryStore.Application.Tests.Products.Queries;

public class GetProductByIdHandlerTests
{
    private readonly Mock<IProductRepository> _productRepo = new();
    private readonly GetProductByIdQueryHandler _handler;

    public GetProductByIdHandlerTests()
    {
        _handler = new GetProductByIdQueryHandler(_productRepo.Object);
    }

    [Fact]
    public async Task HandleAsync_ExistingProduct_ReturnsProductDto()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var product = Product.Create(categoryId, "Apple", "apple", Money.Create(2.50m, "CHF"), ProductUnit.Piece);
        _productRepo.Setup(r => r.GetByIdAsync(product.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        // Act
        var result = await _handler.HandleAsync(new GetProductByIdQuery(product.Id));

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Id.Should().Be(product.Id);
        result.Value.Name.Should().Be("Apple");
        result.Value.Slug.Should().Be("apple");
        result.Value.PriceAmount.Should().Be(2.50m);
    }

    [Fact]
    public async Task HandleAsync_NonExistingProduct_ReturnsNotFound()
    {
        // Arrange
        var id = Guid.NewGuid();
        _productRepo.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);

        // Act
        var result = await _handler.HandleAsync(new GetProductByIdQuery(id));

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainSingle()
            .Which.Type.Should().Be(ErrorType.NotFound);
    }
}
