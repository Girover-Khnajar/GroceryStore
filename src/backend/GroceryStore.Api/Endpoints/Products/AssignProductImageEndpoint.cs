using CQRS.Abstractions.Messaging;
using GroceryStore.Api.Contracts.Products;
using GroceryStore.Api.Extensions;
using GroceryStore.Application.Products.Commands.AssignImageAssetToProduct;

namespace GroceryStore.Api.Endpoints.Products;

public class AssignProductImageEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/products/{id:guid}/images", Handle)
            .WithName("AssignProductImage")
            .WithTags("Products")
            .Produces(204)
            .ProducesProblem(400)
            .ProducesProblem(404)
            .ProducesProblem(409)
            .ProducesProblem(422);
    }

    private static async Task<IResult> Handle(
        Guid id,
        AssignProductImageRequest request,
        IMessageDispatcher dispatcher)
    {
        var command = new AssignImageAssetToProductCommand(
            id,
            request.ImageId,
            request.MakePrimary,
            request.SortOrder,
            request.AltText);

        var result = await dispatcher.SendAsync(command);

        return result.ToHttpResult();
    }
}
