using CQRS.CqrsResult;
using GroceryStore.Application.Categories.Commands;
using GroceryStore.Domain.Entities;
using GroceryStore.Domain.Interfaces;

namespace GroceryStore.Application.Tests.Categories.Commands;

public class UpdateCategoryHandlerTests
{
    private readonly Mock<ICategoryRepository> _categoryRepo = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly UpdateCategoryCommandHandler _handler;

    public UpdateCategoryHandlerTests()
    {
        _handler = new UpdateCategoryCommandHandler(_categoryRepo.Object, _unitOfWork.Object);
    }

    private static Category CreateCategory(string name = "Fruits", string slug = "fruits")
        => Category.Create(name, slug);

    private static UpdateCategoryCommand ValidCommand(Guid id, string slug = "fruits-updated") => new(
        Id: id,
        Name: "Fruits Updated",
        Slug: slug,
        SortOrder: 2,
        ParentCategoryId: null,
        Description: "Updated desc",
        ImageUrl: null,
        IconName: null,
        SeoMetaTitle: "Updated",
        SeoMetaDescription: "Updated desc");

    [Fact]
    public async Task HandleAsync_ValidCommand_ReturnsSuccess()
    {
        // Arrange
        var category = CreateCategory();
        var id = category.Id;

        _categoryRepo.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);
        _categoryRepo.Setup(r => r.GetBySlugAsync("fruits-updated", It.IsAny<CancellationToken>()))
            .ReturnsAsync((Category?)null);

        // Act
        var result = await _handler.HandleAsync(ValidCommand(id));

        // Assert
        result.IsSuccess.Should().BeTrue();
        _categoryRepo.Verify(r => r.Update(category), Times.Once);
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
        var result = await _handler.HandleAsync(ValidCommand(id));

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainSingle()
            .Which.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task HandleAsync_SlugConflict_ReturnsConflict()
    {
        // Arrange
        var category = CreateCategory();
        var otherId = Guid.NewGuid();
        var other = CreateCategory("Veggies", "veggies");

        _categoryRepo.Setup(r => r.GetByIdAsync(category.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);
        _categoryRepo.Setup(r => r.GetBySlugAsync("veggies", It.IsAny<CancellationToken>()))
            .ReturnsAsync(other);

        // Act
        var result = await _handler.HandleAsync(ValidCommand(category.Id, slug: "veggies"));

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainSingle()
            .Which.Type.Should().Be(ErrorType.Conflict);
    }

    [Fact]
    public async Task HandleAsync_SameSlugSameEntity_ReturnsSuccess()
    {
        // Arrange
        var category = CreateCategory();

        _categoryRepo.Setup(r => r.GetByIdAsync(category.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);
        _categoryRepo.Setup(r => r.GetBySlugAsync("fruits", It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        // Act
        var result = await _handler.HandleAsync(ValidCommand(category.Id, slug: "fruits"));

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task HandleAsync_SelfParent_ReturnsValidationError()
    {
        // Arrange
        var category = CreateCategory();
        _categoryRepo.Setup(r => r.GetByIdAsync(category.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);
        _categoryRepo.Setup(r => r.GetBySlugAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Category?)null);

        var command = new UpdateCategoryCommand(
            Id: category.Id,
            Name: "Fruits",
            Slug: "fruits",
            SortOrder: 0,
            ParentCategoryId: category.Id, // self-parent
            Description: null,
            ImageUrl: null,
            IconName: null,
            SeoMetaTitle: null,
            SeoMetaDescription: null);

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainSingle()
            .Which.Type.Should().Be(ErrorType.Validation);
    }

    [Fact]
    public async Task HandleAsync_ParentNotFound_ReturnsNotFound()
    {
        // Arrange
        var category = CreateCategory();
        var parentId = Guid.NewGuid();

        _categoryRepo.Setup(r => r.GetByIdAsync(category.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);
        _categoryRepo.Setup(r => r.GetBySlugAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Category?)null);
        _categoryRepo.Setup(r => r.ExistsAsync(parentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var command = new UpdateCategoryCommand(
            Id: category.Id,
            Name: "Fruits",
            Slug: "fruits",
            SortOrder: 0,
            ParentCategoryId: parentId,
            Description: null,
            ImageUrl: null,
            IconName: null,
            SeoMetaTitle: null,
            SeoMetaDescription: null);

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainSingle()
            .Which.Type.Should().Be(ErrorType.NotFound);
    }
}
