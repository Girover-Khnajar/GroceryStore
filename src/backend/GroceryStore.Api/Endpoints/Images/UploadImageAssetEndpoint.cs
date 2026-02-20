using GroceryStore.Api.Contracts.Images;
using Microsoft.AspNetCore.Mvc;
using GroceryStore.Api.Services.Images;
using GroceryStore.Api.Extensions;

namespace GroceryStore.Api.Endpoints.Images;

public sealed class UploadImageAssetEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/images/upload", Handle)
            .WithName("UploadImageAsset")
            .WithTags("Images")
            .DisableAntiforgery()
            .Accepts<UploadImageRequest>("multipart/form-data")
            .Produces(201)
            .ProducesProblem(400)
            .ProducesProblem(413)
            .ProducesProblem(422);
    }

    private static async Task<IResult> Handle(
        HttpRequest httpRequest,
        [FromForm] UploadImageRequest request,
        IImageUploadService uploadService,
        CancellationToken cancellationToken)
    {
        var result = await uploadService.UploadAsync(httpRequest, request, cancellationToken);
        return result.ToCreatedHttpResult("GetImageById");
    }
}
