using CQRS.Abstractions.Messaging;
using CQRS.CqrsResult;
using GroceryStore.Domain.Entities;
using GroceryStore.Domain.Enums;
using GroceryStore.Domain.Interfaces;
using GroceryStore.Domain.ValueObjects;

namespace GroceryStore.Application.Products.Commands;

public sealed class CreateProductCommandHandler : CommandHandlerBase<CreateProductCommand, Guid>
{
    private readonly IProductRepository _productRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateProductCommandHandler(
        IProductRepository productRepository,
        ICategoryRepository categoryRepository,
        IUnitOfWork unitOfWork)
    {
        _productRepository = productRepository;
        _categoryRepository = categoryRepository;
        _unitOfWork = unitOfWork;
    }

    public override async Task<Result<Guid>> HandleAsync(
        CreateProductCommand command, CancellationToken cancellationToken = default)
    {
        var categoryExists = await _categoryRepository.ExistsAsync(command.CategoryId, cancellationToken);
        if (!categoryExists)
            return Failure(Error.NotFound($"Category '{command.CategoryId}' not found."));

        var existingBySlug = await _productRepository.GetBySlugAsync(command.Slug, cancellationToken);
        if (existingBySlug is not null)
            return Failure(Error.Conflict($"A product with slug '{command.Slug}' already exists."));

        if (!Enum.TryParse<ProductUnit>(command.Unit, ignoreCase: true, out var unit))
            return Failure(Error.Validation($"Invalid product unit '{command.Unit}'."));

        var price = Money.Create(command.PriceAmount, command.PriceCurrency);
        var seo = SeoMeta.Create(command.SeoMetaTitle, command.SeoMetaDescription);

        var product = Product.Create(
            command.CategoryId,
            command.Name,
            command.Slug,
            price,
            unit,
            command.SortOrder,
            command.IsFeatured,
            command.ShortDescription,
            command.LongDescription,
            seo);

        await _productRepository.AddAsync(product, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Success(product.Id);
    }
}
