using CQRS.CqrsResult;
using GroceryStore.Application.Categories.Queries;
using GroceryStore.Domain.Entities;
using GroceryStore.Domain.Interfaces;

namespace GroceryStore.Application.Tests.Categories.Queries;

public class GetCategoryBySlugHandlerTests
{
    private readonly Mock<ICategoryRepository> _categoryRepo = new();
    private readonly GetCategoryBySlugQueryHandler _handler;

    public GetCategoryBySlugHandlerTests()
    {
        _handler = new GetCategoryBySlugQueryHandler(_categoryRepo.Object);
    }

    [Fact]
    public async Task HandleAsync_ExistingSlug_ReturnsCategoryDto()
    {
        // Arrange
        var category = Category.Create("Fruits", "fruits");
        _categoryRepo.Setup(r => r.GetBySlugAsync("fruits", It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        // Act
        var result = await _handler.HandleAsync(new GetCategoryBySlugQuery("fruits"));

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Slug.Should().Be("fruits");
    }

    [Fact]
    public async Task HandleAsync_NonExistingSlug_ReturnsNotFound()
    {
        // Arrange
        _categoryRepo.Setup(r => r.GetBySlugAsync("nonexistent", It.IsAny<CancellationToken>()))
            .ReturnsAsync((Category?)null);

        // Act
        var result = await _handler.HandleAsync(new GetCategoryBySlugQuery("nonexistent"));

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainSingle()
            .Which.Type.Should().Be(ErrorType.NotFound);
    }
}
