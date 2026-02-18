using CQRS.Abstractions.Messaging;

namespace GroceryStore.Application.Categories.Commands;

public sealed record DeleteCategoryCommand(Guid Id) : ICommand;
