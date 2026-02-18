using CQRS.CqrsResult;
using GroceryStore.Application.Products.Commands;
using GroceryStore.Domain.Entities;
using GroceryStore.Domain.Enums;
using GroceryStore.Domain.Interfaces;
using GroceryStore.Domain.ValueObjects;

namespace GroceryStore.Application.Tests.Products.Commands;

public class UpdateProductHandlerTests
{
    private readonly Mock<IProductRepository> _productRepo = new();
    private readonly Mock<ICategoryRepository> _categoryRepo = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly UpdateProductCommandHandler _handler;

    public UpdateProductHandlerTests()
    {
        _handler = new UpdateProductCommandHandler(_productRepo.Object, _categoryRepo.Object, _unitOfWork.Object);
    }

    private static Product CreateProduct(Guid categoryId, string slug = "apple")
        => Product.Create(categoryId, "Apple", slug, Money.Create(2, "CHF"), ProductUnit.Piece);

    private static UpdateProductCommand ValidCommand(Guid id, Guid categoryId, string slug = "apple-updated") => new(
        Id: id,
        CategoryId: categoryId,
        Name: "Apple Updated",
        Slug: slug,
        PriceAmount: 3.00m,
        PriceCurrency: "CHF",
        Unit: "Kg",
        SortOrder: 2,
        IsFeatured: true,
        ShortDescription: "Updated desc",
        LongDescription: "Updated long desc",
        SeoMetaTitle: "Updated Apple",
        SeoMetaDescription: "Updated apple desc");

    [Fact]
    public async Task HandleAsync_ValidCommand_ReturnsSuccess()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var product = CreateProduct(categoryId);

        _productRepo.Setup(r => r.GetByIdAsync(product.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);
        _categoryRepo.Setup(r => r.ExistsAsync(categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _productRepo.Setup(r => r.GetBySlugAsync("apple-updated", It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);

        // Act
        var result = await _handler.HandleAsync(ValidCommand(product.Id, categoryId));

        // Assert
        result.IsSuccess.Should().BeTrue();
        _productRepo.Verify(r => r.Update(product), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ProductNotFound_ReturnsNotFound()
    {
        // Arrange
        var id = Guid.NewGuid();
        _productRepo.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);

        // Act
        var result = await _handler.HandleAsync(ValidCommand(id, Guid.NewGuid()));

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainSingle()
            .Which.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task HandleAsync_CategoryNotFound_ReturnsNotFound()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var newCategoryId = Guid.NewGuid();
        var product = CreateProduct(categoryId);

        _productRepo.Setup(r => r.GetByIdAsync(product.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);
        _categoryRepo.Setup(r => r.ExistsAsync(newCategoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _handler.HandleAsync(ValidCommand(product.Id, newCategoryId));

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainSingle()
            .Which.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task HandleAsync_SlugConflict_ReturnsConflict()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var product = CreateProduct(categoryId, "apple");
        var other = CreateProduct(categoryId, "banana");

        _productRepo.Setup(r => r.GetByIdAsync(product.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);
        _categoryRepo.Setup(r => r.ExistsAsync(categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _productRepo.Setup(r => r.GetBySlugAsync("banana", It.IsAny<CancellationToken>()))
            .ReturnsAsync(other);

        // Act
        var result = await _handler.HandleAsync(ValidCommand(product.Id, categoryId, slug: "banana"));

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainSingle()
            .Which.Type.Should().Be(ErrorType.Conflict);
    }

    [Fact]
    public async Task HandleAsync_SameSlugSameProduct_ReturnsSuccess()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var product = CreateProduct(categoryId, "apple");

        _productRepo.Setup(r => r.GetByIdAsync(product.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);
        _categoryRepo.Setup(r => r.ExistsAsync(categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _productRepo.Setup(r => r.GetBySlugAsync("apple", It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        // Act
        var result = await _handler.HandleAsync(ValidCommand(product.Id, categoryId, slug: "apple"));

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task HandleAsync_InvalidUnit_ReturnsValidationError()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var product = CreateProduct(categoryId);

        _productRepo.Setup(r => r.GetByIdAsync(product.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);
        _categoryRepo.Setup(r => r.ExistsAsync(categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _productRepo.Setup(r => r.GetBySlugAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);

        var command = new UpdateProductCommand(
            Id: product.Id,
            CategoryId: categoryId,
            Name: "Apple",
            Slug: "apple",
            PriceAmount: 2,
            PriceCurrency: "CHF",
            Unit: "InvalidUnit",
            SortOrder: 0,
            IsFeatured: false,
            ShortDescription: null,
            LongDescription: null,
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
    public async Task HandleAsync_SetFeatured_CallsFeature()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var product = CreateProduct(categoryId);

        _productRepo.Setup(r => r.GetByIdAsync(product.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);
        _categoryRepo.Setup(r => r.ExistsAsync(categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _productRepo.Setup(r => r.GetBySlugAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);

        // Act
        var result = await _handler.HandleAsync(ValidCommand(product.Id, categoryId));

        // Assert
        result.IsSuccess.Should().BeTrue();
        product.IsFeatured.Should().BeTrue();
    }
}
