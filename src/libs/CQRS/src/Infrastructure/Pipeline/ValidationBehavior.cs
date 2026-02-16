using CQRS.Abstractions.Messaging;
using CQRS.CqrsResult;
using Microsoft.Extensions.DependencyInjection;

namespace CQRS.Infrastructure.Pipeline;

/// <summary>
/// Runs all registered validators for a message. If any errors are returned,
/// short-circuits and returns a failed result.
/// </summary>
/// <typeparam name="TMessage">The message type (command or query)</typeparam>
/// <typeparam name="TResult">The result type returned by the handler</typeparam>
internal sealed class ValidationBehavior<TMessage, TResult>(IServiceProvider serviceProvider) : IPipelineBehavior<TMessage, TResult>
    where TMessage : IMessage
    where TResult : ResultBase, IFailureResult<TResult>
{
    private readonly IServiceProvider _serviceProvider = serviceProvider;

    public async Task<TResult> HandleAsync(
        TMessage message,
        MessageHandlerDelegate<TResult> next,
        CancellationToken cancellationToken = default)
    {
        var validators = _serviceProvider.GetServices<IValidator<TMessage>>().ToArray();
        if (validators.Length == 0)
        {
            return await next();
        }

        var allErrors = new List<Error>();

        foreach (var validator in validators)
        {
            var errors = await validator.ValidateAsync(message, cancellationToken);
            if (errors is { Count: > 0 })
            {
                allErrors.AddRange(errors);
            }
        }

        if (allErrors.Count > 0)
        {
            return TResult.Fail(allErrors);
        }

        return await next();
    }
}
