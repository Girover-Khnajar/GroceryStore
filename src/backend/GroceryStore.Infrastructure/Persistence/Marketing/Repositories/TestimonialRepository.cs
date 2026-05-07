using GroceryStore.Domain.Entities;
using GroceryStore.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GroceryStore.Infrastructure.Persistence.Marketing.Repositories;

public sealed class TestimonialRepository : ITestimonialRepository
{
    private readonly AppDbContext _dbContext;

    public TestimonialRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<Testimonial>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Testimonials
            .OrderBy(t => t.SortOrder)
            .ThenByDescending(t => t.Id)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Testimonial>> GetActiveAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Testimonials
            .Where(t => t.IsActive)
            .OrderBy(t => t.SortOrder)
            .ThenByDescending(t => t.Id)
            .ToListAsync(cancellationToken);
    }

    public Task<Testimonial?> FindByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return _dbContext.Testimonials
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
    }

    public async Task AddAsync(Testimonial testimonial, CancellationToken cancellationToken = default)
    {
        await _dbContext.Testimonials.AddAsync(testimonial, cancellationToken);
    }

    public void Update(Testimonial testimonial)
    {
        _dbContext.Testimonials.Update(testimonial);
    }

    public void Delete(Testimonial testimonial)
    {
        _dbContext.Testimonials.Remove(testimonial);
    }

    public async Task ToggleActiveAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var item = await _dbContext.Testimonials.FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
        if (item is null)
            return;

        item.ToggleActive();
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateSortOrderAsync(IEnumerable<(Guid Id, int SortOrder)> items, CancellationToken cancellationToken = default)
    {
        var map = items.ToDictionary(x => x.Id, x => x.SortOrder);
        var ids = map.Keys.ToList();

        var testimonials = await _dbContext.Testimonials
            .Where(t => ids.Contains(t.Id))
            .ToListAsync(cancellationToken);

        foreach (var testimonial in testimonials)
        {
            testimonial.SetSortOrder(map[testimonial.Id]);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
