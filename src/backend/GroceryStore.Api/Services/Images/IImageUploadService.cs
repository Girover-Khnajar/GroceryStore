using GroceryStore.Api.Contracts.Images;
using CQRS.CqrsResult;

namespace GroceryStore.Api.Services.Images;

public interface IImageUploadService
{
    Task<Result<Guid>> UploadAsync(HttpRequest httpRequest, UploadImageRequest request, CancellationToken cancellationToken);
}
