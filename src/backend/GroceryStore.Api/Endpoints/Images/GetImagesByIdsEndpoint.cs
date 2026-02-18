using CQRS.Abstractions.Messaging;
using GroceryStore.Api.Extensions;
using GroceryStore.Application.Images.Dtos;
using GroceryStore.Application.Images.Queries;

using GroceryStore.Api.Endpoints;

namespace GroceryStore.Api.Endpoints.Images;

public class GetImagesByIdsEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/images/batch", Handle)
            .WithName("GetImagesByIds")
            .WithTags("Images")
            .Produces(200);
    }

    private static async Task<IResult> Handle(
        [AsParameters] ImagesByIdsRequest request,
        IMessageDispatcher dispatcher)
    {
        var ids = request.Ids ?? [];
        var result = await dispatcher.QueryAsync<GetImagesByIdsQuery, IReadOnlyList<ImageAssetDto>>(
            new GetImagesByIdsQuery(ids.ToList().AsReadOnly()));

        return result.ToHttpResult();
    }
}

public sealed record ImagesByIdsRequest([Microsoft.AspNetCore.Mvc.FromQuery] Guid[]? Ids);
