using CQRS.Abstractions.Messaging;
using GroceryStore.Api.Extensions;
using GroceryStore.Application.Categories.Dtos;
using GroceryStore.Application.Categories.Queries;

using GroceryStore.Api.Endpoints;

namespace GroceryStore.Api.Endpoints.Categories;

public class GetAllActiveCategoriesEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/categories", Handle)
            .WithName("GetAllActiveCategories")
            .WithTags("Categories")
            .Produces(200)
            .ProducesProblem(500);
    }

    private static async Task<IResult> Handle(IMessageDispatcher dispatcher)
    {
        var result = await dispatcher.QueryAsync<GetAllActiveCategoriesQuery, IReadOnlyList<CategoryDto>>(
            new GetAllActiveCategoriesQuery());

        return result.ToHttpResult();
    }
}
