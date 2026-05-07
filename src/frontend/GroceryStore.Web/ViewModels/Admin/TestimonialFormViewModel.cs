using System.ComponentModel.DataAnnotations;

namespace GroceryStore.Web.ViewModels.Admin;

public sealed class TestimonialFormViewModel
{
    public Guid? Id { get; set; }

    [Required, MaxLength(150)]
    public string ClientName { get; set; } = string.Empty;

    [MaxLength(150)]
    public string? ClientTitle { get; set; }

    [MaxLength(150)]
    public string? ClientCompany { get; set; }

    [MaxLength(255)]
    public string? ClientImage { get; set; }

    [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5.")]
    public byte? Rating { get; set; }

    [Required]
    public string Testimonial { get; set; } = string.Empty;

    [Range(0, int.MaxValue, ErrorMessage = "Sort order must be numeric.")]
    public int SortOrder { get; set; }

    public bool IsActive { get; set; } = true;

    public IFormFile? ClientImageFile { get; set; }
}
