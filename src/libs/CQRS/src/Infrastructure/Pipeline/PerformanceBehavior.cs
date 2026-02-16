using System.Diagnostics;
using CQRS.Abstractions.Messaging;
using CQRS.CqrsResult;
using CQRS.Infrastructure.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CQRS.Infrastructure.Pipeline;

/// <summary>
/// Measures execution time and logs a warning if it exceeds a configured threshold.
/// </summary>
internal sealed class PerformanceBehavior<TMessage, TResult> : IPipelineBehavior<TMessage, TResult>
    where TMessage : IMessage
    where TResult : ResultBase
{
    private readonly ILogger<PerformanceBehavior<TMessage, TResult>> _logger;
    private readonly IOptions<PerformanceBehaviorOptions> _options;

    public PerformanceBehavior(
        ILogger<PerformanceBehavior<TMessage, TResult>> logger,
        IOptions<PerformanceBehaviorOptions> options)
    {
        _logger = logger;
        _options = options;
    }

    public async Task<TResult> HandleAsync(
        TMessage message,
        MessageHandlerDelegate<TResult> next,
        CancellationToken cancellationToken = default)
    {
        var sw = Stopwatch.StartNew();
        var result = await next();
        sw.Stop();

        var threshold = _options.Value.WarningThresholdMilliseconds;
        if (threshold > 0 && sw.ElapsedMilliseconds >= threshold)
        {
            _logger.LogWarning(
                "Slow CQRS message detected: {MessageType} took {ElapsedMs} ms (threshold: {ThresholdMs} ms)",
                typeof(TMessage).Name,
                sw.ElapsedMilliseconds,
                threshold);
        }

        return result;
    }
}
