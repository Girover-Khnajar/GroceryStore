using System.Diagnostics;
using CQRS.Abstractions.Messaging;
using CQRS.CqrsResult;
using Microsoft.Extensions.Logging;

namespace CQRS.Infrastructure.Pipeline;

/// <summary>
/// Logs start/end of message handling with basic tracing info.
/// </summary>
/// <typeparam name="TMessage">The message type. Command or Query</typeparam>
/// <typeparam name="TResult">The result type returned by the handler</typeparam>
internal sealed class LoggingBehavior<TMessage, TResult> : IPipelineBehavior<TMessage, TResult>
    where TMessage : IMessage
    where TResult : ResultBase
{
    private readonly ILogger<LoggingBehavior<TMessage, TResult>> _logger;

    public LoggingBehavior(ILogger<LoggingBehavior<TMessage, TResult>> logger)
    {
        _logger = logger;
    }

    public async Task<TResult> HandleAsync(
        TMessage message,
        MessageHandlerDelegate<TResult> next,
        CancellationToken cancellationToken = default)
    {
        var messageName = typeof(TMessage).Name;
        var traceId = Activity.Current?.TraceId.ToString();

        using var scope = _logger.BeginScope(new Dictionary<string, object?>
        {
            ["messageType"] = messageName,
            ["traceId"] = traceId,
        });

        _logger.LogInformation("Handling {MessageType}", messageName);

        var sw = Stopwatch.StartNew();
        var result = await next();
        sw.Stop();

        if (result.IsSuccess)
        {
            _logger.LogInformation("Handled {MessageType} in {ElapsedMs} ms", messageName, sw.ElapsedMilliseconds);
        }
        else
        {
            _logger.LogWarning(
                "Handled {MessageType} with failure in {ElapsedMs} ms. Errors: {ErrorCount}",
                messageName,
                sw.ElapsedMilliseconds,
                result.Errors.Count);
        }

        return result;
    }
}
