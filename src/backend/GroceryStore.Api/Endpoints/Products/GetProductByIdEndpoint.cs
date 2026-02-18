using CQRS.Abstractions.Messaging;
using GroceryStore.Api.Extensions;
using GroceryStore.Application.Products.Dtos;
using GroceryStore.Application.Products.Queries;

using GroceryStore.Api.Endpoints;

namespace GroceryStore.Api.Endpoints.Products;

public class GetProductByIdEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/products/{id:guid}", Handle)
            .WithName("GetProductById")
            .WithTags("Products")
            .Produces(200)
            .ProducesProblem(404);
    }

    private static async Task<IResult> Handle(Guid id, IMessageDispatcher dispatcher)
    {
        var result = await dispatcher.QueryAsync<GetProductByIdQuery, ProductDto>(
            new GetProductByIdQuery(id));

        return result.ToHttpResult();
    }
}
