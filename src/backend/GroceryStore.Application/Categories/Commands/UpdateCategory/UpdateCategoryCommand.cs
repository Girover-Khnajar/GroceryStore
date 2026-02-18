using CQRS.Abstractions.Messaging;

namespace GroceryStore.Application.Categories.Commands;

public sealed record UpdateCategoryCommand(
    Guid Id,
    string Name,
    string Slug,
    int SortOrder,
    Guid? ParentCategoryId,
    string? Description,
    string? ImageUrl,
    string? IconName,
    string? SeoMetaTitle,
    string? SeoMetaDescription) : ICommand;
