using System.Net.Http.Headers;
using GroceryStore.App.Models;
using GroceryStore.App.Services.Interfaces;
using Microsoft.AspNetCore.Components.Forms;

namespace GroceryStore.App.Services.Http;

public sealed class HttpImageGalleryService : IImageGalleryService
{
    private readonly HttpClient _http;

    public HttpImageGalleryService(IHttpClientFactory factory)
    {
        _http = factory.CreateClient("ApiClient");
    }
// ─────────────────────────────────────────────
    // Library (ImageAsset)
    // ─────────────────────────────────────────────

    public async Task<List<GalleryImage>> GetImagesAsync(string? search = null)
    {
        var url = "/api/images" + (string.IsNullOrWhiteSpace(search)
            ? ""
            : $"?search={Uri.EscapeDataString(search)}");

        var res = await _http.GetFromJsonAsync<List<GalleryImage>>(url);
        return res ?? [];
    }

    public async Task<GalleryImage> UploadAsync(IBrowserFile file)
    {
        if (file is null) throw new ArgumentNullException(nameof(file));

        // Keep in sync with domain max size (10MB)
        var stream = file.OpenReadStream(maxAllowedSize: 10 * 1024 * 1024);

        using var form = new MultipartFormDataContent();
        var fileContent = new StreamContent(stream);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);

        // IMPORTANT: parameter name must match UploadImageRequest.File => "file"
        form.Add(fileContent, "file", file.Name);

        var httpRes = await _http.PostAsync("/api/images/upload", form);
        if (!httpRes.IsSuccessStatusCode)
            throw new InvalidOperationException(await httpRes.Content.ReadAsStringAsync());

        // Your endpoint returns Created(...) with body of ImageAssetDto (via ToCreatedHttpResult)
        var dto = await httpRes.Content.ReadFromJsonAsync<ImageAssetDto>() ?? throw new InvalidOperationException("Upload succeeded but response body was empty.");
        
        return MapAssetToGallery(dto);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var res = await _http.DeleteAsync($"/api/images/{id:guid}");
        return res.IsSuccessStatusCode;
    }

    // ─────────────────────────────────────────────
    // Entity images (Assignments)
    // - products use ProductImageRef (IsPrimary/SortOrder/AltText)
    // - categories can be similar (single image or list)
    // ─────────────────────────────────────────────

    public async Task<List<GalleryImage>> GetEntityImagesAsync(GalleryTarget target, int entityId)
    {
        if (entityId <= 0) return new List<GalleryImage>();

        if (target == GalleryTarget.Product)
        {
            var res = await _http.GetFromJsonAsync<ListProductImagesResponse>(
                $"/api/images/products/{entityId}/images");

            return res?.Items?.Select(MapProductImageToGallery).ToList() ?? new List<GalleryImage>();
        }

        // Category (choose one style)
        // If categories have single image ref, expose a list anyway for UI consistency:
        var catRes = await _http.GetFromJsonAsync<ListCategoryImagesResponse>(
            $"/api/images/categories/{entityId}/images");

        return catRes?.Items?.Select(MapCategoryImageToGallery).ToList() ?? new List<GalleryImage>();
    }

    public async Task<bool> AssignAsync(List<Guid> imageIds, GalleryTarget target, int entityId, bool makeFirstPrimary)
    {
        if (entityId <= 0 || imageIds is null || imageIds.Count == 0) return false;

        if (target == GalleryTarget.Product)
        {
            var payload = new AssignProductImagesRequest
            {
                ImageIds = imageIds,
                MakeFirstPrimary = makeFirstPrimary
            };

            var res = await _http.PostAsJsonAsync($"/api/images/products/{entityId}/assign", payload);
            return res.IsSuccessStatusCode;
        }

        var catPayload = new AssignCategoryImagesRequest { ImageIds = imageIds };
        var catRes = await _http.PostAsJsonAsync($"/api/images/categories/{entityId}/assign", catPayload);
        return catRes.IsSuccessStatusCode;
    }

    public async Task<bool> UnassignAsync(List<Guid> imageIds, GalleryTarget target, int entityId)
    {
        if (entityId <= 0 || imageIds is null || imageIds.Count == 0) return false;

        if (target == GalleryTarget.Product)
        {
            var payload = new UnassignProductImagesRequest { ImageIds = imageIds };
            var res = await _http.PostAsJsonAsync($"/api/images/products/{entityId}/unassign", payload);
            return res.IsSuccessStatusCode;
        }

        var catPayload = new UnassignCategoryImagesRequest { ImageIds = imageIds };
        var catRes = await _http.PostAsJsonAsync($"/api/images/categories/{entityId}/unassign", catPayload);
        return catRes.IsSuccessStatusCode;
    }

    public async Task<bool> SetPrimaryAsync(Guid imageId, GalleryTarget target, int entityId)
    {
        if (entityId <= 0 || imageId == Guid.Empty) return false;

        if (target == GalleryTarget.Product)
        {
            var payload = new SetProductPrimaryRequest { ImageId = imageId };
            var res = await _http.PostAsJsonAsync($"/api/images/products/{entityId}/primary", payload);
            return res.IsSuccessStatusCode;
        }

        var catPayload = new SetCategoryPrimaryRequest { ImageId = imageId };
        var catRes = await _http.PostAsJsonAsync($"/api/images/categories/{entityId}/primary", catPayload);
        return catRes.IsSuccessStatusCode;
    }

    public async Task<bool> ReorderAsync(GalleryTarget target, int entityId, Guid draggingId, Guid targetId)
    {
        if (entityId <= 0 || draggingId == Guid.Empty || targetId == Guid.Empty) return false;

        if (target == GalleryTarget.Product)
        {
            var payload = new ReorderProductImagesRequest { DraggingId = draggingId, TargetId = targetId };
            var res = await _http.PostAsJsonAsync($"/api/images/products/{entityId}/reorder", payload);
            return res.IsSuccessStatusCode;
        }

        var catPayload = new ReorderCategoryImagesRequest { DraggingId = draggingId, TargetId = targetId };
        var catRes = await _http.PostAsJsonAsync($"/api/images/categories/{entityId}/reorder", catPayload);
        return catRes.IsSuccessStatusCode;
    }

    // ─────────────────────────────────────────────
    // Mapping helpers
    // ─────────────────────────────────────────────

    private static GalleryImage MapAssetToGallery(ImageAssetDto dto) => new()
    {
        Id = dto.Id,
        Url = dto.Url,
        FileName = dto.OriginalFileName ?? dto.FileName ?? "",
        ContentType = dto.ContentType,
        FileSizeBytes = dto.FileSizeBytes,
        AltText = dto.AltText
    };

    private static GalleryImage MapProductImageToGallery(ProductImageItemDto dto) => new()
    {
        Id = dto.ImageId,                 // IMPORTANT: cross-aggregate id
        Url = dto.Url,
        FileName = dto.OriginalFileName ?? dto.FileName ?? "",
        ContentType = dto.ContentType,
        FileSizeBytes = dto.FileSizeBytes,
        AltText = dto.AltText,

        IsPrimary = dto.IsPrimary,
        SortOrder = dto.SortOrder
    };

    private static GalleryImage MapCategoryImageToGallery(CategoryImageItemDto dto) => new()
    {
        Id = dto.ImageId,
        Url = dto.Url,
        FileName = dto.OriginalFileName ?? dto.FileName ?? "",
        ContentType = dto.ContentType,
        FileSizeBytes = dto.FileSizeBytes,
        AltText = dto.AltText,

        IsPrimary = dto.IsPrimary,
        SortOrder = dto.SortOrder
    };

    // ─────────────────────────────────────────────
    // DTOs expected from API (App-side mirrors)
    // Adjust property names to match your API contracts.
    // ─────────────────────────────────────────────

    private sealed class ListImagesResponse
    {
        public List<ImageAssetDto> Items { get; set; } = new();
    }

    // ImageAsset DTO (Media context)
    private sealed class ImageAssetDto
    {
        public Guid Id { get; set; }                 // ImageId.Value
        public string Url { get; set; } = "";
        public string StoragePath { get; set; } = "";
        public string? AltText { get; set; }

        // Metadata flattened (recommended) OR keep nested
        public string? OriginalFileName { get; set; }
        public string? FileName { get; set; }        // fallback if your API uses FileName
        public string? ContentType { get; set; }
        public long FileSizeBytes { get; set; }

        // audit timestamps (depending on what you expose)
        public DateTime? CreatedAtUtc { get; set; }
        public DateTime? UploadedAtUtc { get; set; } // fallback
    }

    // Product entity list
    private sealed class ListProductImagesResponse
    {
        public List<ProductImageItemDto> Items { get; set; } = new();
    }

    private sealed class ProductImageItemDto
    {
        // from ProductImageRef
        public Guid ImageId { get; set; }
        public bool IsPrimary { get; set; }
        public int SortOrder { get; set; }
        public string? AltText { get; set; }

        // joined from ImageAsset for display
        public string Url { get; set; } = "";
        public string? OriginalFileName { get; set; }
        public string? FileName { get; set; }
        public string? ContentType { get; set; }
        public long FileSizeBytes { get; set; }
        public DateTime? CreatedAtUtc { get; set; }
        public DateTime? UploadedAtUtc { get; set; }
    }

    // Category entity list (if you support it similarly)
    private sealed class ListCategoryImagesResponse
    {
        public List<CategoryImageItemDto> Items { get; set; } = new();
    }

    private sealed class CategoryImageItemDto
    {
        public Guid ImageId { get; set; }
        public bool IsPrimary { get; set; }
        public int SortOrder { get; set; }
        public string? AltText { get; set; }

        public string Url { get; set; } = "";
        public string? OriginalFileName { get; set; }
        public string? FileName { get; set; }
        public string? ContentType { get; set; }
        public long FileSizeBytes { get; set; }
        public DateTime? CreatedAtUtc { get; set; }
        public DateTime? UploadedAtUtc { get; set; }
    }

    // Requests
    private sealed class AssignProductImagesRequest
    {
        public List<Guid> ImageIds { get; set; } = new();
        public bool MakeFirstPrimary { get; set; }
    }

    private sealed class UnassignProductImagesRequest
    {
        public List<Guid> ImageIds { get; set; } = new();
    }

    private sealed class SetProductPrimaryRequest
    {
        public Guid ImageId { get; set; }
    }

    private sealed class ReorderProductImagesRequest
    {
        public Guid DraggingId { get; set; }
        public Guid TargetId { get; set; }
    }

    // Category requests (optional)
    private sealed class AssignCategoryImagesRequest
    {
        public List<Guid> ImageIds { get; set; } = new();
    }

    private sealed class UnassignCategoryImagesRequest
    {
        public List<Guid> ImageIds { get; set; } = new();
    }

    private sealed class SetCategoryPrimaryRequest
    {
        public Guid ImageId { get; set; }
    }

    private sealed class ReorderCategoryImagesRequest
    {
        public Guid DraggingId { get; set; }
        public Guid TargetId { get; set; }
    }
}