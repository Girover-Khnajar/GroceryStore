using CQRS.Abstractions.Messaging;
using GroceryStore.Application.Categories.Dtos;

namespace GroceryStore.Application.Categories.Queries;

public sealed record GetCategoriesByParentIdQuery(Guid? ParentCategoryId) : IQuery<IReadOnlyList<CategoryDto>>;
