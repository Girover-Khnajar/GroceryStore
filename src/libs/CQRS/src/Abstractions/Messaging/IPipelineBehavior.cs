using CQRS.CqrsResult;

namespace CQRS.Abstractions.Messaging;

/// <summary>
/// Delegate that represents the next step in a pipeline.
/// </summary>
public delegate Task<TResult> MessageHandlerDelegate<TResult>();

/// <summary>
/// Defines a pipeline behavior that can run cross-cutting concerns (validation, logging, etc.)
/// around message handling.
/// </summary>
/// <typeparam name="TMessage">The message type (command or query)</typeparam>
/// <typeparam name="TResult">The result type returned by the handler</typeparam>
public interface IPipelineBehavior<in TMessage, TResult>
    where TMessage : IMessage
    where TResult : ResultBase
{
    /// <summary>
    /// Handles the message and optionally invokes the next delegate in the pipeline.
    /// </summary>
    Task<TResult> HandleAsync(
        TMessage message,
        MessageHandlerDelegate<TResult> next,
        CancellationToken cancellationToken = default);
}
