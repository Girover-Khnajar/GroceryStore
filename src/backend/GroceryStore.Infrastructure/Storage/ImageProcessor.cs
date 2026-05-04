using GroceryStore.Domain.Interfaces;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Formats;

namespace GroceryStore.Infrastructure.Storage;

public sealed class ImageProcessor : IImageProcessor
{
    private const int ThumbnailMaxWidth = 300;
    private const int ThumbnailMaxHeight = 300;
    private const string ThumbnailFolder = "thumbnails";

    public async Task<string?> GenerateThumbnailAsync(
        Stream sourceStream,
        string originalPhysicalPath,
        string originalFileName,
        string contentType,
        CancellationToken cancellationToken = default)
    {
        // SVG files don't need thumbnails
        if (contentType.Equals("image/svg+xml", StringComparison.OrdinalIgnoreCase))
            return null;

        try
        {
            // Reset stream position
            if (sourceStream.CanSeek)
                sourceStream.Position = 0;

            // Load the image
            using var image = await Image.LoadAsync(sourceStream, cancellationToken);

            // Calculate thumbnail dimensions maintaining aspect ratio
            var scale = Math.Min(
                (double)ThumbnailMaxWidth / image.Width,
                (double)ThumbnailMaxHeight / image.Height);

            // Only create thumbnail if image is larger than thumbnail size
            if (scale >= 1.0)
                return null; // Image is already small enough

            var thumbnailWidth = (int)(image.Width * scale);
            var thumbnailHeight = (int)(image.Height * scale);

            // Generate thumbnail path in dedicated folder
            // Original: wwwroot/images/uploads/2026/05/abc123.jpg
            // Thumbnail: wwwroot/images/thumbnails/2026/05/abc123.jpg
            var thumbnailPhysicalPath = GetThumbnailPhysicalPath(originalPhysicalPath);
            var thumbnailDir = Path.GetDirectoryName(thumbnailPhysicalPath)!;
            
            // Create thumbnail directory if it doesn't exist
            Directory.CreateDirectory(thumbnailDir);

            // Resize and save thumbnail
            image.Mutate(x => x.Resize(new ResizeOptions
            {
                Size = new Size(thumbnailWidth, thumbnailHeight),
                Mode = ResizeMode.Max,
                Sampler = KnownResamplers.Lanczos3
            }));

            // Save with appropriate encoder and quality
            IImageEncoder encoder = contentType.ToLowerInvariant() switch
            {
                "image/png" => new PngEncoder(),
                "image/webp" => new WebpEncoder { Quality = 85 },
                _ => new JpegEncoder { Quality = 85 }
            };

            await image.SaveAsync(thumbnailPhysicalPath, encoder, cancellationToken);

            // Return relative path (replace backslashes for web URLs)
            var relativePath = GetThumbnailRelativePath(originalPhysicalPath);
            return relativePath;
        }
        catch (Exception)
        {
            // If thumbnail generation fails, return null (app will use original)
            return null;
        }
    }

    public string? GetThumbnailPath(string originalPath)
    {
        if (string.IsNullOrWhiteSpace(originalPath))
            return null;

        // Check if it's an SVG
        if (originalPath.EndsWith(".svg", StringComparison.OrdinalIgnoreCase))
            return null;

        // Convert: /images/uploads/2026/05/abc123.jpg
        // To:      /images/thumbnails/2026/05/abc123.jpg
        return GetThumbnailRelativePathFromOriginal(originalPath);
    }

    private static string GetThumbnailPhysicalPath(string originalPhysicalPath)
    {
        // Convert: wwwroot/images/uploads/2026/05/abc123.jpg
        // To:      wwwroot/images/thumbnails/2026/05/abc123.jpg
        
        var parts = originalPhysicalPath.Split(Path.DirectorySeparatorChar);
        var imagesIndex = Array.FindIndex(parts, p => p.Equals("images", StringComparison.OrdinalIgnoreCase));
        
        if (imagesIndex < 0)
            return originalPhysicalPath; // Fallback
        
        var uploadsIndex = Array.FindIndex(parts, imagesIndex, p => p.Equals("uploads", StringComparison.OrdinalIgnoreCase));
        
        if (uploadsIndex < 0)
            return originalPhysicalPath; // Fallback
        
        // Replace "uploads" with "thumbnails"
        parts[uploadsIndex] = ThumbnailFolder;
        
        return string.Join(Path.DirectorySeparatorChar.ToString(), parts);
    }

    private static string GetThumbnailRelativePath(string originalPhysicalPath)
    {
        // Convert: wwwroot/images/uploads/2026/05/abc123.jpg
        // To:      /images/thumbnails/2026/05/abc123.jpg
        
        var parts = originalPhysicalPath.Split(Path.DirectorySeparatorChar);
        var imagesIndex = Array.FindIndex(parts, p => p.Equals("images", StringComparison.OrdinalIgnoreCase));
        
        if (imagesIndex < 0)
            return "/" + Path.GetFileName(originalPhysicalPath); // Fallback
        
        var uploadsIndex = Array.FindIndex(parts, imagesIndex, p => p.Equals("uploads", StringComparison.OrdinalIgnoreCase));
        
        if (uploadsIndex < 0)
            return "/" + Path.GetFileName(originalPhysicalPath); // Fallback
        
        // Replace "uploads" with "thumbnails"
        parts[uploadsIndex] = ThumbnailFolder;
        
        // Build relative path from "images" onward
        var relativeParts = parts[imagesIndex..];
        return "/" + string.Join("/", relativeParts);
    }

    private static string GetThumbnailRelativePathFromOriginal(string originalRelativePath)
    {
        // Convert: /images/uploads/2026/05/abc123.jpg
        // To:      /images/thumbnails/2026/05/abc123.jpg
        
        if (string.IsNullOrWhiteSpace(originalRelativePath))
            return originalRelativePath;
        
        return originalRelativePath.Replace("/uploads/", $"/{ThumbnailFolder}/", StringComparison.OrdinalIgnoreCase)
                                   .Replace("\\uploads\\", $"\\{ThumbnailFolder}\\", StringComparison.OrdinalIgnoreCase);
    }
}
