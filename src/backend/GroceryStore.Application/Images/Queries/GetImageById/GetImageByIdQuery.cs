using CQRS.Abstractions.Messaging;
using GroceryStore.Application.Images.Dtos;

namespace GroceryStore.Application.Images.Queries;

public sealed record GetImageByIdQuery(Guid ImageId) : IQuery<ImageAssetDto>;
