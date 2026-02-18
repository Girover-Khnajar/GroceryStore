using CQRS.Abstractions.Messaging;
using GroceryStore.Api.Extensions;
using GroceryStore.Application.Categories.Dtos;
using GroceryStore.Application.Categories.Queries;

using GroceryStore.Api.Endpoints;

namespace GroceryStore.Api.Endpoints.Categories;

public class GetCategoryBySlugEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/categories/by-slug/{slug}", Handle)
            .WithName("GetCategoryBySlug")
            .WithTags("Categories")
            .Produces(200)
            .ProducesProblem(404);
    }

    private static async Task<IResult> Handle(string slug, IMessageDispatcher dispatcher)
    {
        var result = await dispatcher.QueryAsync<GetCategoryBySlugQuery, CategoryDto>(
            new GetCategoryBySlugQuery(slug));

        return result.ToHttpResult();
    }
}
