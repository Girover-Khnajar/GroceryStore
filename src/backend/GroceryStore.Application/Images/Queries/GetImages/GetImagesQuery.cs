using CQRS.Abstractions.Messaging;
using GroceryStore.Application.Images.Dtos;

namespace GroceryStore.Application.Images.Queries.GetImages;

public sealed record GetImagesQuery(string? Search): IQuery<List<ImageAssetDto>>;
