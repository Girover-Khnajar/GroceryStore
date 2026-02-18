using CQRS.Abstractions.Messaging;
using GroceryStore.Application.Products.Dtos;

namespace GroceryStore.Application.Products.Queries;

public sealed record GetProductBySlugQuery(string Slug) : IQuery<ProductDto>;
