using CQRS.Abstractions.Messaging;
using GroceryStore.Api.Contracts.Categories;
using GroceryStore.Api.Extensions;
using GroceryStore.Application.Categories.Commands;

using GroceryStore.Api.Endpoints;

namespace GroceryStore.Api.Endpoints.Categories;

public class UpdateCategoryEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder app)
    {
        app.MapPut("/api/categories/{id:guid}", Handle)
            .WithName("UpdateCategory")
            .WithTags("Categories")
            .Produces(204)
            .ProducesProblem(400)
            .ProducesProblem(404)
            .ProducesProblem(409)
            .ProducesProblem(422);
    }

    private static async Task<IResult> Handle(
        Guid id,
        UpdateCategoryRequest request,
        IMessageDispatcher dispatcher)
    {
        var command = new UpdateCategoryCommand(
            id,
            request.Name,
            request.Slug,
            request.SortOrder,
            request.ParentCategoryId,
            request.Description,
            request.ImageUrl,
            request.IconName,
            request.SeoMetaTitle,
            request.SeoMetaDescription);

        var result = await dispatcher.SendAsync(command);

        return result.ToHttpResult();
    }
}
