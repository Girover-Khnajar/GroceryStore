using CQRS.Abstractions.Messaging;
using CQRS.CqrsResult;
using Microsoft.Extensions.DependencyInjection;

namespace CQRS.Infrastructure;

/// <summary>
/// Default implementation of message dispatcher using service provider
/// </summary>
internal sealed class MessageDispatcher : IMessageDispatcher

{
    private readonly IServiceProvider _serviceProvider;

    public MessageDispatcher(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task<Result> SendAsync<TCommand>(TCommand command, CancellationToken cancellationToken = default)
        where TCommand : ICommand
    {
        var handler = _serviceProvider.GetRequiredService<ICommandHandler<TCommand>>();
        MessageHandlerDelegate<Result> handlerDelegae = () => handler.HandleAsync(command, cancellationToken);
        var pipeline = BuildPipeline(command, handlerDelegae, cancellationToken);
        return await pipeline();
    }

    public async Task<Result<TResponse>> SendAsync<TCommand, TResponse>(TCommand command, CancellationToken cancellationToken = default)
        where TCommand : ICommand<TResponse>
    {
        var handler = _serviceProvider.GetRequiredService<ICommandHandler<TCommand, TResponse>>();
        MessageHandlerDelegate<Result<TResponse>> handlerDelegae = () => handler.HandleAsync(command, cancellationToken);
        var pipeline = BuildPipeline(command, handlerDelegae, cancellationToken);
        return await pipeline();
    }

    public async Task<Result<TResponse>> QueryAsync<TQuery, TResponse>(TQuery query, CancellationToken cancellationToken = default)
        where TQuery : IQuery<TResponse>
    {
        var handler = _serviceProvider.GetRequiredService<IQueryHandler<TQuery, TResponse>>();
        MessageHandlerDelegate<Result<TResponse>> handlerDelegae = () => handler.HandleAsync(query, cancellationToken);
        var pipeline = BuildPipeline(query, handlerDelegae, cancellationToken);
        return await pipeline();
    }
    
    private MessageHandlerDelegate<TResult> BuildPipeline<TMessage, TResult>(
        TMessage message,
        MessageHandlerDelegate<TResult> handler,
        CancellationToken cancellationToken)
        where TMessage : IMessage
        where TResult : ResultBase
    {
        var behaviors = _serviceProvider
            .GetServices<IPipelineBehavior<TMessage, TResult>>()
            .Reverse()
            .ToArray();

        MessageHandlerDelegate<TResult> next = handler;

        foreach (var behavior in behaviors)
        {
            var currentNext = next;
            next = () => behavior.HandleAsync(message, currentNext, cancellationToken);
        }

        return next;
    }
}