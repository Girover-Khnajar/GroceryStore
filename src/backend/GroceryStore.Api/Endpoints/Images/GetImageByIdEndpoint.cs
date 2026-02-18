using CQRS.Abstractions.Messaging;
using GroceryStore.Api.Extensions;
using GroceryStore.Application.Images.Dtos;
using GroceryStore.Application.Images.Queries;

using GroceryStore.Api.Endpoints;

namespace GroceryStore.Api.Endpoints.Images;

public class GetImageByIdEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/images/{id:guid}", Handle)
            .WithName("GetImageById")
            .WithTags("Images")
            .Produces(200)
            .ProducesProblem(404);
    }

    private static async Task<IResult> Handle(Guid id, IMessageDispatcher dispatcher)
    {
        var result = await dispatcher.QueryAsync<GetImageByIdQuery, ImageAssetDto>(
            new GetImageByIdQuery(id));

        return result.ToHttpResult();
    }
}
