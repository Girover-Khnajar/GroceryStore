using CQRS.Abstractions.Messaging;
using GroceryStore.Api.Extensions;
using GroceryStore.Application.Products.Dtos;
using GroceryStore.Application.Products.Queries;

using GroceryStore.Api.Endpoints;

namespace GroceryStore.Api.Endpoints.Products;

public class GetProductsByCategoryIdEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/products/by-category/{categoryId:guid}", Handle)
            .WithName("GetProductsByCategoryId")
            .WithTags("Products")
            .Produces(200);
    }

    private static async Task<IResult> Handle(Guid categoryId, IMessageDispatcher dispatcher)
    {
        var result = await dispatcher.QueryAsync<GetProductsByCategoryIdQuery, IReadOnlyList<ProductDto>>(
            new GetProductsByCategoryIdQuery(categoryId));

        return result.ToHttpResult();
    }
}
