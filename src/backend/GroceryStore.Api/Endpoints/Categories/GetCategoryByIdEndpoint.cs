using CQRS.Abstractions.Messaging;
using GroceryStore.Api.Extensions;
using GroceryStore.Application.Categories.Dtos;
using GroceryStore.Application.Categories.Queries;

using GroceryStore.Api.Endpoints;

namespace GroceryStore.Api.Endpoints.Categories;

public class GetCategoryByIdEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/categories/{id:guid}", Handle)
            .WithName("GetCategoryById")
            .WithTags("Categories")
            .Produces(200)
            .ProducesProblem(404);
    }

    private static async Task<IResult> Handle(Guid id, IMessageDispatcher dispatcher)
    {
        var result = await dispatcher.QueryAsync<GetCategoryByIdQuery, CategoryDto>(
            new GetCategoryByIdQuery(id));

        return result.ToHttpResult();
    }
}
