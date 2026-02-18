using CQRS.Abstractions.Messaging;

namespace GroceryStore.Application.Categories.Commands;

public sealed record CreateCategoryCommand(
    string Name,
    string Slug,
    int SortOrder = 0,
    Guid? ParentCategoryId = null,
    string? Description = null,
    string? ImageUrl = null,
    string? IconName = null,
    string? SeoMetaTitle = null,
    string? SeoMetaDescription = null) : ICommand<Guid>;
