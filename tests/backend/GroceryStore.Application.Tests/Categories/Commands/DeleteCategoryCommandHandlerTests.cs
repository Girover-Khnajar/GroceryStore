using CQRS.CqrsResult;
using GroceryStore.Application.Categories.Commands;
using GroceryStore.Domain.Entities;
using GroceryStore.Domain.Interfaces;

namespace GroceryStore.Application.Tests.Categories.Commands;

public class DeleteCategoryHandlerTests
{
    private readonly Mock<ICategoryRepository> _categoryRepo = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly DeleteCategoryCommandHandler _handler;

    public DeleteCategoryHandlerTests()
    {
        _handler = new DeleteCategoryCommandHandler(_categoryRepo.Object, _unitOfWork.Object);
    }

    [Fact]
    public async Task HandleAsync_ExistingCategoryNoChildren_ReturnsSuccess()
    {
        // Arrange
        var category = Category.Create("Fruits", "fruits");
        _categoryRepo.Setup(r => r.GetByIdAsync(category.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);
        _categoryRepo.Setup(r => r.GetByParentIdAsync(category.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Category>());

        // Act
        var result = await _handler.HandleAsync(new DeleteCategoryCommand(category.Id));

        // Assert
        result.IsSuccess.Should().BeTrue();
        _categoryRepo.Verify(r => r.Remove(category), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_CategoryNotFound_ReturnsNotFound()
    {
        // Arrange
        var id = Guid.NewGuid();
        _categoryRepo.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Category?)null);

        // Act
        var result = await _handler.HandleAsync(new DeleteCategoryCommand(id));

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainSingle()
            .Which.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task HandleAsync_CategoryHasChildren_ReturnsConflict()
    {
        // Arrange
        var category = Category.Create("Fruits", "fruits");
        var child = Category.Create("Tropical", "tropical", parentCategoryId: category.Id);

        _categoryRepo.Setup(r => r.GetByIdAsync(category.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);
        _categoryRepo.Setup(r => r.GetByParentIdAsync(category.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Category> { child });

        // Act
        var result = await _handler.HandleAsync(new DeleteCategoryCommand(category.Id));

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainSingle()
            .Which.Type.Should().Be(ErrorType.Conflict);
        _categoryRepo.Verify(r => r.Remove(It.IsAny<Category>()), Times.Never);
    }
}
