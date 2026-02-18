using CQRS.Abstractions.Messaging;
using CQRS.CqrsResult;
using GroceryStore.Domain.Enums;
using GroceryStore.Domain.Interfaces;
using GroceryStore.Domain.ValueObjects;

namespace GroceryStore.Application.Products.Commands;

public sealed class UpdateProductCommandHandler : CommandHandlerBase<UpdateProductCommand>
{
    private readonly IProductRepository _productRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateProductCommandHandler(
        IProductRepository productRepository,
        ICategoryRepository categoryRepository,
        IUnitOfWork unitOfWork)
    {
        _productRepository = productRepository;
        _categoryRepository = categoryRepository;
        _unitOfWork = unitOfWork;
    }

    public override async Task<Result> HandleAsync(
        UpdateProductCommand command, CancellationToken cancellationToken = default)
    {
        var product = await _productRepository.GetByIdAsync(command.Id, cancellationToken);
        if (product is null)
            return Failure(Error.NotFound($"Product '{command.Id}' not found."));

        var categoryExists = await _categoryRepository.ExistsAsync(command.CategoryId, cancellationToken);
        if (!categoryExists)
            return Failure(Error.NotFound($"Category '{command.CategoryId}' not found."));

        var existingBySlug = await _productRepository.GetBySlugAsync(command.Slug, cancellationToken);
        if (existingBySlug is not null && existingBySlug.Id != command.Id)
            return Failure(Error.Conflict($"A product with slug '{command.Slug}' already exists."));

        if (!Enum.TryParse<ProductUnit>(command.Unit, ignoreCase: true, out var unit))
            return Failure(Error.Validation($"Invalid product unit '{command.Unit}'."));

        var price = Money.Create(command.PriceAmount, command.PriceCurrency);

        product.ChangeCategory(command.CategoryId);
        product.Rename(command.Name);
        product.ChangeSlug(command.Slug);
        product.ChangePrice(price, unit);
        product.SetDescriptions(command.ShortDescription, command.LongDescription);
        product.Reorder(command.SortOrder);
        product.SetSeo(command.SeoMetaTitle, command.SeoMetaDescription);

        if (command.IsFeatured)
            product.Feature();
        else
            product.Unfeature();

        _productRepository.Update(product);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Success();
    }
}
