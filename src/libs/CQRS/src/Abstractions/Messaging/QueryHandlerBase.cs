using CQRS.CqrsResult;

namespace CQRS.Abstractions.Messaging;

/// <summary>
/// Abstract base class for query handlers
/// </summary>
/// <typeparam name="TQuery">The query type</typeparam>
/// <typeparam name="TResponse">The response type</typeparam>
public abstract class QueryHandlerBase<TQuery, TResponse> : IQueryHandler<TQuery, TResponse>
    where TQuery : IQuery<TResponse>
{

    public abstract Task<Result<TResponse>> HandleAsync(TQuery query, CancellationToken cancellationToken = default);

    /// <summary>
    /// Helper method to create a success result with value
    /// </summary>
    protected Result<TResponse> Success(TResponse value) => Result<TResponse>.Ok(value);

    /// <summary>
    /// Helper method to create a failure result with error message
    /// </summary>
    protected Result<TResponse> Failure(string errorMessage) => Result<TResponse>.Fail(Error.Validation(errorMessage));

    /// <summary>
    /// Helper method to create a failure result with custom error
    /// </summary>
    protected Result<TResponse> Failure(Error error) => Result<TResponse>.Fail(error);

    /// <summary>
    /// Helper method to create a not found result
    /// </summary>
    protected Result<TResponse> NotFound(string message = "Resource not found") => 
        Result<TResponse>.Fail(Error.NotFound(message));
}
