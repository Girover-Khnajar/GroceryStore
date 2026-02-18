using GroceryStore.Application.Categories.Queries;
using GroceryStore.Domain.Entities;
using GroceryStore.Domain.Interfaces;

namespace GroceryStore.Application.Tests.Categories.Queries;

public class GetAllActiveCategoriesHandlerTests
{
    private readonly Mock<ICategoryRepository> _categoryRepo = new();
    private readonly GetAllActiveCategoriesQueryHandler _handler;

    public GetAllActiveCategoriesHandlerTests()
    {
        _handler = new GetAllActiveCategoriesQueryHandler(_categoryRepo.Object);
    }

    [Fact]
    public async Task HandleAsync_ReturnsAllActiveCategories()
    {
        // Arrange
        var categories = new List<Category>
        {
            Category.Create("Fruits", "fruits"),
            Category.Create("Vegetables", "vegetables")
        };
        _categoryRepo.Setup(r => r.GetAllActiveAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(categories);

        // Act
        var result = await _handler.HandleAsync(new GetAllActiveCategoriesQuery());

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
    }

    [Fact]
    public async Task HandleAsync_NoActive_ReturnsEmptyList()
    {
        // Arrange
        _categoryRepo.Setup(r => r.GetAllActiveAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Category>());

        // Act
        var result = await _handler.HandleAsync(new GetAllActiveCategoriesQuery());

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }
}
