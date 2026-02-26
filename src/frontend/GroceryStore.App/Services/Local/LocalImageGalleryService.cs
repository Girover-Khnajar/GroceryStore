using System.Text.Json;
using GroceryStore.App.Models;
using GroceryStore.App.Services.Interfaces;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Hosting;

namespace GroceryStore.App.Services.Local;

// public sealed class LocalImageGalleryService : IImageGalleryService
// {
//     private readonly IWebHostEnvironment _env;
//     private readonly IProductService _products;
//     private readonly ICategoryService _categories;

//     private readonly string _dataDir;
//     private readonly string _metaPath;
//     private readonly SemaphoreSlim _gate = new(1, 1);

//     private static readonly JsonSerializerOptions _json = new()
//     {
//         PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
//         WriteIndented = true
//     };

//     public LocalImageGalleryService(IWebHostEnvironment env, IProductService products, ICategoryService categories)
//     {
//         _env = env;
//         _products = products;
//         _categories = categories;

//         _dataDir = Path.Combine(_env.ContentRootPath, "App_Data");
//         Directory.CreateDirectory(_dataDir);

//         _metaPath = Path.Combine(_dataDir, "gallery.json");
//     }

//     public async Task<List<GalleryImage>> GetImagesAsync(GalleryTarget? target = null, int? entityId = null)
//     {
//         await _gate.WaitAsync();
//         try
//         {
//             var all = await ReadAllAsync();

//             IEnumerable<GalleryImage> q = all;
//             if (target is not null) q = q.Where(x => x.Target == target.Value);
//             if (entityId is not null) q = q.Where(x => x.EntityId == entityId.Value);

//             // ✅ لا تكسر ترتيب التخزين
//             return q.ToList();
//         }
//         finally
//         {
//             _gate.Release();
//         }
//     }

//     public async Task<GalleryImage> UploadAsync(IBrowserFile file, GalleryTarget target, int entityId, bool setAsPrimary = false)
//     {
//         if (file is null) throw new ArgumentNullException(nameof(file));
//         if (entityId <= 0) throw new ArgumentOutOfRangeException(nameof(entityId));

//         var ext = Path.GetExtension(file.Name).ToLowerInvariant();
//         var allowed = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { ".png", ".jpg", ".jpeg", ".webp", ".gif" };
//         if (!allowed.Contains(ext))
//             throw new InvalidOperationException($"Unsupported image type: {ext}");

//         var safeName = $"{Guid.NewGuid():N}{ext}";
//         var sub = target == GalleryTarget.Product ? "products" : "categories";
//         var relDir = Path.Combine("uploads", sub, entityId.ToString());
//         var absDir = Path.Combine(_env.WebRootPath, relDir);
//         Directory.CreateDirectory(absDir);

//         var absPath = Path.Combine(absDir, safeName);
//         await using (var fs = File.Create(absPath))
//         await using (var stream = file.OpenReadStream(maxAllowedSize: 15 * 1024 * 1024))
//         {
//             await stream.CopyToAsync(fs);
//         }

//         var url = "/" + relDir.Replace(Path.DirectorySeparatorChar, '/') + "/" + safeName;

//         var item = new GalleryImage
//         {
//             Id = Guid.NewGuid(),
//             Target = target,
//             EntityId = entityId,
//             Url = url,
//             FileName = file.Name,
//             UploadedAtUtc = DateTime.UtcNow,

//             // ✅ (لو الموديل عندك فيه هالحقول)
//             ContentType = file.ContentType,
//             FileSizeBytes = (long)file.Size
//         };

//         await _gate.WaitAsync();
//         try
//         {
//             var all = await ReadAllAsync();

//             // ✅ أضفها في مكان مناسب (إذا primary خَلّها أول صورة لنفس المنتج)
//             if (target == GalleryTarget.Product && setAsPrimary)
//             {
//                 // صفّر primary للصور الأخرى
//                 foreach (var x in all.Where(x => x.Target == GalleryTarget.Product && x.EntityId == entityId))
//                     x.IsPrimary = false;

//                 item.IsPrimary = true;

//                 // احذف أي نسخة قديمة بنفس URL (احتياط)
//                 all.RemoveAll(x => x.Target == GalleryTarget.Product && x.EntityId == entityId &&
//                                    string.Equals(x.Url, url, StringComparison.OrdinalIgnoreCase));

//                 // ضعها قبل أول صورة للمنتج (يعني تصير أول وحدة)
//                 var firstIdx = all.FindIndex(x => x.Target == GalleryTarget.Product && x.EntityId == entityId);
//                 if (firstIdx < 0) all.Add(item);
//                 else all.Insert(firstIdx, item);
//             }
//             else
//             {
//                 all.Add(item);
//             }

//             await WriteAllAsync(all);
//         }
//         finally
//         {
//             _gate.Release();
//         }

//         // Attach to entity
//         if (target == GalleryTarget.Category)
//         {
//             var cat = await _categories.GetCategoryByIdAsync(entityId);
//             if (cat is null) throw new InvalidOperationException($"Category {entityId} not found.");
//             cat.Image = url;
//             await _categories.UpdateCategoryAsync(cat);
//         }
//         else
//         {
//             var p = await _products.GetProductByIdAsync(entityId);
//             if (p is null) throw new InvalidOperationException($"Product {entityId} not found.");

//             if (setAsPrimary)
//             {
//                 p.Images.RemoveAll(x => string.Equals(x, url, StringComparison.OrdinalIgnoreCase));
//                 p.Images.Insert(0, url);
//             }
//             else
//             {
//                 if (!p.Images.Any(x => string.Equals(x, url, StringComparison.OrdinalIgnoreCase)))
//                     p.Images.Add(url);
//             }

//             await _products.UpdateProductAsync(p);
//         }

//         return item;
//     }

//     public async Task<bool> DeleteAsync(Guid id)
//     {
//         GalleryImage? item;

//         await _gate.WaitAsync();
//         try
//         {
//             var all = await ReadAllAsync();
//             item = all.FirstOrDefault(x => x.Id == id);
//             if (item is null) return false;

//             all.Remove(item);
//             await WriteAllAsync(all);
//         }
//         finally
//         {
//             _gate.Release();
//         }

//         var abs = ToAbsolutePath(item.Url);
//         if (!string.IsNullOrWhiteSpace(abs) && File.Exists(abs))
//         {
//             try { File.Delete(abs); } catch { /* ignore */ }
//         }

//         if (item.Target == GalleryTarget.Category)
//         {
//             var cat = await _categories.GetCategoryByIdAsync(item.EntityId);
//             if (cat is not null && string.Equals(cat.Image, item.Url, StringComparison.OrdinalIgnoreCase))
//             {
//                 cat.Image = null;
//                 await _categories.UpdateCategoryAsync(cat);
//             }
//         }
//         else
//         {
//             var p = await _products.GetProductByIdAsync(item.EntityId);
//             if (p is not null)
//             {
//                 p.Images.RemoveAll(x => string.Equals(x, item.Url, StringComparison.OrdinalIgnoreCase));
//                 await _products.UpdateProductAsync(p);
//             }
//         }

//         return true;
//     }

//     public async Task<bool> SetPrimaryAsync(Guid id)
//     {
//         await _gate.WaitAsync();
//         try
//         {
//             var all = await ReadAllAsync();
//             var img = all.FirstOrDefault(x => x.Id == id);
//             if (img == null || img.Target != GalleryTarget.Product)
//                 return false;

//             var product = await _products.GetProductByIdAsync(img.EntityId);
//             if (product == null)
//                 return false;

//             // ✅ حدّث الميتاداتا: primary واحد فقط لنفس المنتج
//             foreach (var x in all.Where(x => x.Target == GalleryTarget.Product && x.EntityId == img.EntityId))
//                 x.IsPrimary = (x.Id == id);

//             // ✅ حرّك العنصر للواجهة (أول صور المنتج داخل القائمة)
//             all.Remove(img);
//             var firstIdx = all.FindIndex(x => x.Target == GalleryTarget.Product && x.EntityId == img.EntityId);
//             if (firstIdx < 0) all.Add(img);
//             else all.Insert(firstIdx, img);

//             await WriteAllAsync(all);

//             // ✅ حرّك الـ URL لأول قائمة صور المنتج
//             product.Images.RemoveAll(x => string.Equals(x, img.Url, StringComparison.OrdinalIgnoreCase));
//             product.Images.Insert(0, img.Url);
//             await _products.UpdateProductAsync(product);

//             return true;
//         }
//         finally
//         {
//             _gate.Release();
//         }
//     }

//     public async Task<bool> ReorderAsync(Guid draggingId, Guid targetId)
//     {
//         await _gate.WaitAsync();
//         try
//         {
//             var all = await ReadAllAsync();
//             var draggingImg = all.FirstOrDefault(x => x.Id == draggingId);
//             var targetImg = all.FirstOrDefault(x => x.Id == targetId);

//             if (draggingImg == null || targetImg == null)
//                 return false;

//             // لازم يكونوا بنفس الـ target والـ entity
//             if (draggingImg.Target != targetImg.Target || draggingImg.EntityId != targetImg.EntityId)
//                 return false;

//             // ✅ إعادة إدراج dragging قبل target ضمن نفس القائمة (يحفظ الترتيب داخل JSON)
//             all.Remove(draggingImg);
//             var targetIdx = all.IndexOf(targetImg);
//             if (targetIdx < 0) return false;

//             all.Insert(targetIdx, draggingImg);
//             await WriteAllAsync(all);
//             return true;
//         }
//         finally
//         {
//             _gate.Release();
//         }
//     }

//     private string? ToAbsolutePath(string url)
//     {
//         if (string.IsNullOrWhiteSpace(url)) return null;
//         var rel = url.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
//         return Path.Combine(_env.WebRootPath, rel);
//     }

//     private async Task<List<GalleryImage>> ReadAllAsync()
//     {
//         if (!File.Exists(_metaPath)) return new List<GalleryImage>();

//         try
//         {
//             var json = await File.ReadAllTextAsync(_metaPath);
//             return JsonSerializer.Deserialize<List<GalleryImage>>(json, _json) ?? new List<GalleryImage>();
//         }
//         catch
//         {
//             return new List<GalleryImage>();
//         }
//     }

//     private async Task WriteAllAsync(List<GalleryImage> items)
//     {
//         var json = JsonSerializer.Serialize(items, _json);
//         await File.WriteAllTextAsync(_metaPath, json);
//     }

//     public Task<List<GalleryImage>> GetImagesAsync(string? search = null)
//     {
//         throw new NotImplementedException();
//     }

//     public Task<GalleryImage> UploadAsync(IBrowserFile file)
//     {
//         throw new NotImplementedException();
//     }

//     public Task<List<GalleryImage>> GetEntityImagesAsync(GalleryTarget target, int entityId)
//     {
//         throw new NotImplementedException();
//     }

//     public Task<bool> AssignAsync(List<Guid> imageIds, GalleryTarget target, int entityId, bool makeFirstPrimary)
//     {
//         throw new NotImplementedException();
//     }

//     public Task<bool> UnassignAsync(List<Guid> imageIds, GalleryTarget target, int entityId)
//     {
//         throw new NotImplementedException();
//     }

//     public Task<bool> SetPrimaryAsync(Guid imageId, GalleryTarget target, int entityId)
//     {
//         throw new NotImplementedException();
//     }

//     public Task<bool> ReorderAsync(GalleryTarget target, int entityId, Guid draggingId, Guid targetId)
//     {
//         throw new NotImplementedException();
//     }

// }