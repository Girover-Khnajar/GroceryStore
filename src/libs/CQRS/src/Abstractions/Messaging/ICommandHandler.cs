using CQRS.CqrsResult;

namespace CQRS.Abstractions.Messaging;

/// <summary>
/// Defines a handler for commands without a return value
/// </summary>
/// <typeparam name="TCommand">The type of command being handled</typeparam>
public interface ICommandHandler<in TCommand>
    where TCommand : ICommand
{
    /// <summary>
    /// Handles the command execution
    /// </summary>
    /// <param name="command">The command to handle</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A result indicating success or failure</returns>
    Task<Result> HandleAsync(TCommand command, CancellationToken cancellationToken = default);
}


/// <summary>
/// Defines a handler for commands that return a typed value
/// </summary>
/// <typeparam name="TCommand">The type of command being handled</typeparam>
/// <typeparam name="TResponse">The type of response returned</typeparam>
public interface ICommandHandler<in TCommand, TResponse>
    where TCommand : ICommand<TResponse>
{
    /// <summary>
    /// Handles the command execution
    /// </summary>
    /// <param name="command">The command to handle</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A result containing the response or error information</returns>
    Task<Result<TResponse>> HandleAsync(TCommand command, CancellationToken cancellationToken = default);
}
