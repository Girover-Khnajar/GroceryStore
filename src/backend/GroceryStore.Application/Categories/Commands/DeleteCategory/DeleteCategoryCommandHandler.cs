using CQRS.Abstractions.Messaging;
using CQRS.CqrsResult;
using GroceryStore.Domain.Interfaces;

namespace GroceryStore.Application.Categories.Commands;

public sealed class DeleteCategoryCommandHandler : CommandHandlerBase<DeleteCategoryCommand>
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteCategoryCommandHandler(ICategoryRepository categoryRepository, IUnitOfWork unitOfWork)
    {
        _categoryRepository = categoryRepository;
        _unitOfWork = unitOfWork;
    }

    public override async Task<Result> HandleAsync(
        DeleteCategoryCommand command, CancellationToken cancellationToken = default)
    {
        var category = await _categoryRepository.GetByIdAsync(command.Id, cancellationToken);
        if (category is null)
            return Failure(Error.NotFound($"Category '{command.Id}' not found."));

        var children = await _categoryRepository.GetByParentIdAsync(command.Id, cancellationToken);
        if (children.Count > 0)
            return Failure(Error.Conflict("Cannot delete a category that has subcategories."));

        _categoryRepository.Remove(category);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Success();
    }
}
