using GroceryStore.Domain.Entities.Media;
using GroceryStore.Domain.Interfaces;
using GroceryStore.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace GroceryStore.Infrastructure.Persistence.Media.Repositories;

public sealed class ImageAssetRepository : IImageAssetRepository
{
    private readonly AppDbContext _dbContext;

    public ImageAssetRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ImageAsset?> GetByIdAsync(ImageId imageId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.ImageAssets
            .FirstOrDefaultAsync(a => a.ImageId == imageId, cancellationToken);
    }

    public async Task<IReadOnlyList<ImageAsset>> GetByIdsAsync(IEnumerable<ImageId> imageIds, CancellationToken cancellationToken = default)
    {
        var guidIds = imageIds.Select(id => (Guid)id).ToList();

        if (guidIds.Count == 0)
            return [];

        return await _dbContext.ImageAssets
            .Where(a => guidIds.Contains(a.Id))
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(ImageAsset imageAsset, CancellationToken cancellationToken = default)
    {
        await _dbContext.ImageAssets.AddAsync(imageAsset, cancellationToken);
    }

    public void Update(ImageAsset imageAsset)
    {
        _dbContext.ImageAssets.Update(imageAsset);
    }

    public void Remove(ImageAsset imageAsset)
    {
        _dbContext.ImageAssets.Remove(imageAsset);
    }

    public async Task<bool> ExistsAsync(ImageId imageId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.ImageAssets
            .AnyAsync(a => a.ImageId == imageId, cancellationToken);
    }
}
