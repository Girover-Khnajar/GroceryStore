using CQRS.Abstractions.Messaging;
using CQRS.CqrsResult;
using GroceryStore.Domain.Entities;
using GroceryStore.Domain.Interfaces;
using GroceryStore.Domain.ValueObjects;

namespace GroceryStore.Application.Categories.Commands;

public sealed class CreateCategoryCommandHandler : CommandHandlerBase<CreateCategoryCommand, Guid>
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateCategoryCommandHandler(ICategoryRepository categoryRepository, IUnitOfWork unitOfWork)
    {
        _categoryRepository = categoryRepository;
        _unitOfWork = unitOfWork;
    }

    public override async Task<Result<Guid>> HandleAsync(
        CreateCategoryCommand command, CancellationToken cancellationToken = default)
    {
        var existingBySlug = await _categoryRepository.GetBySlugAsync(command.Slug, cancellationToken);
        if (existingBySlug is not null)
            return Failure(Error.Conflict($"A category with slug '{command.Slug}' already exists."));

        if (command.ParentCategoryId.HasValue)
        {
            var parentExists = await _categoryRepository.ExistsAsync(command.ParentCategoryId.Value, cancellationToken);
            if (!parentExists)
                return Failure(Error.NotFound($"Parent category '{command.ParentCategoryId.Value}' not found."));
        }

        var seo = SeoMeta.Create(command.SeoMetaTitle, command.SeoMetaDescription);

        var category = Category.Create(
            command.Name,
            command.Slug,
            command.SortOrder,
            command.ParentCategoryId,
            command.Description,
            command.ImageUrl,
            command.IconName,
            seo);

        await _categoryRepository.AddAsync(category, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Success(category.Id);
    }
}
