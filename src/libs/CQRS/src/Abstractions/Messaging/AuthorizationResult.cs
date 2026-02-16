using CQRS.CqrsResult;

namespace CQRS.Abstractions.Messaging;

/// <summary>
/// Represents the outcome of an authorization check.
/// </summary>
public sealed class AuthorizationResult
{
    public bool IsAuthorized { get; }
    public IReadOnlyList<Error> Errors { get; }

    private AuthorizationResult(bool isAuthorized, IReadOnlyList<Error> errors)
    {
        IsAuthorized = isAuthorized;
        Errors = errors;
    }

    public static AuthorizationResult Success() => new(true, Array.Empty<Error>());

    public static AuthorizationResult Fail(params Error[] errors)
        => new(false, errors);

    public static AuthorizationResult Fail(IEnumerable<Error> errors)
        => new(false, errors.ToArray());

    public static AuthorizationResult Unauthorized(string message = "Unauthorized")
        => new(false, new[] { Error.Unauthorized(message) });

    public static AuthorizationResult Forbidden(string message = "Forbidden")
        => new(false, new[] { Error.Forbidden(message) });
}
