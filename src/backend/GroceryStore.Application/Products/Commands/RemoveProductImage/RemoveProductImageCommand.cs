using CQRS.Abstractions.Messaging;

namespace GroceryStore.Application.Products.Commands.RemoveProductImage;

public sealed record RemoveProductImageCommand(
    Guid ProductId,
    Guid ImageId) : ICommand;
