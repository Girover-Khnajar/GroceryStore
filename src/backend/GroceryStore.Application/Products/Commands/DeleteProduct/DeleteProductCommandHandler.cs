using CQRS.Abstractions.Messaging;
using CQRS.CqrsResult;
using GroceryStore.Domain.Interfaces;

namespace GroceryStore.Application.Products.Commands;

public sealed class DeleteProductCommandHandler : CommandHandlerBase<DeleteProductCommand>
{
    private readonly IProductRepository _productRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteProductCommandHandler(IProductRepository productRepository, IUnitOfWork unitOfWork)
    {
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
    }

    public override async Task<Result> HandleAsync(
        DeleteProductCommand command, CancellationToken cancellationToken = default)
    {
        var product = await _productRepository.GetByIdAsync(command.Id, cancellationToken);
        if (product is null)
            return Failure(Error.NotFound($"Product '{command.Id}' not found."));

        _productRepository.Remove(product);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Success();
    }
}
