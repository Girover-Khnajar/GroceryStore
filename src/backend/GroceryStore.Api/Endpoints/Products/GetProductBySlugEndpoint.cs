using CQRS.Abstractions.Messaging;
using GroceryStore.Api.Extensions;
using GroceryStore.Application.Products.Dtos;
using GroceryStore.Application.Products.Queries;

using GroceryStore.Api.Endpoints;

namespace GroceryStore.Api.Endpoints.Products;

public class GetProductBySlugEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/products/by-slug/{slug}", Handle)
            .WithName("GetProductBySlug")
            .WithTags("Products")
            .Produces(200)
            .ProducesProblem(404);
    }

    private static async Task<IResult> Handle(string slug, IMessageDispatcher dispatcher)
    {
        var result = await dispatcher.QueryAsync<GetProductBySlugQuery, ProductDto>(
            new GetProductBySlugQuery(slug));

        return result.ToHttpResult();
    }
}
