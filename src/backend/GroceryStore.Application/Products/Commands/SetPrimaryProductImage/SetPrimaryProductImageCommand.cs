using CQRS.Abstractions.Messaging;

namespace GroceryStore.Application.Products.Commands.SetPrimaryProductImage;

public sealed record SetPrimaryProductImageCommand(
    Guid ProductId,
    Guid ImageId) : ICommand;
