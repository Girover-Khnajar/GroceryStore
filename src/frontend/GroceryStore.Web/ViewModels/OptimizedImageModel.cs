namespace GroceryStore.Web.ViewModels;

public class OptimizedImageModel
{
    public string? Src { get; set; }
    public string? ThumbnailSrc { get; set; }
    public string Alt { get; set; } = "";
    public string CssClass { get; set; } = "";
    public string ContainerClass { get; set; } = "";
    public string ContainerStyle { get; set; } = "";
    public string AspectRatio { get; set; } = "1/1";
    public bool Eager { get; set; } = false;
    public string Fallback { get; set; } = "/images/ui/product-placeholder.svg";
    public bool ShowOverlay { get; set; } = false;
    public string? OverlayContent { get; set; }
}
