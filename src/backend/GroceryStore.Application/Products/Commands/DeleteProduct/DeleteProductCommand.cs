using CQRS.Abstractions.Messaging;

namespace GroceryStore.Application.Products.Commands;

public sealed record DeleteProductCommand(Guid Id) : ICommand;
