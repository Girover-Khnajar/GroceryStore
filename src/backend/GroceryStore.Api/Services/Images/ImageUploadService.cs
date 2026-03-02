using System.Globalization;
using CQRS.Abstractions.Messaging;
using CQRS.CqrsResult;
using GroceryStore.Api.Contracts.Images;
using GroceryStore.Application.Images.Commands;
using SixLabors.ImageSharp;

namespace GroceryStore.Api.Services.Images;

public sealed class ImageUploadService : IImageUploadService
{
    private const long MaxFileSizeBytes = 5 * 1024 * 1024; // 5 MB

    private static readonly HashSet<string> AllowedContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/jpeg",
        "image/png",
        "image/webp",
        "image/gif",
        "image/svg+xml"
    };

    private readonly IWebHostEnvironment _env;
    private readonly IMessageDispatcher _dispatcher;

    public ImageUploadService(IWebHostEnvironment env, IMessageDispatcher dispatcher)
    {
        _env = env;
        _dispatcher = dispatcher;
    }

    public async Task<Result<StoredImageDto>> UploadAsync(HttpRequest httpRequest, UploadImageRequest request, CancellationToken cancellationToken)
    {
        if (request.File is null)
            return Result<StoredImageDto>.Fail(Error.BadRequest("File is required."));

        if (request.File.Length <= 0)
            return Result<StoredImageDto>.Fail(Error.Validation("File is empty."));

        if (request.File.Length > MaxFileSizeBytes)
            return Result<StoredImageDto>.Fail(
                Error.BadRequest($"File size must not exceed {MaxFileSizeBytes / (1024 * 1024)} MB.")
                    .WithMetadata("statusCode", StatusCodes.Status413PayloadTooLarge));

        var contentType = request.File.ContentType?.Trim();
        if (string.IsNullOrWhiteSpace(contentType) || !AllowedContentTypes.Contains(contentType))
        {
            return Result<StoredImageDto>.Fail(Error.Validation($"ContentType '{request.File.ContentType}' is not allowed."));
        }

        // Buffer the file once so the stream can be used for both dimension detection and disk write
        using var memoryStream = new MemoryStream();
        await request.File.CopyToAsync(memoryStream, cancellationToken);
        memoryStream.Position = 0;

        var (width, height) = await TryGetDimensionsAsync(memoryStream, request.File.ContentType, cancellationToken);
        memoryStream.Position = 0;

        var extension = GetExtension(request.File.FileName, contentType);
        var now = DateTimeOffset.UtcNow;
        var yyyy = now.Year.ToString(CultureInfo.InvariantCulture);
        var mm = now.Month.ToString("00", CultureInfo.InvariantCulture);
        var safeFileName = $"{Guid.NewGuid():N}{extension}";

        var relativePath = Path.Combine("images", "uploads", yyyy, mm, safeFileName);
        var relativeUrlPath = relativePath.Replace('\\', '/');
        var dbStoragePath = "/" + relativeUrlPath;
        var physicalPath = Path.Combine(_env.WebRootPath, relativePath);

        Directory.CreateDirectory(Path.GetDirectoryName(physicalPath)!);

        try
        {
            await using var stream = File.Create(physicalPath);
            await memoryStream.CopyToAsync(stream, cancellationToken);

            var storedImage = new StoredImageDto(
                Id: Guid.NewGuid(), // This will be replaced by the actual ImageId from the database after the command is processed
                Url: $"{httpRequest.Scheme}://{httpRequest.Host}/{relativeUrlPath}",
                StoragePath: dbStoragePath,
                FileName: Path.GetFileName(request.File.FileName),
                ContentType: contentType,
                FileSizeBytes: request.File.Length,
                Width: width,
                Height: height,
                AltText: request.AltText,
                CreatedAtUtc: DateTime.UtcNow
            );
            return Result<StoredImageDto>.Ok(storedImage);
        }
        catch
        {
            return Result<StoredImageDto>.Fail(Error.Unexpected("Failed to store the uploaded file."));
        }
    }

    private static async Task<(int width, int height)> TryGetDimensionsAsync(Stream stream, string? contentType, CancellationToken cancellationToken)
    {
        if (string.Equals(contentType, "image/svg+xml", StringComparison.OrdinalIgnoreCase))
            return (0, 0);

        try
        {
            var info = await Image.IdentifyAsync(stream, cancellationToken);
            if (info is null)
                return (0, 0);

            return (info.Width, info.Height);
        }
        catch
        {
            return (0, 0);
        }
    }

    private static string GetExtension(string originalFileName, string contentType)
    {
        var ext = Path.GetExtension(originalFileName);
        if (!string.IsNullOrWhiteSpace(ext) && ext.Length <= 10)
            return ext;

        return contentType.ToLowerInvariant() switch
        {
            "image/jpeg" => ".jpg",
            "image/png" => ".png",
            "image/webp" => ".webp",
            "image/gif" => ".gif",
            "image/svg+xml" => ".svg",
            _ => string.Empty
        };
    }
}
