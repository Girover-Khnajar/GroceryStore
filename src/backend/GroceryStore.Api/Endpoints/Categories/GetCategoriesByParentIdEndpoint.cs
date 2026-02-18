using CQRS.Abstractions.Messaging;
using GroceryStore.Api.Extensions;
using GroceryStore.Application.Categories.Dtos;
using GroceryStore.Application.Categories.Queries;

using GroceryStore.Api.Endpoints;

namespace GroceryStore.Api.Endpoints.Categories;

public class GetCategoriesByParentIdEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/categories/by-parent/{parentId:guid?}", Handle)
            .WithName("GetCategoriesByParentId")
            .WithTags("Categories")
            .Produces(200);
    }

    private static async Task<IResult> Handle(Guid? parentId, IMessageDispatcher dispatcher)
    {
        var result = await dispatcher.QueryAsync<GetCategoriesByParentIdQuery, IReadOnlyList<CategoryDto>>(
            new GetCategoriesByParentIdQuery(parentId));

        return result.ToHttpResult();
    }
}
