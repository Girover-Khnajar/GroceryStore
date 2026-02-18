using CQRS.Abstractions.Messaging;
using GroceryStore.Api.Extensions;
using GroceryStore.Application.Images.Commands;

using GroceryStore.Api.Endpoints;

namespace GroceryStore.Api.Endpoints.Images;

public class DeleteImageAssetEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder app)
    {
        app.MapDelete("/api/images/{id:guid}", Handle)
            .WithName("DeleteImageAsset")
            .WithTags("Images")
            .Produces(204)
            .ProducesProblem(404);
    }

    private static async Task<IResult> Handle(Guid id, IMessageDispatcher dispatcher)
    {
        var result = await dispatcher.SendAsync(new DeleteImageAssetCommand(id));

        return result.ToHttpResult();
    }
}
