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

    public async Task<Result<Guid>> UploadAsync(HttpRequest httpRequest, UploadImageRequest request, CancellationToken cancellationToken)
    {
        if (request.File is null)
            return Result<Guid>.Fail(Error.BadRequest("File is required."));

        if (request.File.Length <= 0)
            return Result<Guid>.Fail(Error.Validation("File is empty."));

        if (request.File.Length > MaxFileSizeBytes)
            return Result<Guid>.Fail(
                Error.BadRequest($"File size must not exceed {MaxFileSizeBytes / (1024 * 1024)} MB.")
                    .WithMetadata("statusCode", StatusCodes.Status413PayloadTooLarge));

        var contentType = request.File.ContentType?.Trim();
        if (string.IsNullOrWhiteSpace(contentType) || !AllowedContentTypes.Contains(contentType))
        {
            return Result<Guid>.Fail(Error.Validation($"ContentType '{request.File.ContentType}' is not allowed."));
        }

        var (width, height) = await TryGetDimensionsAsync(request.File, cancellationToken);

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
            await request.File.CopyToAsync(stream, cancellationToken);
        }
        catch
        {
            return Result<Guid>.Fail(Error.Unexpected("Failed to store the uploaded file."));
        }

        var url = $"{httpRequest.Scheme}://{httpRequest.Host}/{relativeUrlPath}";

        var command = new CreateImageAssetCommand(
            StoragePath: dbStoragePath,
            Url: url,
            FileName: Path.GetFileName(request.File.FileName),
            ContentType: contentType,
            FileSizeBytes: request.File.Length,
            Width: width,
            Height: height,
            AltText: request.AltText);

        return await _dispatcher.SendAsync<CreateImageAssetCommand, Guid>(command, cancellationToken);
    }

    private static async Task<(int width, int height)> TryGetDimensionsAsync(IFormFile file, CancellationToken cancellationToken)
    {
        if (string.Equals(file.ContentType, "image/svg+xml", StringComparison.OrdinalIgnoreCase))
            return (0, 0);

        try
        {
            await using var stream = file.OpenReadStream();
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
