using FluentAssertions;
using GroceryStore.Domain.Exceptions;
using GroceryStore.Domain.ValueObjects;

namespace GroceryStore.Domain.Tests.ValueObjects;

public class ImageMetadataTests
{
    private const string ValidFileName = "photo.jpg";
    private const string ValidContentType = "image/jpeg";
    private const long ValidSize = 1024 * 100; // 100 KB

    private static ImageMetadata CreateValid(
        string? fileName = null,
        string? contentType = null,
        long? size = null,
        int? width = null,
        int? height = null)
        => ImageMetadata.Create(
            fileName ?? ValidFileName,
            contentType ?? ValidContentType,
            size ?? ValidSize,
            width,
            height);

    // ───────────────── Creation ─────────────────

    [Fact]
    public void Create_ValidInputs_ReturnsInstance()
    {
        var meta = CreateValid(width: 800, height: 600);

        meta.OriginalFileName.Should().Be(ValidFileName);
        meta.ContentType.Should().Be(ValidContentType);
        meta.FileSizeBytes.Should().Be(ValidSize);
        meta.WidthPx.Should().Be(800);
        meta.HeightPx.Should().Be(600);
    }

    [Fact]
    public void Create_NullDimensions_IsAllowed()
    {
        var meta = CreateValid();

        meta.WidthPx.Should().BeNull();
        meta.HeightPx.Should().BeNull();
    }

    [Fact]
    public void Create_TrimsFileNameAndContentType()
    {
        var meta = ImageMetadata.Create("  photo.jpg  ", "  image/jpeg  ", ValidSize);

        meta.OriginalFileName.Should().Be("photo.jpg");
        meta.ContentType.Should().Be("image/jpeg");
    }

    // ───────────────── FileName validation ─────────────────

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_NullOrEmptyFileName_Throws(string? fileName)
    {
        var act = () => ImageMetadata.Create(fileName!, ValidContentType, ValidSize);

        act.Should().Throw<ValidationException>();
    }

    [Fact]
    public void Create_FileNameTooLong_Throws()
    {
        var longName = new string('a', 261);

        var act = () => ImageMetadata.Create(longName, ValidContentType, ValidSize);

        act.Should().Throw<ValidationException>();
    }

    // ───────────────── ContentType validation ─────────────────

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_NullOrEmptyContentType_Throws(string? contentType)
    {
        var act = () => ImageMetadata.Create(ValidFileName, contentType!, ValidSize);

        act.Should().Throw<ValidationException>();
    }

    [Theory]
    [InlineData("image/jpeg")]
    [InlineData("image/png")]
    [InlineData("image/webp")]
    [InlineData("image/gif")]
    [InlineData("image/svg+xml")]
    public void Create_AllowedContentTypes_DoNotThrow(string contentType)
    {
        var act = () => ImageMetadata.Create(ValidFileName, contentType, ValidSize);

        act.Should().NotThrow();
    }

    [Theory]
    [InlineData("image/bmp")]
    [InlineData("application/pdf")]
    [InlineData("text/plain")]
    public void Create_DisallowedContentType_Throws(string contentType)
    {
        var act = () => ImageMetadata.Create(ValidFileName, contentType, ValidSize);

        act.Should().Throw<ValidationException>()
           .WithMessage("*not allowed*");
    }

    // ───────────────── FileSize validation ─────────────────

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Create_ZeroOrNegativeFileSize_Throws(long size)
    {
        var act = () => ImageMetadata.Create(ValidFileName, ValidContentType, size);

        act.Should().Throw<ValidationException>()
           .WithMessage("*greater than zero*");
    }

    [Fact]
    public void Create_ExceedsMaxFileSize_Throws()
    {
        var tooBig = ImageMetadata.MaxFileSizeBytes + 1;

        var act = () => ImageMetadata.Create(ValidFileName, ValidContentType, tooBig);

        act.Should().Throw<ValidationException>()
           .WithMessage("*must not exceed*");
    }

    [Fact]
    public void Create_ExactMaxFileSize_DoesNotThrow()
    {
        var act = () => ImageMetadata.Create(ValidFileName, ValidContentType, ImageMetadata.MaxFileSizeBytes);

        act.Should().NotThrow();
    }

    // ───────────────── Dimension validation ─────────────────

    [Fact]
    public void Create_ZeroWidth_Throws()
    {
        var act = () => CreateValid(width: 0);

        act.Should().Throw<ValidationException>()
           .WithMessage("*WidthPx*positive*");
    }

    [Fact]
    public void Create_NegativeWidth_Throws()
    {
        var act = () => CreateValid(width: -1);

        act.Should().Throw<ValidationException>();
    }

    [Fact]
    public void Create_ZeroHeight_Throws()
    {
        var act = () => CreateValid(height: 0);

        act.Should().Throw<ValidationException>()
           .WithMessage("*HeightPx*positive*");
    }

    [Fact]
    public void Create_NegativeHeight_Throws()
    {
        var act = () => CreateValid(height: -1);

        act.Should().Throw<ValidationException>();
    }

    // ───────────────── Value equality ─────────────────

    [Fact]
    public void Equality_SameValues_AreEqual()
    {
        var a = ImageMetadata.Create(ValidFileName, ValidContentType, ValidSize, 800, 600);
        var b = ImageMetadata.Create(ValidFileName, ValidContentType, ValidSize, 800, 600);

        a.Should().Be(b);
    }

    [Fact]
    public void Equality_DifferentFileName_NotEqual()
    {
        var a = ImageMetadata.Create("a.jpg", ValidContentType, ValidSize);
        var b = ImageMetadata.Create("b.jpg", ValidContentType, ValidSize);

        a.Should().NotBe(b);
    }
}
