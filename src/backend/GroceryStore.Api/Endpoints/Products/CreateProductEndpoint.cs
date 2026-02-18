using CQRS.Abstractions.Messaging;
using GroceryStore.Api.Contracts.Products;
using GroceryStore.Api.Extensions;
using GroceryStore.Application.Products.Commands;

using GroceryStore.Api.Endpoints;

namespace GroceryStore.Api.Endpoints.Products;

public class CreateProductEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/products", Handle)
            .WithName("CreateProduct")
            .WithTags("Products")
            .Produces(201)
            .ProducesProblem(400)
            .ProducesProblem(404)
            .ProducesProblem(409)
            .ProducesProblem(422);
    }

    private static async Task<IResult> Handle(
        CreateProductRequest request,
        IMessageDispatcher dispatcher)
    {
        var command = new CreateProductCommand(
            request.CategoryId,
            request.Name,
            request.Slug,
            request.PriceAmount,
            request.PriceCurrency,
            request.Unit,
            request.SortOrder,
            request.IsFeatured,
            request.ShortDescription,
            request.LongDescription,
            request.SeoMetaTitle,
            request.SeoMetaDescription);

        var result = await dispatcher.SendAsync<CreateProductCommand, Guid>(command);

        return result.ToCreatedHttpResult("GetProductById");
    }
}
