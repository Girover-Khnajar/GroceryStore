using CQRS.Abstractions.Messaging;
using GroceryStore.Application.Products.Dtos;

namespace GroceryStore.Application.Products.Queries;

public sealed record GetProductsByCategoryIdQuery(Guid CategoryId) : IQuery<IReadOnlyList<ProductDto>>;
