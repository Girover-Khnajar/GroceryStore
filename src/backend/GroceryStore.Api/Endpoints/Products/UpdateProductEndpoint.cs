using CQRS.Abstractions.Messaging;
using GroceryStore.Api.Contracts.Products;
using GroceryStore.Api.Extensions;
using GroceryStore.Application.Products.Commands;

using GroceryStore.Api.Endpoints;

namespace GroceryStore.Api.Endpoints.Products;

public class UpdateProductEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder app)
    {
        app.MapPut("/api/products/{id:guid}", Handle)
            .WithName("UpdateProduct")
            .WithTags("Products")
            .Produces(204)
            .ProducesProblem(400)
            .ProducesProblem(404)
            .ProducesProblem(409)
            .ProducesProblem(422);
    }

    private static async Task<IResult> Handle(
        Guid id,
        UpdateProductRequest request,
        IMessageDispatcher dispatcher)
    {
        var command = new UpdateProductCommand(
            id,
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

        var result = await dispatcher.SendAsync(command);

        return result.ToHttpResult();
    }
}
