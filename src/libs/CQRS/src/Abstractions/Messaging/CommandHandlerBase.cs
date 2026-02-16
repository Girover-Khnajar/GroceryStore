using CQRS.CqrsResult;

namespace CQRS.Abstractions.Messaging;

/// <summary>
/// Abstract base class for command handlers without response
/// Provides common functionality and enforces consistent patterns
/// </summary>
/// <typeparam name="TCommand">The command type</typeparam>
public abstract class CommandHandlerBase<TCommand> : ICommandHandler<TCommand>
    where TCommand : ICommand
{
    public abstract Task<Result> HandleAsync(TCommand command, CancellationToken cancellationToken = default);

    /// <summary>
    /// Helper method to create a success result
    /// </summary>
    protected Result Success() => Result.Ok();

    /// <summary>
    /// Helper method to create a failure result with error message
    /// </summary>
    protected Result Failure(string errorMessage) => 
                    Result.Fail(Error.Validation(errorMessage));

    /// <summary>
    /// Helper method to create a failure result with custom error
    /// </summary>
    protected Result Failure(Error error) => Result.Fail(error);

    /// <summary>
    /// Helper method to create a failure result with multiple errors
    /// </summary>
    protected Result Failure(IEnumerable<Error> errors) => Result.Fail([.. errors]);
}




/// <summary>
/// Abstract base class for command handlers with response
/// </summary>
/// <typeparam name="TCommand">The command type</typeparam>
/// <typeparam name="TResponse">The response type</typeparam>
public abstract class CommandHandlerBase<TCommand, TResponse> : ICommandHandler<TCommand, TResponse>
    where TCommand : ICommand<TResponse>
{
    public abstract Task<Result<TResponse>> HandleAsync(TCommand command, CancellationToken cancellationToken = default);

    /// <summary>
    /// Helper method to create a success result with value
    /// </summary>
    protected Result<TResponse> Success(TResponse value) => Result<TResponse>.Ok(value);

    /// <summary>
    /// Helper method to create a failure result with error message
    /// </summary>
    protected Result<TResponse> Failure(string errorMessage) 
        => Result<TResponse>.Fail(Error.Validation(errorMessage));

    /// <summary>
    /// Helper method to create a failure result with custom error
    /// </summary>
    protected Result<TResponse> Failure(Error error) => Result<TResponse>.Fail(error);

    /// <summary>
    /// Helper method to create a failure result with multiple errors
    /// </summary>
    protected Result<TResponse> Failure(IEnumerable<Error> errors) => Result<TResponse>.Fail([.. errors]);
}