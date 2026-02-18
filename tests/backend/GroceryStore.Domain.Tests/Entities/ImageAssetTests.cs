using FluentAssertions;
using GroceryStore.Domain.Entities.Media;
using GroceryStore.Domain.Exceptions;
using GroceryStore.Domain.Interfaces;
using GroceryStore.Domain.ValueObjects;

namespace GroceryStore.Domain.Tests.Entities;

public class ImageAssetTests
{
    // ─── Helpers ───

    private static ImageMetadata ValidMetadata(
        string fileName = "photo.jpg",
        string contentType = "image/jpeg",
        long size = 100_000)
        => ImageMetadata.Create(fileName, contentType, size, 800, 600);

    private static ImageAsset CreateValid(
        string storagePath = "images/2024/photo.jpg",
        string url = "https://cdn.test/photo.jpg",
        ImageMetadata? metadata = null,
        string? altText = null)
        => ImageAsset.Create(storagePath, url, metadata ?? ValidMetadata(), altText);

    // ═══════════════════════════════════════════
    // Create
    // ═══════════════════════════════════════════

    [Fact]
    public void Create_ValidInputs_ReturnsImageAsset()
    {
        var meta = ValidMetadata();
        var asset = ImageAsset.Create("images/pic.jpg", "https://cdn/pic.jpg", meta, "A picture");

        asset.ImageId.Should().NotBeNull();
        asset.ImageId.Value.Should().NotBeEmpty();
        asset.StoragePath.Should().Be("images/pic.jpg");
        asset.Url.Should().Be("https://cdn/pic.jpg");
        asset.Metadata.Should().Be(meta);
        asset.AltText.Should().Be("A picture");
        asset.IsDeleted.Should().BeFalse();
    }

    [Fact]
    public void Create_SyncsEntityIdWithImageId()
    {
        var asset = CreateValid();

        asset.Id.Should().Be(asset.ImageId.Value);
    }

    [Fact]
    public void Create_TrimsStoragePathAndUrl()
    {
        var asset = ImageAsset.Create("  images/pic.jpg  ", "  https://cdn/pic.jpg  ", ValidMetadata());

        asset.StoragePath.Should().Be("images/pic.jpg");
        asset.Url.Should().Be("https://cdn/pic.jpg");
    }

    [Fact]
    public void Create_NullAltText_IsAllowed()
    {
        var asset = CreateValid(altText: null);

        asset.AltText.Should().BeNull();
    }

    [Fact]
    public void Create_ImplementsIAggregateRoot()
    {
        var asset = CreateValid();

        asset.Should().BeAssignableTo<IAggregateRoot>();
    }

    // ── Create validation ──

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_NullOrEmptyStoragePath_Throws(string? path)
    {
        var act = () => ImageAsset.Create(path!, "https://cdn/pic.jpg", ValidMetadata());

        act.Should().Throw<ValidationException>();
    }

    [Fact]
    public void Create_StoragePathTooLong_Throws()
    {
        var act = () => ImageAsset.Create(new string('a', 501), "https://cdn/pic.jpg", ValidMetadata());

        act.Should().Throw<ValidationException>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_NullOrEmptyUrl_Throws(string? url)
    {
        var act = () => ImageAsset.Create("images/pic.jpg", url!, ValidMetadata());

        act.Should().Throw<ValidationException>();
    }

    [Fact]
    public void Create_UrlTooLong_Throws()
    {
        var act = () => ImageAsset.Create("images/pic.jpg", new string('a', 1001), ValidMetadata());

        act.Should().Throw<ValidationException>();
    }

    [Fact]
    public void Create_NullMetadata_Throws()
    {
        var act = () => ImageAsset.Create("images/pic.jpg", "https://cdn/pic.jpg", null!);

        act.Should().Throw<ValidationException>();
    }

    [Fact]
    public void Create_AltTextTooLong_Throws()
    {
        var act = () => CreateValid(altText: new string('a', 201));

        act.Should().Throw<ValidationException>();
    }

    // ═══════════════════════════════════════════
    // ChangeAltText
    // ═══════════════════════════════════════════

    [Fact]
    public void ChangeAltText_ValidText_Updates()
    {
        var asset = CreateValid();

        asset.ChangeAltText("New alt text");

        asset.AltText.Should().Be("New alt text");
        asset.ModifiedOnUtc.Should().NotBeNull();
    }

    [Fact]
    public void ChangeAltText_Null_ClearsAltText()
    {
        var asset = CreateValid(altText: "old");

        asset.ChangeAltText(null);

        asset.AltText.Should().BeNull();
    }

    [Fact]
    public void ChangeAltText_TrimsText()
    {
        var asset = CreateValid();

        asset.ChangeAltText("  trimmed  ");

        asset.AltText.Should().Be("trimmed");
    }

    [Fact]
    public void ChangeAltText_TooLong_Throws()
    {
        var asset = CreateValid();

        var act = () => asset.ChangeAltText(new string('x', 201));

        act.Should().Throw<ValidationException>();
    }

    // ═══════════════════════════════════════════
    // MarkDeleted
    // ═══════════════════════════════════════════

    [Fact]
    public void MarkDeleted_SetsIsDeleted()
    {
        var asset = CreateValid();

        asset.MarkDeleted();

        asset.IsDeleted.Should().BeTrue();
        asset.ModifiedOnUtc.Should().NotBeNull();
    }

    [Fact]
    public void MarkDeleted_AlreadyDeleted_IsIdempotent()
    {
        var asset = CreateValid();
        asset.MarkDeleted();
        var firstModified = asset.ModifiedOnUtc;

        asset.MarkDeleted(); // Second call

        asset.IsDeleted.Should().BeTrue();
        asset.ModifiedOnUtc.Should().Be(firstModified); // No additional Touch
    }

    // ═══════════════════════════════════════════
    // Two assets have different ImageIds
    // ═══════════════════════════════════════════

    [Fact]
    public void Create_TwoAssets_DifferentImageIds()
    {
        var a = CreateValid();
        var b = CreateValid();

        a.ImageId.Should().NotBe(b.ImageId);
    }
}
