using GroceryStore.Domain.Exceptions;

namespace GroceryStore.Domain.ValueObjects;

/// <summary>
/// Metadata about a stored image asset.
/// Immutable value object capturing technical and descriptive information about the file.
/// </summary>
public sealed record ImageMetadata
{
    /// <summary>Original file name as uploaded.</summary>
    public string OriginalFileName { get; }

    /// <summary>MIME content type (e.g. "image/jpeg", "image/png", "image/webp").</summary>
    public string ContentType { get; }

    /// <summary>File size in bytes.</summary>
    public long FileSizeBytes { get; }

    /// <summary>Image width in pixels (optional, may be null for SVG).</summary>
    public int? WidthPx { get; }

    /// <summary>Image height in pixels (optional, may be null for SVG).</summary>
    public int? HeightPx { get; }

    public const long MaxFileSizeBytes = 10 * 1024 * 1024; // 10 MB

    private static readonly HashSet<string> AllowedContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/jpeg",
        "image/png",
        "image/webp",
        "image/gif",
        "image/svg+xml"
    };

    private ImageMetadata(string originalFileName, string contentType, long fileSizeBytes, int? widthPx, int? heightPx)
    {
        ValidationException.ThrowIfNullOrWhiteSpace(originalFileName);

        ValidationException.ThrowIfTooLong(originalFileName, maxLen: 260);
        ValidationException.ThrowIfNullOrWhiteSpace(contentType);

        ValidationException.ThrowIfTooLong(contentType, maxLen: 100);
        OriginalFileName = originalFileName.Trim();
        ContentType = contentType.Trim();

        if (!AllowedContentTypes.Contains(ContentType))
            throw new ValidationException(
                $"ContentType '{contentType}' is not allowed. Allowed: {string.Join(", ", AllowedContentTypes)}.");

        if (fileSizeBytes <= 0)
            throw new ValidationException("FileSizeBytes must be greater than zero.");
        if (fileSizeBytes > MaxFileSizeBytes)
            throw new ValidationException($"File size must not exceed {MaxFileSizeBytes / (1024 * 1024)} MB.");

        if (widthPx.HasValue && widthPx.Value <= 0)
            throw new ValidationException("WidthPx must be positive.");

        if (heightPx.HasValue && heightPx.Value <= 0)
            throw new ValidationException("HeightPx must be positive.");

        FileSizeBytes = fileSizeBytes;
        WidthPx = widthPx;
        HeightPx = heightPx;
    }

    public static ImageMetadata Create(
        string originalFileName,
        string contentType,
        long fileSizeBytes,
        int? widthPx = null,
        int? heightPx = null)
        => new(originalFileName, contentType, fileSizeBytes, widthPx, heightPx);
}
