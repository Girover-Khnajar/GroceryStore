using CQRS.CqrsResult;

namespace CQRS.Abstractions.Messaging;

/// <summary>
/// Dispatcher for sending commands and queries
/// Provides a centralized way to dispatch messages without direct handler references
/// </summary>
public interface IMessageDispatcher
{
    /// <summary>
    /// Sends a command without a response
    /// </summary>
    /// <param name="command">The command to send</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A task representing the asynchronous operation, containing the result of the command</returns>
    /// <remarks> Use this method for commands that do not return a value </remarks>
    Task<Result> SendAsync<TCommand>(TCommand command, CancellationToken cancellationToken = default)
        where TCommand : ICommand;

    /// <summary>
    /// Sends a command with a typed response.
    /// </summary>
    /// <typeparam name="TCommand">The type of the command</typeparam>
    /// <typeparam name="TResponse">The type of the command result</typeparam>
    /// <param name="command">The command to send</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A task representing the asynchronous operation, 
    /// containing the result of the command. see <see cref="Result{T}"/></returns>
    /// <remarks> Use this method for commands that return a value </remarks>
    Task<Result<TResponse>> SendAsync<TCommand, TResponse>(TCommand command, CancellationToken cancellationToken = default)
        where TCommand : ICommand<TResponse>;

    /// <summary>
    /// Sends a query with a typed response
    /// </summary>
    Task<Result<TResponse>> QueryAsync<TQuery, TResponse>(TQuery query, CancellationToken cancellationToken = default)
        where TQuery : IQuery<TResponse>;
}
