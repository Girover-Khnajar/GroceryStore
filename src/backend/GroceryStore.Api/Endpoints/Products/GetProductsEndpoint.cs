using CQRS.Abstractions.Messaging;
using GroceryStore.Api.Contracts.Products;
using GroceryStore.Api.Extensions;
using GroceryStore.Application.Common;
using GroceryStore.Application.Products.Dtos;
using GroceryStore.Application.Products.Queries.GetProducts;
using Microsoft.AspNetCore.Mvc;

namespace GroceryStore.Api.Endpoints.Products;

public class GetProductsEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/products", Handle)
            .WithName("GetProducts")
            .WithTags("Products")
            .Produces(200)
            .ProducesProblem(400)
            .ProducesProblem(404)
            .ProducesProblem(409)
            .ProducesProblem(422);
    }

    private static async Task<IResult> Handle(
        [AsParameters] GetProductsRequest request,
        IMessageDispatcher dispatcher)
    {
        var query = new GetProductsQuery(
            request.Search,
            request.CategoryId,
            request.MinPrice,
            request.MaxPrice,
            request.IsActive,
            request.IsFeatured,
            request.Brand,
            request.Sort,
            request.Page,
            request.PageSize
        );
        var result = 
            await dispatcher.QueryAsync<GetProductsQuery, PagedResult<ProductListItemDto>>(query);

        return result.ToHttpResult();
    }
}
