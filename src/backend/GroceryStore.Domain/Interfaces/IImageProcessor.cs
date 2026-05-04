namespace GroceryStore.Domain.Interfaces;

/// <summary>
/// Service for image processing operations including thumbnail generation.
/// </summary>
public interface IImageProcessor
{
    /// <summary>
    /// Processes an uploaded image and generates a thumbnail.
    /// </summary>
    /// <param name="sourceStream">The source image stream</param>
    /// <param name="originalPhysicalPath">Physical path where the original image is saved</param>
    /// <param name="originalFileName">Original filename</param>
    /// <param name="contentType">Image content type</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Result containing the thumbnail path relative to wwwroot, or null if generation failed</returns>
    Task<string?> GenerateThumbnailAsync(
        Stream sourceStream,
        string originalPhysicalPath,
        string originalFileName,
        string contentType,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the thumbnail path for a given original image path.
    /// </summary>
    /// <param name="originalPath">Original image path (relative to wwwroot)</param>
    /// <returns>Thumbnail path or null if not applicable</returns>
    string? GetThumbnailPath(string originalPath);
}
