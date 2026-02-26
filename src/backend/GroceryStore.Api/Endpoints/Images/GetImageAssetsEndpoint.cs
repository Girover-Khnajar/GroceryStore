using CQRS.Abstractions.Messaging;
using GroceryStore.Api.Contracts.Images;
using GroceryStore.Api.Extensions;
using GroceryStore.Application.Images.Dtos;
using GroceryStore.Application.Images.Queries.GetImages;

namespace GroceryStore.Api.Endpoints.Images;

public sealed class ListImageAssetsEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/images", Handle)
            .WithName("ListImageAssets")
            .WithTags("Images")
            .Produces<List<ImageAssetDto>>(200)
            .ProducesProblem(400);
    }

    private static async Task<IResult> Handle(
        string? search,
        IMessageDispatcher dispatcher,
        CancellationToken ct)
    {
        var query = new GetImagesQuery(search);
        var result = await dispatcher.QueryAsync<GetImagesQuery, List<ImageAssetDto>>(query, ct);
        return result.ToHttpResult(); // your extension maps Result<T> to 200/4xx
    }
}