using GroceryStore.Domain.Interfaces;

namespace GroceryStore.Web.Services;

/// <summary>
/// Helper service for image URL operations including thumbnail path resolution.
/// Can be injected into views using @inject GroceryStore.Web.Services.ImageUrlHelper ImageHelper
/// </summary>
public class ImageUrlHelper
{
    private readonly IImageProcessor _imageProcessor;
    private readonly IWebHostEnvironment? _env;

    public ImageUrlHelper(IImageProcessor imageProcessor, IWebHostEnvironment env)
    {
        _imageProcessor = imageProcessor;
        _env = env;
    }

    /// <summary>
    /// Gets the thumbnail URL for a given image URL if the thumbnail exists, otherwise returns the original URL.
    /// Use this when you want to display a thumbnail if available, falling back to the original.
    /// </summary>
    public string GetThumbnailUrlOrOriginal(string? imageUrl)
    {
        if (string.IsNullOrWhiteSpace(imageUrl))
            return string.Empty;

        var normalizedImagePath = NormalizeToRelativePath(imageUrl);
        var thumbnailUrl = _imageProcessor.GetThumbnailPath(normalizedImagePath);
        
        // Check if thumbnail exists
        if (!string.IsNullOrWhiteSpace(thumbnailUrl) && ThumbnailPhysicallyExists(thumbnailUrl))
            return thumbnailUrl;

        return imageUrl;
    }

    /// <summary>
    /// Gets the thumbnail URL for a given image URL, or null if not available.
    /// </summary>
    public string? GetThumbnailUrl(string? imageUrl)
    {
        if (string.IsNullOrWhiteSpace(imageUrl))
            return null;

        var normalizedImagePath = NormalizeToRelativePath(imageUrl);
        var thumbnailUrl = _imageProcessor.GetThumbnailPath(normalizedImagePath);
        
        if (!string.IsNullOrWhiteSpace(thumbnailUrl) && ThumbnailPhysicallyExists(thumbnailUrl))
            return thumbnailUrl;

        return null;
    }

    private bool ThumbnailPhysicallyExists(string thumbnailUrl)
    {
        if (_env == null)
            return false;

        var normalizedThumbnailPath = NormalizeToRelativePath(thumbnailUrl);
        var relativePath = normalizedThumbnailPath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
        var physicalPath = Path.Combine(_env.WebRootPath, relativePath);
        return File.Exists(physicalPath);
    }

    private static string NormalizeToRelativePath(string imagePath)
    {
        if (string.IsNullOrWhiteSpace(imagePath))
            return string.Empty;

        var normalizedPath = imagePath;

        if (Uri.TryCreate(imagePath, UriKind.Absolute, out var absoluteUri))
        {
            normalizedPath = absoluteUri.AbsolutePath;
        }

        var queryIndex = normalizedPath.IndexOfAny(['?', '#']);
        if (queryIndex >= 0)
        {
            normalizedPath = normalizedPath[..queryIndex];
        }

        normalizedPath = normalizedPath.Replace('\\', '/');

        if (!normalizedPath.StartsWith('/'))
        {
            normalizedPath = "/" + normalizedPath.TrimStart('/');
        }

        return normalizedPath;
    }
}
