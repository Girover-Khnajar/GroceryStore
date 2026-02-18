using CQRS.CqrsResult;
using GroceryStore.Application.Categories.Commands;
using GroceryStore.Domain.Entities;
using GroceryStore.Domain.Interfaces;
using GroceryStore.Domain.ValueObjects;

namespace GroceryStore.Application.Tests.Categories.Commands;

public class CreateCategoryHandlerTests
{
    private readonly Mock<ICategoryRepository> _categoryRepo = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly CreateCategoryCommandHandler _handler;

    public CreateCategoryHandlerTests()
    {
        _handler = new CreateCategoryCommandHandler(_categoryRepo.Object, _unitOfWork.Object);
    }

    private static CreateCategoryCommand ValidCommand(
        string slug = "fruits",
        Guid? parentId = null) => new(
        Name: "Fruits",
        Slug: slug,
        SortOrder: 1,
        ParentCategoryId: parentId,
        Description: "Fresh fruits",
        ImageUrl: "https://img.test/fruits.jpg",
        IconName: "fruit",
        SeoMetaTitle: "Fruits",
        SeoMetaDescription: "Buy fresh fruits");

    [Fact]
    public async Task HandleAsync_ValidCommand_ReturnsSuccessWithId()
    {
        // Arrange
        _categoryRepo.Setup(r => r.GetBySlugAsync("fruits", It.IsAny<CancellationToken>()))
            .ReturnsAsync((Category?)null);

        // Act
        var result = await _handler.HandleAsync(ValidCommand());

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();

        _categoryRepo.Verify(r => r.AddAsync(It.IsAny<Category>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_DuplicateSlug_ReturnsConflict()
    {
        // Arrange
        var existing = Category.Create("Existing", "fruits");
        _categoryRepo.Setup(r => r.GetBySlugAsync("fruits", It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        // Act
        var result = await _handler.HandleAsync(ValidCommand());

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainSingle()
            .Which.Type.Should().Be(ErrorType.Conflict);

        _categoryRepo.Verify(r => r.AddAsync(It.IsAny<Category>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WithParent_ParentExists_ReturnsSuccess()
    {
        // Arrange
        var parentId = Guid.NewGuid();
        _categoryRepo.Setup(r => r.GetBySlugAsync("tropical", It.IsAny<CancellationToken>()))
            .ReturnsAsync((Category?)null);
        _categoryRepo.Setup(r => r.ExistsAsync(parentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.HandleAsync(ValidCommand(slug: "tropical", parentId: parentId));

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task HandleAsync_WithParent_ParentNotFound_ReturnsNotFound()
    {
        // Arrange
        var parentId = Guid.NewGuid();
        _categoryRepo.Setup(r => r.GetBySlugAsync("tropical", It.IsAny<CancellationToken>()))
            .ReturnsAsync((Category?)null);
        _categoryRepo.Setup(r => r.ExistsAsync(parentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _handler.HandleAsync(ValidCommand(slug: "tropical", parentId: parentId));

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainSingle()
            .Which.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task HandleAsync_NoParent_DoesNotCheckParentExists()
    {
        // Arrange
        _categoryRepo.Setup(r => r.GetBySlugAsync("fruits", It.IsAny<CancellationToken>()))
            .ReturnsAsync((Category?)null);

        // Act
        await _handler.HandleAsync(ValidCommand());

        // Assert
        _categoryRepo.Verify(r => r.ExistsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
