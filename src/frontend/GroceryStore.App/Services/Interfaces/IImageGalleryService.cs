using GroceryStore.App.Models;
using Microsoft.AspNetCore.Components.Forms;

namespace GroceryStore.App.Services.Interfaces;

public interface IImageGalleryService
{
    // Library
    Task<List<GalleryImage>> GetImagesAsync(string? search = null);
    Task<GalleryImage> UploadAsync(IBrowserFile file);
    Task<bool> DeleteAsync(Guid id);

    // Assignments
    Task<List<GalleryImage>> GetEntityImagesAsync(GalleryTarget target, int entityId);
    Task<bool> AssignAsync(List<Guid> imageIds, GalleryTarget target, int entityId, bool makeFirstPrimary);
    Task<bool> UnassignAsync(List<Guid> imageIds, GalleryTarget target, int entityId);
    Task<bool> SetPrimaryAsync(Guid imageId, GalleryTarget target, int entityId);
    Task<bool> ReorderAsync(GalleryTarget target, int entityId, Guid draggingId, Guid targetId);
}