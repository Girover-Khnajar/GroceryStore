using CQRS.Abstractions.Messaging;
using GroceryStore.Api.Extensions;
using GroceryStore.Application.Categories.Commands;

using GroceryStore.Api.Endpoints;

namespace GroceryStore.Api.Endpoints.Categories;

public class DeleteCategoryEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder app)
    {
        app.MapDelete("/api/categories/{id:guid}", Handle)
            .WithName("DeleteCategory")
            .WithTags("Categories")
            .Produces(204)
            .ProducesProblem(404);
    }

    private static async Task<IResult> Handle(Guid id, IMessageDispatcher dispatcher)
    {
        var result = await dispatcher.SendAsync(new DeleteCategoryCommand(id));

        return result.ToHttpResult();
    }
}
