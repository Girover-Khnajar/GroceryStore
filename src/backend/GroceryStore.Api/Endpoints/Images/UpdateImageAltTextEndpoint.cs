using CQRS.Abstractions.Messaging;
using GroceryStore.Api.Contracts.Images;
using GroceryStore.Api.Extensions;
using GroceryStore.Application.Images.Commands;

using GroceryStore.Api.Endpoints;

namespace GroceryStore.Api.Endpoints.Images;

public class UpdateImageAltTextEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder app)
    {
        app.MapPut("/api/images/{id:guid}/alt-text", Handle)
            .WithName("UpdateImageAltText")
            .WithTags("Images")
            .Produces(204)
            .ProducesProblem(404);
    }

    private static async Task<IResult> Handle(
        Guid id,
        UpdateImageAltTextRequest request,
        IMessageDispatcher dispatcher)
    {
        var command = new UpdateImageAltTextCommand(id, request.AltText);
        var result = await dispatcher.SendAsync(command);

        return result.ToHttpResult();
    }
}
