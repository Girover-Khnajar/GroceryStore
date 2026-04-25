using System.ComponentModel.DataAnnotations;
using GroceryStore.Application.Categories.Dtos;

namespace GroceryStore.Web.ViewModels.Admin;

public sealed class ProductFormViewModel
{
    public Guid? Id { get; set; }

    [Required]
    public Guid CategoryId { get; set; }

    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [Required, MaxLength(200)]
    public string Slug { get; set; } = string.Empty;

    [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than zero.")]
    public decimal PriceAmount { get; set; }

    [Required, MaxLength(10)]
    public string PriceCurrency { get; set; } = "USD";

    [Required, MaxLength(20)]
    public string Unit { get; set; } = "Piece";

    public int SortOrder { get; set; }
    public bool IsFeatured { get; set; }

    [MaxLength(500)]
    public string? ShortDescription { get; set; }

    [MaxLength(5000)]
    public string? LongDescription { get; set; }

    [MaxLength(200)]
    public string? SeoMetaTitle { get; set; }

    [MaxLength(300)]
    public string? SeoMetaDescription { get; set; }

    /// <summary>Available categories for the dropdown.</summary>
    public IReadOnlyList<CategoryDto> AvailableCategories { get; set; } = [];
}
