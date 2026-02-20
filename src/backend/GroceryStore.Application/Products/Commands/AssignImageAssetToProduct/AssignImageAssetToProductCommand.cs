using CQRS.Abstractions.Messaging;

namespace GroceryStore.Application.Products.Commands.AssignImageAssetToProduct;

public sealed record AssignImageAssetToProductCommand(
    Guid ProductId,
    Guid ImageId,
    bool MakePrimary = false,
    int SortOrder = 0,
    string? AltText = null) : ICommand;
