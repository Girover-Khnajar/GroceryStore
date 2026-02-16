using CQRS.Abstractions.Messaging;
using CQRS.CqrsResult;
using Microsoft.Extensions.DependencyInjection;

namespace CQRS.Infrastructure.Pipeline;

/// <summary>
/// Runs all registered authorizers for a message. If any fails,
/// short-circuits and returns a failed result.
/// </summary>
internal sealed class AuthorizationBehavior<TMessage, TResult> : IPipelineBehavior<TMessage, TResult>
    where TMessage : IMessage
    where TResult : ResultBase, IFailureResult<TResult>
{
    private readonly IServiceProvider _serviceProvider;

    public AuthorizationBehavior(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task<TResult> HandleAsync(
        TMessage message,
        MessageHandlerDelegate<TResult> next,
        CancellationToken cancellationToken = default)
    {
        var authorizers = _serviceProvider.GetServices<IAuthorizer<TMessage>>().ToArray();
        if (authorizers.Length == 0)
        {
            return await next();
        }

        var allErrors = new List<Error>();

        foreach (var authorizer in authorizers)
        {
            var authResult = await authorizer.AuthorizeAsync(message, cancellationToken);
            if (!authResult.IsAuthorized && authResult.Errors.Count > 0)
            {
                allErrors.AddRange(authResult.Errors);
            }
        }

        if (allErrors.Count > 0)
        {
            return TResult.Fail(allErrors);
        }

        return await next();
    }
}
