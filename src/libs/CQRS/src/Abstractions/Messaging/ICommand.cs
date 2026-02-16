namespace CQRS.Abstractions.Messaging;

public interface IMessage;
/// <summary>
/// Marker interface for commands without a response (void operations)
/// </summary>
public interface ICommand : IMessage;

/// <summary>
/// Marker interface for commands that return a typed response
/// </summary>
/// <typeparam name="TResponse">The type of the command result</typeparam>
public interface ICommand<TResponse> : IMessage;
