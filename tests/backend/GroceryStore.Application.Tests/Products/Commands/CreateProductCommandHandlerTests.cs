using CQRS.CqrsResult;
using GroceryStore.Application.Products.Commands;
using GroceryStore.Domain.Entities;
using GroceryStore.Domain.Interfaces;
using GroceryStore.Domain.ValueObjects;

namespace GroceryStore.Application.Tests.Products.Commands;

public class CreateProductHandlerTests
{
    private readonly Mock<IProductRepository> _productRepo = new();
    private readonly Mock<ICategoryRepository> _categoryRepo = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly CreateProductCommandHandler _handler;

    public CreateProductHandlerTests()
    {
        _handler = new CreateProductCommandHandler(_productRepo.Object, _categoryRepo.Object, _unitOfWork.Object);
    }

    private static CreateProductCommand ValidCommand(Guid categoryId, string slug = "apple") => new(
        CategoryId: categoryId,
        Name: "Apple",
        Slug: slug,
        PriceAmount: 2.50m,
        PriceCurrency: "CHF",
        Unit: "Piece",
        SortOrder: 1,
        IsFeatured: false,
        ShortDescription: "Fresh apple",
        LongDescription: "A crisp, fresh apple",
        SeoMetaTitle: "Apple",
        SeoMetaDescription: "Buy apples");

    [Fact]
    public async Task HandleAsync_ValidCommand_ReturnsSuccessWithId()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        _categoryRepo.Setup(r => r.ExistsAsync(categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _productRepo.Setup(r => r.GetBySlugAsync("apple", It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);

        // Act
        var result = await _handler.HandleAsync(ValidCommand(categoryId));

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();
        _productRepo.Verify(r => r.AddAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_CategoryNotFound_ReturnsNotFound()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        _categoryRepo.Setup(r => r.ExistsAsync(categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _handler.HandleAsync(ValidCommand(categoryId));

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainSingle()
            .Which.Type.Should().Be(ErrorType.NotFound);
        _productRepo.Verify(r => r.AddAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_DuplicateSlug_ReturnsConflict()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        _categoryRepo.Setup(r => r.ExistsAsync(categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var existing = Product.Create(categoryId, "Existing", "apple", Money.Create(1, "CHF"),
            GroceryStore.Domain.Enums.ProductUnit.Piece);
        _productRepo.Setup(r => r.GetBySlugAsync("apple", It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        // Act
        var result = await _handler.HandleAsync(ValidCommand(categoryId));

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainSingle()
            .Which.Type.Should().Be(ErrorType.Conflict);
    }

    [Fact]
    public async Task HandleAsync_InvalidUnit_ReturnsValidationError()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        _categoryRepo.Setup(r => r.ExistsAsync(categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _productRepo.Setup(r => r.GetBySlugAsync("apple", It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);

        var command = new CreateProductCommand(
            CategoryId: categoryId,
            Name: "Apple",
            Slug: "apple",
            PriceAmount: 2.50m,
            PriceCurrency: "CHF",
            Unit: "InvalidUnit");

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainSingle()
            .Which.Type.Should().Be(ErrorType.Validation);
    }
}
