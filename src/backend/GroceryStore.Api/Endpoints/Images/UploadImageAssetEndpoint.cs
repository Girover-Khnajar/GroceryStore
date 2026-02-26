using GroceryStore.Api.Contracts.Images;
using Microsoft.AspNetCore.Mvc;
using GroceryStore.Api.Services.Images;
using GroceryStore.Api.Extensions;
using CQRS.Abstractions.Messaging;
using GroceryStore.Application.Images.Commands;

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
        [FromForm] IFormFile File,
        [FromForm] string? AltText,
        IMessageDispatcher dispatcher,
        IImageUploadService uploadService,
        CancellationToken ct)
    {
        // CQRS Command
        var uploadResult = await uploadService.UploadAsync(httpRequest,
            new UploadImageRequest
            {
                File = File,
                AltText = AltText
            },
            ct
        );
        if (uploadResult.IsFailure)
        {
            var error = uploadResult.Errors[0];
            int statusCode = StatusCodes.Status400BadRequest;
            if (error.Metadata != null && error.Metadata.TryGetValue("statusCode", out var statusCodeObj) && statusCodeObj is int code)
            {
                statusCode = code;
            }

            return Results.Problem(
                title: error.Message,
                statusCode: statusCode);
        }

        var command = new CreateImageAssetCommand(
            uploadResult.Value!.StoragePath,
            uploadResult.Value!.Url,
            uploadResult.Value!.FileName,
            uploadResult.Value!.ContentType,
            uploadResult.Value!.FileSizeBytes,
            uploadResult.Value!.Width,
            uploadResult.Value!.Height,
            AltText
            );

        var result = await dispatcher.SendAsync<CreateImageAssetCommand, Guid>(command, ct);

        // assumes result contains created ImageAsset DTO + routeName "GetImageById"
        return result.ToCreatedHttpResult("GetImageById");
    }
}
