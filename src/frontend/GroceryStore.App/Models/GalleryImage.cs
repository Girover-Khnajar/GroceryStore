namespace GroceryStore.App.Models;

public enum GalleryTarget
{
    Category = 0,
    Product  = 1
}

/// <summary>Represents an uploaded image in the admin gallery.</summary>
public sealed class GalleryImage
{
    public Guid Id { get; set; }
    public Guid ImageId { get; set; }
    public string StoragePath { get; set; } = "";
    public string Url { get; set; } = "";
    public string? AltText { get; set; } // ✅
    public string FileName { get; set; } = "";
    public string? ContentType { get; set; }
    public string OriginalFileName { get; set; } = "";
    public long FileSizeBytes { get; set; }
    public bool IsDeleted { get; set; }
    public bool IsPrimary { get; set; } = false;  // ✅ (Entity view)
    public int SortOrder { get; set; } = 0;   // ✅ (Entity view)
    public int WidthPx { get; set; }
    public int HeightPx { get; set; }
    public DateTime CreatedOnUtc { get; set; }
    public DateTime? ModifiedOnUtc { get; set; }
}
