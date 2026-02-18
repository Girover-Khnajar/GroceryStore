using CQRS.Abstractions.Messaging;
using CQRS.CqrsResult;
using GroceryStore.Domain.Interfaces;

namespace GroceryStore.Application.Categories.Commands;

public sealed class UpdateCategoryCommandHandler : CommandHandlerBase<UpdateCategoryCommand>
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateCategoryCommandHandler(ICategoryRepository categoryRepository, IUnitOfWork unitOfWork)
    {
        _categoryRepository = categoryRepository;
        _unitOfWork = unitOfWork;
    }

    public override async Task<Result> HandleAsync(
        UpdateCategoryCommand command, CancellationToken cancellationToken = default)
    {
        var category = await _categoryRepository.GetByIdAsync(command.Id, cancellationToken);
        if (category is null)
            return Failure(Error.NotFound($"Category '{command.Id}' not found."));

        var existingBySlug = await _categoryRepository.GetBySlugAsync(command.Slug, cancellationToken);
        if (existingBySlug is not null && existingBySlug.Id != command.Id)
            return Failure(Error.Conflict($"A category with slug '{command.Slug}' already exists."));

        if (command.ParentCategoryId.HasValue)
        {
            if (command.ParentCategoryId.Value == command.Id)
                return Failure(Error.Validation("A category cannot be its own parent."));

            var parentExists = await _categoryRepository.ExistsAsync(command.ParentCategoryId.Value, cancellationToken);
            if (!parentExists)
                return Failure(Error.NotFound($"Parent category '{command.ParentCategoryId.Value}' not found."));
        }

        category.Rename(command.Name);
        category.ChangeSlug(command.Slug);
        category.SetDescription(command.Description);
        category.SetImage(command.ImageUrl);
        category.SetIcon(command.IconName);
        category.SetParent(command.ParentCategoryId);
        category.Reorder(command.SortOrder);
        category.SetSeo(command.SeoMetaTitle, command.SeoMetaDescription);

        _categoryRepository.Update(category);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Success();
    }
}
