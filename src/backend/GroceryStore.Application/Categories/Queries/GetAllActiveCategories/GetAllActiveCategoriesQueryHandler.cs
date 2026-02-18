using CQRS.Abstractions.Messaging;
using CQRS.CqrsResult;
using GroceryStore.Application.Categories.Dtos;
using GroceryStore.Domain.Interfaces;

namespace GroceryStore.Application.Categories.Queries;

public sealed class GetAllActiveCategoriesQueryHandler : QueryHandlerBase<GetAllActiveCategoriesQuery, IReadOnlyList<CategoryDto>>
{
    private readonly ICategoryRepository _categoryRepository;

    public GetAllActiveCategoriesQueryHandler(ICategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    public override async Task<Result<IReadOnlyList<CategoryDto>>> HandleAsync(
        GetAllActiveCategoriesQuery query, CancellationToken cancellationToken = default)
    {
        var categories = await _categoryRepository.GetAllActiveAsync(cancellationToken);
        var dtos = categories.Select(c => c.ToDto()).ToList();
        return Success(dtos);
    }
}
