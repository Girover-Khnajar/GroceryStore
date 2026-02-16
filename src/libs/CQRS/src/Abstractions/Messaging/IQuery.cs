namespace CQRS.Abstractions.Messaging;

/// <summary>
/// Marker interface for queries that return a typed response
/// </summary>
/// <typeparam name="TResponse">The type of the query result</typeparam>
public interface IQuery<TResponse> : IMessage;
