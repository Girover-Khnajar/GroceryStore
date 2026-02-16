namespace CQRS.Abstractions.Messaging;

/// <summary>
/// Optional authorization hook for a given message.
/// Register one or more <see cref="IAuthorizer{TMessage}"/> implementations to enforce access control.
/// </summary>
public interface IAuthorizer<in TMessage>
{
    Task<AuthorizationResult> AuthorizeAsync(TMessage message, CancellationToken cancellationToken = default);
}
