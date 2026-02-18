using CQRS.Abstractions.Messaging;
using GroceryStore.Api.Contracts.Categories;
using GroceryStore.Api.Extensions;
using GroceryStore.Application.Categories.Commands;

using GroceryStore.Api.Endpoints;

namespace GroceryStore.Api.Endpoints.Categories;

public class CreateCategoryEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/categories", Handle)
            .WithName("CreateCategory")
            .WithTags("Categories")
            .Produces(201)
            .ProducesProblem(400)
            .ProducesProblem(409)
            .ProducesProblem(422);
    }

    private static async Task<IResult> Handle(
        CreateCategoryRequest request,
        IMessageDispatcher dispatcher)
    {
        var command = new CreateCategoryCommand(
            request.Name,
            request.Slug,
            request.SortOrder,
            request.ParentCategoryId,
            request.Description,
            request.ImageUrl,
            request.IconName,
            request.SeoMetaTitle,
            request.SeoMetaDescription);

        var result = await dispatcher.SendAsync<CreateCategoryCommand, Guid>(command);

        return result.ToCreatedHttpResult("GetCategoryById");
    }
}
