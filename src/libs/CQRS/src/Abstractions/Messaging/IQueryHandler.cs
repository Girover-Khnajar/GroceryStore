using CQRS.CqrsResult;

namespace CQRS.Abstractions.Messaging;

/// <summary>
/// Defines a handler for queries that return a typed value
/// </summary>
/// <typeparam name="TQuery">The type of query being handled</typeparam>
/// <typeparam name="TResponse">The type of response returned</typeparam>
public interface IQueryHandler<in TQuery, TResponse>
    where TQuery : IQuery<TResponse>
{
    /// <summary>
    /// Handles the query execution
    /// </summary>
    /// <param name="query">The query to handle</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A result containing the response or error information</returns>
    Task<Result<TResponse>> HandleAsync(TQuery query, CancellationToken cancellationToken = default);
}
