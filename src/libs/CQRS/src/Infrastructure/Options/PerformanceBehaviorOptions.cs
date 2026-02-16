namespace CQRS.Infrastructure.Options;

/// <summary>
/// Options for <see cref="Pipeline.PerformanceBehavior{TMessage,TResult}"/>.
/// </summary>
public sealed class PerformanceBehaviorOptions
{
    /// <summary>
    /// Threshold (ms) above which a warning will be logged.
    /// </summary>
    public long WarningThresholdMilliseconds { get; set; } = 500;
}
