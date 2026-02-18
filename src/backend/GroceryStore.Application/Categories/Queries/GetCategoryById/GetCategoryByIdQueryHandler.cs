using CQRS.Abstractions.Messaging;
using CQRS.CqrsResult;
using GroceryStore.Application.Categories.Dtos;
using GroceryStore.Domain.Interfaces;

namespace GroceryStore.Application.Categories.Queries;

public sealed class GetCategoryByIdQueryHandler : QueryHandlerBase<GetCategoryByIdQuery, CategoryDto>
{
    private readonly ICategoryRepository _categoryRepository;

    public GetCategoryByIdQueryHandler(ICategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    public override async Task<Result<CategoryDto>> HandleAsync(
        GetCategoryByIdQuery query, CancellationToken cancellationToken = default)
    {
        var category = await _categoryRepository.GetByIdAsync(query.Id, cancellationToken);
        if (category is null)
            return NotFound($"Category '{query.Id}' not found.");

        return Success(category.ToDto());
    }
}
