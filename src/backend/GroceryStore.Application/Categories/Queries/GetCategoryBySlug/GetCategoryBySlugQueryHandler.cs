using CQRS.Abstractions.Messaging;
using CQRS.CqrsResult;
using GroceryStore.Application.Categories.Dtos;
using GroceryStore.Domain.Interfaces;

namespace GroceryStore.Application.Categories.Queries;

public sealed class GetCategoryBySlugQueryHandler : QueryHandlerBase<GetCategoryBySlugQuery, CategoryDto>
{
    private readonly ICategoryRepository _categoryRepository;

    public GetCategoryBySlugQueryHandler(ICategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    public override async Task<Result<CategoryDto>> HandleAsync(
        GetCategoryBySlugQuery query, CancellationToken cancellationToken = default)
    {
        var category = await _categoryRepository.GetBySlugAsync(query.Slug, cancellationToken);
        if (category is null)
            return NotFound($"Category with slug '{query.Slug}' not found.");

        return Success(category.ToDto());
    }
}
