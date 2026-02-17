using GroceryStore.Domain.Entities.Media;
using GroceryStore.Domain.ValueObjects;

namespace GroceryStore.Domain.Interfaces;

/// <summary>
/// Repository interface for the ImageAsset aggregate (Media bounded context).
/// Defined in Domain, implemented in Infrastructure.
/// </summary>
public interface IImageAssetRepository
{
    Task<ImageAsset?> GetByIdAsync(ImageId imageId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ImageAsset>> GetByIdsAsync(IEnumerable<ImageId> imageIds, CancellationToken cancellationToken = default);
    Task AddAsync(ImageAsset imageAsset, CancellationToken cancellationToken = default);
    void Update(ImageAsset imageAsset);
    void Remove(ImageAsset imageAsset);
    Task<bool> ExistsAsync(ImageId imageId, CancellationToken cancellationToken = default);
}
