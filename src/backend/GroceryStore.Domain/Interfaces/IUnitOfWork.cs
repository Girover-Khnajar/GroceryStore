namespace GroceryStore.Domain.Interfaces;

/// <summary>
/// Unit of Work abstraction for coordinating transactional persistence
/// across aggregates within the same or multiple bounded contexts.
/// Implemented in Infrastructure.
/// </summary>
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
