using CQRS.CqrsResult;

namespace CQRS.Abstractions.Messaging;

/// <summary>
/// Optional validation abstraction.
/// Implementations are discovered via assembly scanning when using AddCqrs(...).
/// </summary>
/// <typeparam name="TMessage">The message type to validate.</typeparam>
public interface IValidator<in TMessage> where TMessage : IMessage
{
    /// <summary>
    /// Validates the message.
    /// Return an empty collection if the message is valid.
    /// </summary>
    Task<IReadOnlyList<Error>> ValidateAsync(TMessage message, CancellationToken cancellationToken = default);
}
