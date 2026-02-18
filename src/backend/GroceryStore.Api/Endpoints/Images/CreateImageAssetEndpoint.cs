using CQRS.Abstractions.Messaging;
using GroceryStore.Api.Contracts.Images;
using GroceryStore.Api.Extensions;
using GroceryStore.Application.Images.Commands;

using GroceryStore.Api.Endpoints;

namespace GroceryStore.Api.Endpoints.Images;

public class CreateImageAssetEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/images", Handle)
            .WithName("CreateImageAsset")
            .WithTags("Images")
            .Produces(201)
            .ProducesProblem(400)
            .ProducesProblem(422);
    }

    private static async Task<IResult> Handle(
        CreateImageAssetRequest request,
        IMessageDispatcher dispatcher)
    {
        var command = new CreateImageAssetCommand(
            request.StoragePath,
            request.Url,
            request.FileName,
            request.ContentType,
            request.FileSizeBytes,
            request.Width,
            request.Height,
            request.AltText);

        var result = await dispatcher.SendAsync<CreateImageAssetCommand, Guid>(command);

        return result.ToCreatedHttpResult("GetImageById");
    }
}
