using GroceryStore.Domain.Entities;

namespace GroceryStore.Domain.Interfaces;

/// <summary>
/// Repository interface for the Category aggregate (Catalog bounded context).
/// Defined in Domain, implemented in Infrastructure.
/// </summary>
public interface ICategoryRepository
{
    Task<Category?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Category?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Category>> GetByParentIdAsync(Guid? parentCategoryId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Category>> GetAllActiveAsync(CancellationToken cancellationToken = default);
    Task AddAsync(Category category, CancellationToken cancellationToken = default);
    void Update(Category category);
    void Remove(Category category);
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);
}
