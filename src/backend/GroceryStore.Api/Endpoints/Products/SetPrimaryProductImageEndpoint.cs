using CQRS.Abstractions.Messaging;
using GroceryStore.Api.Extensions;
using GroceryStore.Application.Products.Commands;

namespace GroceryStore.Api.Endpoints.Products;

public class SetPrimaryProductImageEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder app)
    {
        app.MapPut("/api/products/{id:guid}/images/{imageId:guid}/primary", Handle)
            .WithName("SetPrimaryProductImage")
            .WithTags("Products")
            .Produces(204)
            .ProducesProblem(404)
            .ProducesProblem(422);
    }

    private static async Task<IResult> Handle(
        Guid id,
        Guid imageId,
        IMessageDispatcher dispatcher)
    {
        var result = await dispatcher.SendAsync(new SetPrimaryProductImageCommand(id, imageId));
        return result.ToHttpResult();
    }
}
