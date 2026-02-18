using CQRS.Abstractions.Messaging;
using CQRS.CqrsResult;
using GroceryStore.Application.Categories.Dtos;
using GroceryStore.Domain.Interfaces;

namespace GroceryStore.Application.Categories.Queries;

public sealed class GetCategoriesByParentIdQueryHandler : QueryHandlerBase<GetCategoriesByParentIdQuery, IReadOnlyList<CategoryDto>>
{
    private readonly ICategoryRepository _categoryRepository;

    public GetCategoriesByParentIdQueryHandler(ICategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    public override async Task<Result<IReadOnlyList<CategoryDto>>> HandleAsync(
        GetCategoriesByParentIdQuery query, CancellationToken cancellationToken = default)
    {
        var categories = await _categoryRepository.GetByParentIdAsync(query.ParentCategoryId, cancellationToken);
        var dtos = categories.Select(c => c.ToDto()).ToList();
        return Success(dtos);
    }
}
