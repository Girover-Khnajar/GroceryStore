using CQRS.CqrsResult;
using GroceryStore.Application.Products.Queries;
using GroceryStore.Domain.Entities;
using GroceryStore.Domain.Enums;
using GroceryStore.Domain.Interfaces;
using GroceryStore.Domain.ValueObjects;

namespace GroceryStore.Application.Tests.Products.Queries;

public class GetProductBySlugHandlerTests
{
    private readonly Mock<IProductRepository> _productRepo = new();
    private readonly GetProductBySlugQueryHandler _handler;

    public GetProductBySlugHandlerTests()
    {
        _handler = new GetProductBySlugQueryHandler(_productRepo.Object);
    }

    [Fact]
    public async Task HandleAsync_ExistingSlug_ReturnsProductDto()
    {
        // Arrange
        var product = Product.Create(Guid.NewGuid(), "Apple", "apple", Money.Create(2, "CHF"), ProductUnit.Piece);
        _productRepo.Setup(r => r.GetBySlugAsync("apple", It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        // Act
        var result = await _handler.HandleAsync(new GetProductBySlugQuery("apple"));

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Slug.Should().Be("apple");
    }

    [Fact]
    public async Task HandleAsync_NonExistingSlug_ReturnsNotFound()
    {
        // Arrange
        _productRepo.Setup(r => r.GetBySlugAsync("nonexistent", It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);

        // Act
        var result = await _handler.HandleAsync(new GetProductBySlugQuery("nonexistent"));

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainSingle()
            .Which.Type.Should().Be(ErrorType.NotFound);
    }
}
