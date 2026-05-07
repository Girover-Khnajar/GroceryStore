using GroceryStore.Domain.Entities;

namespace GroceryStore.Domain.Interfaces;

public interface ITestimonialRepository
{
    Task<IReadOnlyList<Testimonial>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Testimonial>> GetActiveAsync(CancellationToken cancellationToken = default);
    Task<Testimonial?> FindByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddAsync(Testimonial testimonial, CancellationToken cancellationToken = default);
    void Update(Testimonial testimonial);
    void Delete(Testimonial testimonial);
    Task ToggleActiveAsync(Guid id, CancellationToken cancellationToken = default);
    Task UpdateSortOrderAsync(IEnumerable<(Guid Id, int SortOrder)> items, CancellationToken cancellationToken = default);
}
