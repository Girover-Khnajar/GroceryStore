using CQRS.Abstractions.Messaging;

namespace GroceryStore.Application.Images.Commands;

public sealed record UpdateImageAltTextCommand(Guid ImageId, string? AltText) : ICommand;
