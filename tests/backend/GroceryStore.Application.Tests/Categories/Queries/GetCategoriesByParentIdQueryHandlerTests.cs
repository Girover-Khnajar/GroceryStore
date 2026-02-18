using GroceryStore.Application.Categories.Queries;
using GroceryStore.Domain.Entities;
using GroceryStore.Domain.Interfaces;

namespace GroceryStore.Application.Tests.Categories.Queries;

public class GetCategoriesByParentIdHandlerTests
{
    private readonly Mock<ICategoryRepository> _categoryRepo = new();
    private readonly GetCategoriesByParentIdQueryHandler _handler;

    public GetCategoriesByParentIdHandlerTests()
    {
        _handler = new GetCategoriesByParentIdQueryHandler(_categoryRepo.Object);
    }

    [Fact]
    public async Task HandleAsync_WithParentId_ReturnsChildren()
    {
        // Arrange
        var parentId = Guid.NewGuid();
        var children = new List<Category>
        {
            Category.Create("Tropical", "tropical", parentCategoryId: parentId),
            Category.Create("Berries", "berries", parentCategoryId: parentId)
        };
        _categoryRepo.Setup(r => r.GetByParentIdAsync(parentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(children);

        // Act
        var result = await _handler.HandleAsync(new GetCategoriesByParentIdQuery(parentId));

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
    }

    [Fact]
    public async Task HandleAsync_NullParentId_ReturnsRootCategories()
    {
        // Arrange
        var roots = new List<Category> { Category.Create("Fruits", "fruits") };
        _categoryRepo.Setup(r => r.GetByParentIdAsync(null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(roots);

        // Act
        var result = await _handler.HandleAsync(new GetCategoriesByParentIdQuery(null));

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
    }

    [Fact]
    public async Task HandleAsync_NoChildren_ReturnsEmptyList()
    {
        // Arrange
        var parentId = Guid.NewGuid();
        _categoryRepo.Setup(r => r.GetByParentIdAsync(parentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Category>());

        // Act
        var result = await _handler.HandleAsync(new GetCategoriesByParentIdQuery(parentId));

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }
}
