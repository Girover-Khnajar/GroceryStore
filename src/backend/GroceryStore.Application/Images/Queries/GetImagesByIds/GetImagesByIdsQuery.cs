using CQRS.Abstractions.Messaging;
using GroceryStore.Application.Images.Dtos;

namespace GroceryStore.Application.Images.Queries;

public sealed record GetImagesByIdsQuery(IReadOnlyList<Guid> ImageIds) : IQuery<IReadOnlyList<ImageAssetDto>>;
