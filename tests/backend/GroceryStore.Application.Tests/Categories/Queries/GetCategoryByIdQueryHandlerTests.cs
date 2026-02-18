using CQRS.CqrsResult;
using GroceryStore.Application.Categories.Dtos;
using GroceryStore.Application.Categories.Queries;
using GroceryStore.Domain.Entities;
using GroceryStore.Domain.Interfaces;

namespace GroceryStore.Application.Tests.Categories.Queries;

public class GetCategoryByIdHandlerTests
{
    private readonly Mock<ICategoryRepository> _categoryRepo = new();
    private readonly GetCategoryByIdQueryHandler _handler;

    public GetCategoryByIdHandlerTests()
    {
        _handler = new GetCategoryByIdQueryHandler(_categoryRepo.Object);
    }

    [Fact]
    public async Task HandleAsync_ExistingCategory_ReturnsCategoryDto()
    {
        // Arrange
        var category = Category.Create("Fruits", "fruits", description: "Fresh");
        _categoryRepo.Setup(r => r.GetByIdAsync(category.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        // Act
        var result = await _handler.HandleAsync(new GetCategoryByIdQuery(category.Id));

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Id.Should().Be(category.Id);
        result.Value.Name.Should().Be("Fruits");
        result.Value.Slug.Should().Be("fruits");
    }

    [Fact]
    public async Task HandleAsync_NonExistingCategory_ReturnsNotFound()
    {
        // Arrange
        var id = Guid.NewGuid();
        _categoryRepo.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Category?)null);

        // Act
        var result = await _handler.HandleAsync(new GetCategoryByIdQuery(id));

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainSingle()
            .Which.Type.Should().Be(ErrorType.NotFound);
    }
}
