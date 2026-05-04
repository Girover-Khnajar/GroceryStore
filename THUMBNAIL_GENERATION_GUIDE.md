# Thumbnail Generation Implementation Guide

## Overview

This document explains the automatic thumbnail generation feature that has been added to the GroceryStore application. When images are uploaded through the admin gallery, thumbnails are automatically created to improve loading performance.

## Architecture

The thumbnail generation is implemented across three layers:

### 1. Domain Layer (GroceryStore.Domain)

**File:** `Interfaces/IImageProcessor.cs`

Defines the contract for image processing operations:
```csharp
public interface IImageProcessor
{
    Task<string?> GenerateThumbnailAsync(...);
    string? GetThumbnailPath(string originalPath);
}
```

### 2. Infrastructure Layer (GroceryStore.Infrastructure)

**File:** `Storage/ImageProcessor.cs`

Implements thumbnail generation using SixLabors.ImageSharp:
- Generates 300x300 max size thumbnails
- Maintains aspect ratio
- Saves with 85% quality (JPEG/WebP)
- Only creates thumbnails for images larger than 300x300
- Skips SVG files (no thumbnail needed)
- Adds `_thumb` suffix to filename (e.g., `image.jpg` → `image_thumb.jpg`)

**Dependencies Added:**
- `SixLabors.ImageSharp` v3.1.5 in `GroceryStore.Infrastructure.csproj`

**Registration:** `DependencyInjection.cs`
```csharp
services.AddScoped<IImageProcessor, ImageProcessor>();
```

### 3. Web Layer (GroceryStore.Web)

**Files:**
- `Services/ImageUrlHelper.cs` - Helper service for thumbnail URL resolution
- `Controllers/AdminController.cs` - Updated to generate thumbnails on upload
- `ViewModels/OptimizedImageModel.cs` - Added `ThumbnailSrc` property
- `Views/Shared/_OptimizedImage.cshtml` - Updated to use thumbnails for lazy loading

## How It Works

### Upload Process

1. **User uploads image** via Admin Gallery (`/Admin/Gallery/Upload`)
2. **Original image is saved** to disk (e.g., `/wwwroot/images/uploads/2026/05/abc123.jpg`)
3. **Thumbnail is generated automatically**:
   - Resizes to max 300x300 (aspect ratio preserved)
   - Saves as `abc123_thumb.jpg` in the same directory
   - Uses Lanczos3 resampling for high quality
4. **API response includes both URLs**:
   ```json
   {
     "imageId": "guid",
     "url": "https://domain.com/images/uploads/2026/05/abc123.jpg",
     "thumbnailUrl": "https://domain.com/images/uploads/2026/05/abc123_thumb.jpg",
     "storagePath": "/images/uploads/2026/05/abc123.jpg",
     "thumbnailPath": "/images/uploads/2026/05/abc123_thumb.jpg"
   }
   ```

### Display Process

Views can automatically use thumbnails for lazy loading:

```cshtml
@inject GroceryStore.Web.Services.ImageUrlHelper ImageHelper

@{
    var imageUrl = product.PrimaryImageId;
    var thumbnailUrl = ImageHelper.GetThumbnailUrl(imageUrl);
}

@await Html.PartialAsync("_OptimizedImage", new OptimizedImageModel {
    Src = imageUrl,              // Full resolution image
    ThumbnailSrc = thumbnailUrl, // Thumbnail for lazy loading (if available)
    Alt = product.Name,
    Eager = false                // Lazy load the image
})
```

**Behavior:**
- If `Eager = false` (lazy loading) and `ThumbnailSrc` is provided → loads thumbnail first
- If `Eager = true` (immediate load) → always loads full `Src`
- If thumbnail doesn't exist → falls back to full image
- Skeleton placeholder shown during load

## Benefits

### Performance Improvements

1. **Faster initial page load**: Thumbnails are ~10-20x smaller than originals
2. **Reduced bandwidth**: Product listing pages load much less data
3. **Better UX**: Images appear faster, skeleton placeholders reduce layout shift
4. **Automatic**: No manual intervention needed

### Example Size Reduction

| Original | Thumbnail | Savings |
|----------|-----------|---------|
| 1200x1200 (350 KB) | 300x300 (25 KB) | ~93% |
| 800x600 (180 KB) | 300x225 (18 KB) | ~90% |
| 500x500 (95 KB) | 300x300 (22 KB) | ~77% |

## Thumbnail File Naming

Thumbnails use the `_thumb` suffix:
- Original: `/images/uploads/2026/05/abc123def456.jpg`
- Thumbnail: `/images/uploads/2026/05/abc123def456_thumb.jpg`

**Special cases:**
- SVG files: No thumbnail generated (vector format, already optimized)
- Small images (≤300x300): No thumbnail needed

## Usage in Views

### Method 1: Using ImageUrlHelper (Recommended)

```cshtml
@inject GroceryStore.Web.Services.ImageUrlHelper ImageHelper

@{
    var thumbnail = ImageHelper.GetThumbnailUrl(imageUrl);
}

@await Html.PartialAsync("_OptimizedImage", new OptimizedImageModel {
    Src = imageUrl,
    ThumbnailSrc = thumbnail,
    ...
})
```

### Method 2: Without Helper (Manual)

If you know the image path pattern:
```cshtml
@{
    var thumbnail = imageUrl?.Replace(".jpg", "_thumb.jpg");
}
```
⚠️ Not recommended - doesn't check if thumbnail exists

### ImageUrlHelper Methods

```csharp
// Returns thumbnail URL if exists, otherwise null
string? GetThumbnailUrl(string? imageUrl)

// Returns thumbnail if exists, otherwise returns original URL
string GetThumbnailUrlOrOriginal(string? imageUrl)
```

## Example: Updated Views

### Products/Index.cshtml

```cshtml
@inject GroceryStore.Web.Services.ImageUrlHelper ImageHelper

@foreach (var product in Model.Products)
{
    var imageUrl = product.PrimaryImageId;
    var thumbnailUrl = ImageHelper.GetThumbnailUrl(imageUrl);
    
    @await Html.PartialAsync("_OptimizedImage", new OptimizedImageModel {
        Src = imageUrl,
        ThumbnailSrc = thumbnailUrl,
        Alt = product.Name,
        AspectRatio = "1/1",
        Eager = false  // Use lazy loading with thumbnail
    })
}
```

### Products/Details.cshtml

```cshtml
@* Detail page - load full image immediately *@
@await Html.PartialAsync("_OptimizedImage", new OptimizedImageModel {
    Src = Model.PrimaryImageUrl,
    Alt = Model.Product.Name,
    Eager = true  // No thumbnail, load full image
})
```

## Configuration

### Thumbnail Settings

Located in `Infrastructure/Storage/ImageProcessor.cs`:

```csharp
private const int ThumbnailMaxWidth = 300;
private const int ThumbnailMaxHeight = 300;
private const string ThumbnailSuffix = "_thumb";
```

### Quality Settings

```csharp
JpegEncoder { Quality = 85 }
WebpEncoder { Quality = 85 }
PngEncoder()  // Lossless
```

**To change thumbnail size:** Modify `ThumbnailMaxWidth` and `ThumbnailMaxHeight`
**To change quality:** Modify encoder quality values (1-100)

## Error Handling

Thumbnail generation is non-blocking:
- If thumbnail generation fails → Original image is still saved
- `thumbnailUrl` in response will be `null`
- Views will fall back to full image automatically
- No exceptions thrown to user

## Testing

### Test Image Upload

1. Navigate to `/Admin/Gallery`
2. Upload a large image (>300x300)
3. Check the response in browser DevTools Network tab
4. Verify `thumbnailUrl` is present
5. Check file system: both original and `_thumb` files should exist

### Test Thumbnail Display

1. Navigate to product listing page (`/Products`)
2. Open DevTools Network tab
3. Scroll to see lazy loading
4. Verify thumbnails load first (smaller file size)
5. Check image quality is acceptable

### Verify Performance

Run Lighthouse audit before and after:
- **Before**: Original images loaded
- **After**: Thumbnails loaded
- Check LCP (Largest Contentful Paint) improvement
- Verify bandwidth savings

## Projects Modified

✅ **GroceryStore.Domain** - Added `IImageProcessor` interface
✅ **GroceryStore.Infrastructure** - Added `ImageProcessor` implementation  
✅ **GroceryStore.Web** - Updated upload handler, added helper service, updated views

❌ **GroceryStore.Api** - Not modified (per user requirement)
❌ **GroceryStore.App** - Not modified (per user requirement)
❌ **GroceryStore.UI** - Not modified (per user requirement)

## Backward Compatibility

✅ **Existing images**: Will work fine, thumbnails will be generated on next upload
✅ **Missing thumbnails**: System falls back to original image automatically
✅ **Old code**: Still works, thumbnails are optional enhancement
✅ **API responses**: New fields are additive (thumbnailUrl, thumbnailPath)

## Future Enhancements

Potential improvements:
- [ ] Batch generate thumbnails for existing images (admin tool)
- [ ] Multiple thumbnail sizes (small, medium, large)
- [ ] Responsive `srcset` with multiple resolutions
- [ ] WebP conversion for better compression
- [ ] Background job for thumbnail generation (async)
- [ ] Thumbnail regeneration when size config changes

## Troubleshooting

### Thumbnails not generating

1. Check `ImageProcessor` is registered in `DependencyInjection.cs`
2. Verify SixLabors.ImageSharp package is restored
3. Check file system permissions on upload directory
4. Review error logs for exceptions

### Thumbnails exist but not loading

1. Check `ImageUrlHelper` is injected in view
2. Verify `ThumbnailSrc` is passed to `OptimizedImageModel`
3. Check file path is correct (case-sensitive on Linux)
4. Ensure `_OptimizedImage.cshtml` has thumbnail logic

### Poor thumbnail quality

1. Increase quality in `ImageProcessor.cs` (default 85)
2. Adjust `KnownResamplers.Lanczos3` to higher quality sampler
3. Increase max thumbnail dimensions

## Summary

Thumbnail generation is now **fully automatic** for all image uploads through the GroceryStore.Web admin gallery. Views can opt-in to using thumbnails by:
1. Injecting `ImageUrlHelper`
2. Getting thumbnail URL with `GetThumbnailUrl()`
3. Passing it to `OptimizedImageModel.ThumbnailSrc`

The implementation is **non-breaking** and **backward compatible** with existing images and code.
