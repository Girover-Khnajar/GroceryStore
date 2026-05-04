using CQRS.Abstractions.Messaging;
using GroceryStore.Application.Categories.Commands;
using GroceryStore.Application.Categories.Dtos;
using GroceryStore.Application.Categories.Queries;
using GroceryStore.Application.Common;
using GroceryStore.Application.Images.Commands;
using GroceryStore.Application.Images.Dtos;
using GroceryStore.Application.Images.Queries;
using GroceryStore.Application.Images.Queries.GetImages;
using GroceryStore.Application.Products.Commands;
using GroceryStore.Application.Products.Commands.AssignImageAssetToProduct;
using GroceryStore.Application.Products.Commands.RemoveProductImage;
using GroceryStore.Application.Products.Commands.SetPrimaryProductImage;
using GroceryStore.Application.Products.Dtos;
using GroceryStore.Application.Products.Queries;
using GroceryStore.Application.Products.Queries.GetProducts;
using GroceryStore.Domain.Entities;
using GroceryStore.Domain.Interfaces;
using GroceryStore.Web.Extensions;
using GroceryStore.Web.Services;
using GroceryStore.Web.ViewModels.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace GroceryStore.Web.Controllers;

[Route("Admin")]
[Authorize]
public class AdminController : Controller
{
    private readonly IMessageDispatcher _dispatcher;
    private readonly IWebHostEnvironment _env;
    private readonly IStoreSettingsService _settingsService;
    private readonly IUserRepository _users;
    private readonly IPasswordHasher<User> _hasher;
    private readonly IImageProcessor _imageProcessor;

    public AdminController(
        IMessageDispatcher dispatcher,
        IWebHostEnvironment env,
        IStoreSettingsService settingsService,
        IUserRepository users,
        IPasswordHasher<User> hasher,
        IImageProcessor imageProcessor)
    {
        _dispatcher = dispatcher;
        _env = env;
        _settingsService = settingsService;
        _users = users;
        _hasher = hasher;
        _imageProcessor = imageProcessor;
    }

    // ── Dashboard ──────────────────────────────────────────────────────

    [HttpGet("")]
    [HttpGet("Dashboard")]
    public async Task<IActionResult> Dashboard(CancellationToken ct)
    {
        var catsResult = await _dispatcher
            .QueryAsync<GetAllActiveCategoriesQuery, IReadOnlyList<CategoryDto>>(
                new GetAllActiveCategoriesQuery(), ct);

        var prodsResult = await _dispatcher
            .QueryAsync<GetProductsQuery, PagedResult<ProductListItemDto>>(
                new GetProductsQuery(null, null, null, null, null, null, null, null, 1, 1000), ct);

        var categories = catsResult.IsSuccess ? catsResult.Value! : [];
        var products = prodsResult.IsSuccess ? prodsResult.Value!.Items : [];

        var vm = new AdminDashboardViewModel
        {
            TotalCategories = categories.Count,
            TotalProducts = products.Count,
            ActiveProducts = products.Count(p => p.IsActive),
            FeaturedProducts = products.Count(p => p.IsFeatured),
            RecentCategories = categories.Take(5).ToList()
        };

        return View(vm);
    }

    // ── Store Settings ────────────────────────────────────────────────

    [HttpGet("Settings")]
    public async Task<IActionResult> Settings(CancellationToken ct)
    {
        var vm = await _settingsService.GetAsync(ct);
        return View(vm);
    }

    [HttpPost("Settings")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Settings(StoreSettingsViewModel vm, CancellationToken ct)
    {
        if (!ModelState.IsValid)
            return View(vm);

        await _settingsService.SaveAsync(vm, ct);
        TempData["Success"] = "Settings saved successfully.";
        return RedirectToAction(nameof(Settings));
    }

    // ── Change Password ────────────────────────────────────────────────

    [HttpGet("ChangePassword")]
    public IActionResult ChangePassword() => View(new ChangePasswordViewModel());

    [HttpPost("ChangePassword")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(ChangePasswordViewModel vm, CancellationToken ct)
    {
        if (!ModelState.IsValid)
            return View(vm);

        var username = User.Identity?.Name;
        var user = username is null ? null : await _users.FindByUsernameAsync(username, ct);

        if (user is null)
        {
            ModelState.AddModelError(string.Empty, "User not found.");
            return View(vm);
        }

        var verification = _hasher.VerifyHashedPassword(user, user.PasswordHash, vm.CurrentPassword);
        if (verification == PasswordVerificationResult.Failed)
        {
            ModelState.AddModelError(nameof(vm.CurrentPassword), "Current password is incorrect.");
            return View(vm);
        }

        var newHash = _hasher.HashPassword(user, vm.NewPassword);
        await _users.UpdatePasswordAsync(user, newHash, ct);

        TempData["Success"] = "Password changed successfully.";
        return RedirectToAction(nameof(ChangePassword));
    }

    // ── Categories CRUD ────────────────────────────────────────────────

    [HttpGet("Categories")]
    public async Task<IActionResult> Categories(CancellationToken ct)
    {
        var result = await _dispatcher
            .QueryAsync<GetAllActiveCategoriesQuery, IReadOnlyList<CategoryDto>>(
                new GetAllActiveCategoriesQuery(), ct);

        return View(result.IsSuccess ? result.Value! : (IReadOnlyList<CategoryDto>)[]);
    }

    [HttpGet("Categories/Create")]
    public async Task<IActionResult> CategoryCreate(CancellationToken ct)
    {
        var vm = new CategoryFormViewModel
        {
            AvailableParents = await GetAllCategories(ct)
        };
        return View("CategoryForm", vm);
    }

    [HttpPost("Categories/Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CategoryCreate(CategoryFormViewModel vm, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            vm.AvailableParents = await GetAllCategories(ct);
            return View("CategoryForm", vm);
        }

        var command = new CreateCategoryCommand(
            vm.Name,
            vm.Slug,
            vm.SortOrder,
            vm.ParentCategoryId,
            vm.Description,
            vm.ImageUrl,
            vm.IconName,
            vm.SeoMetaTitle,
            vm.SeoMetaDescription);

        var result = await _dispatcher.SendAsync<CreateCategoryCommand, Guid>(command, ct);
        if (!result.Succeeded(this))
        {
            vm.AvailableParents = await GetAllCategories(ct);
            return View("CategoryForm", vm);
        }

        TempData["Success"] = "Category created successfully.";
        return RedirectToAction(nameof(Categories));
    }

    [HttpGet("Categories/Edit/{id:guid}")]
    public async Task<IActionResult> CategoryEdit(Guid id, CancellationToken ct)
    {
        var result = await _dispatcher
            .QueryAsync<GetCategoryByIdQuery, CategoryDto>(new GetCategoryByIdQuery(id), ct);

        if (result.IsNotFound()) return NotFound();

        var dto = result.ValueOrDefault(this);
        if (dto is null) return RedirectToAction(nameof(Categories));

        var vm = CategoryFormViewModel.FromDto(dto);
        vm.AvailableParents = await GetAllCategories(ct);
        return View("CategoryForm", vm);
    }

    [HttpPost("Categories/Edit/{id:guid}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CategoryEdit(Guid id, CategoryFormViewModel vm, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            vm.AvailableParents = await GetAllCategories(ct);
            return View("CategoryForm", vm);
        }

        var command = new UpdateCategoryCommand(
            id,
            vm.Name,
            vm.Slug,
            vm.SortOrder,
            vm.ParentCategoryId,
            vm.Description,
            vm.ImageUrl,
            vm.IconName,
            vm.SeoMetaTitle,
            vm.SeoMetaDescription);

        var result = await _dispatcher.SendAsync(command, ct);
        if (!result.Succeeded(this))
        {
            vm.AvailableParents = await GetAllCategories(ct);
            return View("CategoryForm", vm);
        }

        TempData["Success"] = "Category updated successfully.";
        return RedirectToAction(nameof(Categories));
    }

    [HttpPost("Categories/Delete/{id:guid}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CategoryDelete(Guid id, CancellationToken ct)
    {
        var result = await _dispatcher.SendAsync(new DeleteCategoryCommand(id), ct);
        if (result.Succeeded(this))
            TempData["Success"] = "Category deleted.";

        return RedirectToAction(nameof(Categories));
    }

    // ── Products CRUD ──────────────────────────────────────────────────

    [HttpGet("Products")]
    public async Task<IActionResult> Products(
        string? search, int page = 1, CancellationToken ct = default)
    {
        var query = new GetProductsQuery(
            search, null, null, null, null, null, null, null, page, 20);

        var result = await _dispatcher
            .QueryAsync<GetProductsQuery, PagedResult<ProductListItemDto>>(query, ct);

        ViewBag.Search = search;
        return View(result.IsSuccess ? result.Value! : new PagedResult<ProductListItemDto>());
    }

    [HttpGet("Products/Create")]
    public async Task<IActionResult> ProductCreate(CancellationToken ct)
    {
        var vm = new ProductFormViewModel
        {
            AvailableCategories = await GetAllCategories(ct),
            AvailableCurrencies = await GetAvailableCurrenciesAsync(),
        };
        vm.PriceCurrency = vm.AvailableCurrencies.FirstOrDefault() ?? vm.PriceCurrency;
        return View("ProductForm", vm);
    }

    [HttpPost("Products/Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ProductCreate(ProductFormViewModel vm, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            vm.AvailableCategories = await GetAllCategories(ct);
            vm.AvailableCurrencies = await GetAvailableCurrenciesAsync(vm.PriceCurrency);
            return View("ProductForm", vm);
        }

        var command = new CreateProductCommand(
            vm.CategoryId,
            vm.Name,
            vm.Slug,
            vm.PriceAmount,
            vm.PriceCurrency,
            vm.Unit,
            vm.SortOrder,
            vm.IsFeatured,
            vm.ShortDescription,
            vm.LongDescription,
            vm.SeoMetaTitle,
            vm.SeoMetaDescription);

        var result = await _dispatcher.SendAsync<CreateProductCommand, Guid>(command, ct);
        if (!result.Succeeded(this))
        {
            vm.AvailableCategories = await GetAllCategories(ct);
            vm.AvailableCurrencies = await GetAvailableCurrenciesAsync(vm.PriceCurrency);
            return View("ProductForm", vm);
        }

        TempData["Success"] = "Product created successfully.";
        return RedirectToAction(nameof(Products));
    }

    [HttpGet("Products/Edit/{id:guid}")]
    public async Task<IActionResult> ProductEdit(Guid id, CancellationToken ct)
    {
        var result = await _dispatcher
            .QueryAsync<GetProductByIdQuery, ProductDto>(new GetProductByIdQuery(id), ct);

        if (result.IsNotFound()) return NotFound();

        var dto = result.ValueOrDefault(this);
        if (dto is null) return RedirectToAction(nameof(Products));

        var vm = new ProductFormViewModel
        {
            Id = dto.Id,
            CategoryId = dto.CategoryId,
            Name = dto.Name,
            Slug = dto.Slug,
            PriceAmount = dto.PriceAmount,
            PriceCurrency = dto.PriceCurrency,
            Unit = dto.Unit,
            SortOrder = dto.SortOrder,
            IsFeatured = dto.IsFeatured,
            ShortDescription = dto.ShortDescription,
            LongDescription = dto.LongDescription,
            SeoMetaTitle = dto.SeoMetaTitle,
            SeoMetaDescription = dto.SeoMetaDescription,
            AvailableCategories = await GetAllCategories(ct),
            AvailableCurrencies = await GetAvailableCurrenciesAsync(dto.PriceCurrency),
        };

        return View("ProductForm", vm);
    }

    [HttpPost("Products/Edit/{id:guid}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ProductEdit(Guid id, ProductFormViewModel vm, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            vm.AvailableCategories = await GetAllCategories(ct);
            vm.AvailableCurrencies = await GetAvailableCurrenciesAsync(vm.PriceCurrency);
            return View("ProductForm", vm);
        }

        var command = new UpdateProductCommand(
            id,
            vm.CategoryId,
            vm.Name,
            vm.Slug,
            vm.PriceAmount,
            vm.PriceCurrency,
            vm.Unit,
            vm.SortOrder,
            vm.IsFeatured,
            vm.ShortDescription,
            vm.LongDescription,
            vm.SeoMetaTitle,
            vm.SeoMetaDescription);

        var result = await _dispatcher.SendAsync(command, ct);
        if (!result.Succeeded(this))
        {
            vm.AvailableCategories = await GetAllCategories(ct);
            vm.AvailableCurrencies = await GetAvailableCurrenciesAsync(vm.PriceCurrency);
            return View("ProductForm", vm);
        }

        TempData["Success"] = "Product updated successfully.";
        return RedirectToAction(nameof(Products));
    }

    [HttpPost("Products/Delete/{id:guid}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ProductDelete(Guid id, CancellationToken ct)
    {
        var result = await _dispatcher.SendAsync(new DeleteProductCommand(id), ct);
        if (result.Succeeded(this))
            TempData["Success"] = "Product deleted.";

        return RedirectToAction(nameof(Products));
    }

    // ── Gallery ─────────────────────────────────────────────────────────

    [HttpGet("Gallery")]
    public async Task<IActionResult> Gallery(CancellationToken ct)
    {
        var imagesResult = await _dispatcher
            .QueryAsync<GetImagesQuery, List<ImageAssetDto>>(
                new GetImagesQuery(null), ct);

        var catsResult = await _dispatcher
            .QueryAsync<GetAllActiveCategoriesQuery, IReadOnlyList<CategoryDto>>(
                new GetAllActiveCategoriesQuery(), ct);

        var prodsResult = await _dispatcher
            .QueryAsync<GetProductsQuery, PagedResult<ProductListItemDto>>(
                new GetProductsQuery(null, null, null, null, null, null, null, null, 1, 1000), ct);

        ViewBag.Categories = catsResult.IsSuccess ? catsResult.Value! : (IReadOnlyList<CategoryDto>)[];
        ViewBag.Products = prodsResult.IsSuccess ? prodsResult.Value!.Items : [];

        return View(imagesResult.IsSuccess ? imagesResult.Value! : new List<ImageAssetDto>());
    }

    [HttpPost("Gallery/Upload")]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> GalleryUpload(IFormFile file, string? altText, CancellationToken ct)
    {
        if (file is null || file.Length == 0)
            return BadRequest(new { error = "File is required." });

        if (file.Length > 10 * 1024 * 1024)
            return BadRequest(new { error = "File must not exceed 10 MB." });

        var allowed = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            { "image/jpeg", "image/png", "image/webp", "image/gif", "image/svg+xml" };

        if (string.IsNullOrWhiteSpace(file.ContentType) || !allowed.Contains(file.ContentType))
            return BadRequest(new { error = $"Content type '{file.ContentType}' not allowed." });

        var ext = Path.GetExtension(file.FileName);
        if (string.IsNullOrWhiteSpace(ext) || ext.Length > 10)
            ext = file.ContentType.ToLowerInvariant() switch
            {
                "image/jpeg" => ".jpg",
                "image/png" => ".png",
                "image/webp" => ".webp",
                "image/gif" => ".gif",
                "image/svg+xml" => ".svg",
                _ => ""
            };

        var now = DateTimeOffset.UtcNow;
        var safeName = $"{Guid.NewGuid():N}{ext}";
        var relPath = Path.Combine("images", "uploads", now.Year.ToString(),
            now.Month.ToString("00"), safeName);
        var physPath = Path.Combine(_env.WebRootPath, relPath);

        Directory.CreateDirectory(Path.GetDirectoryName(physPath)!);

        // Save original file
        await using (var fs = System.IO.File.Create(physPath))
            await file.CopyToAsync(fs, ct);

        // Generate thumbnail
        string? thumbnailPath = null;
        try
        {
            await using var thumbnailStream = file.OpenReadStream();
            thumbnailPath = await _imageProcessor.GenerateThumbnailAsync(
                thumbnailStream,
                physPath,
                file.FileName,
                file.ContentType,
                ct);
        }
        catch
        {
            // If thumbnail generation fails, continue without it
        }

        var urlPath = "/" + relPath.Replace('\\', '/');
        var fullUrl = $"{Request.Scheme}://{Request.Host}{urlPath}";

        var thumbnailUrl = !string.IsNullOrWhiteSpace(thumbnailPath)
            ? $"{Request.Scheme}://{Request.Host}{thumbnailPath}"
            : null;

        var cmd = new CreateImageAssetCommand(
            urlPath, fullUrl,
            Path.GetFileName(file.FileName), file.ContentType,
            file.Length, 0, 0, altText);

        var result = await _dispatcher.SendAsync<CreateImageAssetCommand, Guid>(cmd, ct);
        if (!result.IsSuccess)
            return StatusCode(500, new { error = "Failed to save image record." });

        return Ok(new
        {
            imageId = result.Value,
            url = fullUrl,
            thumbnailUrl,
            storagePath = urlPath,
            thumbnailPath,
            originalFileName = Path.GetFileName(file.FileName),
            contentType = file.ContentType,
            fileSizeBytes = file.Length,
            createdOnUtc = DateTime.UtcNow
        });
    }

    [HttpPost("Gallery/Delete/{id:guid}")]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> GalleryDeleteAjax(Guid id, CancellationToken ct)
    {
        var result = await _dispatcher.SendAsync(new DeleteImageAssetCommand(id), ct);
        return result.IsSuccess
            ? Ok(new { success = true })
            : BadRequest(new { error = "Delete failed." });
    }

    [HttpPost("Gallery/BulkDelete")]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> GalleryBulkDelete([FromBody] List<Guid> ids, CancellationToken ct)
    {
        int deleted = 0;
        foreach (var id in ids)
        {
            var r = await _dispatcher.SendAsync(new DeleteImageAssetCommand(id), ct);
            if (r.IsSuccess) deleted++;
        }
        return Ok(new { deleted, total = ids.Count });
    }

    [HttpGet("Gallery/Images")]
    public async Task<IActionResult> GalleryImagesJson(CancellationToken ct)
    {
        var result = await _dispatcher
            .QueryAsync<GetImagesQuery, List<ImageAssetDto>>(new GetImagesQuery(null), ct);
        return Json(result.IsSuccess ? result.Value! : new List<ImageAssetDto>());
    }

    // ── Gallery Assign / Unassign / SetPrimary ──────────────────────────

    public sealed record GalleryAssignRequest(
        string Target, Guid EntityId, List<Guid> ImageIds, bool MakePrimary = false);

    public sealed record GalleryUnassignRequest(
        string Target, Guid EntityId, List<Guid> ImageIds);

    public sealed record GallerySetPrimaryRequest(
        string Target, Guid EntityId, Guid ImageId);

    [HttpPost("Gallery/Assign")]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> GalleryAssign(
        [FromBody] GalleryAssignRequest req, CancellationToken ct)
    {
        if (req.ImageIds is null || req.ImageIds.Count == 0)
            return BadRequest(new { error = "At least one image must be selected." });

        if (req.Target == "category")
        {
            var categoryResult = await _dispatcher
                .QueryAsync<GetCategoryByIdQuery, CategoryDto>(new GetCategoryByIdQuery(req.EntityId), ct);

            if (!categoryResult.IsSuccess || categoryResult.Value is null)
                return BadRequest(new { error = "Category not found." });

            var imageResult = await _dispatcher
                .QueryAsync<GetImageByIdQuery, ImageAssetDto>(new GetImageByIdQuery(req.ImageIds[0]), ct);

            if (!imageResult.IsSuccess || imageResult.Value is null)
                return BadRequest(new { error = "Image not found." });

            var cat = categoryResult.Value;
            var update = new UpdateCategoryCommand(
                cat.Id,
                cat.Name,
                cat.Slug,
                cat.SortOrder,
                cat.ParentCategoryId,
                cat.Description,
                imageResult.Value.StoragePath,
                cat.IconName,
                cat.SeoMetaTitle,
                cat.SeoMetaDescription);

            var updateResult = await _dispatcher.SendAsync(update, ct);
            return updateResult.IsSuccess
                ? Ok(new { assigned = 1, total = req.ImageIds.Count })
                : BadRequest(new { error = "Assign to category failed." });
        }

        if (req.Target != "product")
            return BadRequest(new { error = "Unsupported target type." });

        int ok = 0;
        foreach (var imageId in req.ImageIds)
        {
            var makePrimary = req.MakePrimary && ok == 0;  // first one = primary when checked
            var r = await _dispatcher.SendAsync(
                new AssignImageAssetToProductCommand(req.EntityId, imageId, makePrimary), ct);
            if (r.IsSuccess) ok++;
        }
        return Ok(new { assigned = ok, total = req.ImageIds.Count });
    }

    [HttpPost("Gallery/Unassign")]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> GalleryUnassign(
        [FromBody] GalleryUnassignRequest req, CancellationToken ct)
    {
        if (req.Target == "category")
        {
            var categoryResult = await _dispatcher
                .QueryAsync<GetCategoryByIdQuery, CategoryDto>(new GetCategoryByIdQuery(req.EntityId), ct);

            if (!categoryResult.IsSuccess || categoryResult.Value is null)
                return BadRequest(new { error = "Category not found." });

            var cat = categoryResult.Value;
            var update = new UpdateCategoryCommand(
                cat.Id,
                cat.Name,
                cat.Slug,
                cat.SortOrder,
                cat.ParentCategoryId,
                cat.Description,
                null,
                cat.IconName,
                cat.SeoMetaTitle,
                cat.SeoMetaDescription);

            var updateResult = await _dispatcher.SendAsync(update, ct);
            return updateResult.IsSuccess
                ? Ok(new { removed = 1, total = req.ImageIds.Count })
                : BadRequest(new { error = "Unassign from category failed." });
        }

        if (req.Target != "product")
            return BadRequest(new { error = "Unsupported target type." });

        int ok = 0;
        foreach (var imageId in req.ImageIds)
        {
            var r = await _dispatcher.SendAsync(
                new RemoveProductImageCommand(req.EntityId, imageId), ct);
            if (r.IsSuccess) ok++;
        }
        return Ok(new { removed = ok, total = req.ImageIds.Count });
    }

    [HttpPost("Gallery/SetPrimary")]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> GallerySetPrimary(
        [FromBody] GallerySetPrimaryRequest req, CancellationToken ct)
    {
        if (req.Target == "category")
        {
            var categoryResult = await _dispatcher
                .QueryAsync<GetCategoryByIdQuery, CategoryDto>(new GetCategoryByIdQuery(req.EntityId), ct);

            if (!categoryResult.IsSuccess || categoryResult.Value is null)
                return BadRequest(new { error = "Category not found." });

            var imageResult = await _dispatcher
                .QueryAsync<GetImageByIdQuery, ImageAssetDto>(new GetImageByIdQuery(req.ImageId), ct);

            if (!imageResult.IsSuccess || imageResult.Value is null)
                return BadRequest(new { error = "Image not found." });

            var cat = categoryResult.Value;
            var update = new UpdateCategoryCommand(
                cat.Id,
                cat.Name,
                cat.Slug,
                cat.SortOrder,
                cat.ParentCategoryId,
                cat.Description,
                imageResult.Value.StoragePath,
                cat.IconName,
                cat.SeoMetaTitle,
                cat.SeoMetaDescription);

            var updateResult = await _dispatcher.SendAsync(update, ct);
            return updateResult.IsSuccess
                ? Ok(new { success = true })
                : BadRequest(new { error = "Set category image failed." });
        }

        if (req.Target != "product")
            return BadRequest(new { error = "Unsupported target type." });

        var r = await _dispatcher.SendAsync(
            new SetPrimaryProductImageCommand(req.EntityId, req.ImageId), ct);
        return r.IsSuccess
            ? Ok(new { success = true })
            : BadRequest(new { error = "Set primary failed." });
    }

    // ── Helpers ────────────────────────────────────────────────────────

    private async Task<IReadOnlyList<CategoryDto>> GetAllCategories(CancellationToken ct)
    {
        var result = await _dispatcher
            .QueryAsync<GetAllActiveCategoriesQuery, IReadOnlyList<CategoryDto>>(
                new GetAllActiveCategoriesQuery(), ct);
        return result.IsSuccess ? result.Value! : [];
    }

    private async Task<IReadOnlyList<string>> GetAvailableCurrenciesAsync(string? selectedCurrency = null)
    {
        var settings = await _settingsService.GetAsync();
        var configured = settings.Currency?.Trim().ToUpperInvariant();
        var selected = selectedCurrency?.Trim().ToUpperInvariant();

        var options = new List<string>
        {
            "USD",
            "EUR",
            "DKK",
            "SEK",
            "NOK",
            "CHF",
            "GBP"
        };

        if (!string.IsNullOrWhiteSpace(configured) && configured.Length == 3 && !options.Contains(configured))
            options.Insert(0, configured);

        if (!string.IsNullOrWhiteSpace(selected) && selected.Length == 3 && !options.Contains(selected))
            options.Insert(0, selected);

        return options;
    }
}
