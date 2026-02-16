namespace CQRS.Infrastructure.Options;
public sealed class PipelineOptions
{
    public bool UseUnhandledException { get; set; } = true;
    public bool UseLogging { get; set; } = true;
    public bool UsePerformance { get; set; } = false;
    public bool UseAuthorization { get; set; } = false;
    public bool UseValidation { get; set; } = true;

    // اختياري: ترتيب ثابت بدل الاعتماد على ترتيب التسجيل
    public int OrderUnhandledException { get; set; } = 0;
    public int OrderLogging { get; set; } = 30;
    public int OrderPerformance { get; set; } = 40;
    public int OrderAuthorization { get; set; } = 20;
    public int OrderValidation { get; set; } = 10;
}