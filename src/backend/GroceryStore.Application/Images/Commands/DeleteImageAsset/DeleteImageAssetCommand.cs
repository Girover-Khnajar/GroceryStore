using CQRS.Abstractions.Messaging;

namespace GroceryStore.Application.Images.Commands;

public sealed record DeleteImageAssetCommand(Guid ImageId) : ICommand;
