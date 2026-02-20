using CQRS.Abstractions.Messaging;
using GroceryStore.Api.Extensions;
using GroceryStore.Application.Products.Commands;

namespace GroceryStore.Api.Endpoints.Products;

public class RemoveProductImageEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder app)
    {
        app.MapDelete("/api/products/{id:guid}/images/{imageId:guid}", Handle)
            .WithName("RemoveProductImage")
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
        var result = await dispatcher.SendAsync(new RemoveProductImageCommand(id, imageId));
        return result.ToHttpResult();
    }
}
