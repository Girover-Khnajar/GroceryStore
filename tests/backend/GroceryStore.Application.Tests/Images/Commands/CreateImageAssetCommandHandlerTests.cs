using CQRS.CqrsResult;
using GroceryStore.Application.Images.Commands;
using GroceryStore.Domain.Entities.Media;
using GroceryStore.Domain.Interfaces;
using GroceryStore.Domain.ValueObjects;

namespace GroceryStore.Application.Tests.Images.Commands;

public class CreateImageAssetHandlerTests
{
    private readonly Mock<IImageAssetRepository> _imageRepo = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly CreateImageAssetCommandHandler _handler;

    public CreateImageAssetHandlerTests()
    {
        _handler = new CreateImageAssetCommandHandler(_imageRepo.Object, _unitOfWork.Object);
    }

    private static CreateImageAssetCommand ValidCommand() => new(
        StoragePath: "images/2024/apple.jpg",
        Url: "https://cdn.test/apple.jpg",
        FileName: "apple.jpg",
        ContentType: "image/jpeg",
        FileSizeBytes: 1024 * 100,
        Width: 800,
        Height: 600,
        AltText: "A fresh apple");

    [Fact]
    public async Task HandleAsync_ValidCommand_ReturnsSuccessWithId()
    {
        // Act
        var result = await _handler.HandleAsync(ValidCommand());

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();
        _imageRepo.Verify(r => r.AddAsync(It.IsAny<ImageAsset>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_NoAltText_ReturnsSuccess()
    {
        // Arrange
        var command = new CreateImageAssetCommand(
            StoragePath: "images/test.png",
            Url: "https://cdn.test/test.png",
            FileName: "test.png",
            ContentType: "image/png",
            FileSizeBytes: 2048,
            Width: 100,
            Height: 100,
            AltText: null);

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task HandleAsync_ZeroDimensions_PassesNullDimensions()
    {
        // Arrange
        var command = new CreateImageAssetCommand(
            StoragePath: "images/icon.svg",
            Url: "https://cdn.test/icon.svg",
            FileName: "icon.svg",
            ContentType: "image/svg+xml",
            FileSizeBytes: 512,
            Width: 0,
            Height: 0);

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }
}
