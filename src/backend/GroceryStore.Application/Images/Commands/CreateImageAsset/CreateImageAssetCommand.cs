using CQRS.Abstractions.Messaging;

namespace GroceryStore.Application.Images.Commands;

public sealed record CreateImageAssetCommand(
    string StoragePath,
    string Url,
    string FileName,
    string ContentType,
    long FileSizeBytes,
    int Width,
    int Height,
    string? AltText = null) : ICommand<Guid>;
