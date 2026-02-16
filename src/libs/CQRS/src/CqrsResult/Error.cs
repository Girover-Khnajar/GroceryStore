namespace CQRS.CqrsResult;

public sealed class Error : IEquatable<Error>
{
    public string Code { get; }
    public string Message { get; }
    public ErrorType Type { get; }
    public Dictionary<string, object>? Metadata { get; }

    private Error(string code, string message, ErrorType type, Dictionary<string, object>? metadata = null)
    {
        Code = code;
        Message = message;
        Type = type;
        Metadata = metadata;
    }

    public static Error Failure(string code, string message, Dictionary<string, object>? metadata = null)
        => new(code, message, ErrorType.Failure, metadata);

    public static Error Validation(string message, Dictionary<string, object>? metadata = null)
        => new(ErrorCodes.ValidationError, message, ErrorType.Validation, metadata);

    public static Error BadRequest(string message, Dictionary<string, object>? metadata = null)
        => new (ErrorCodes.BadRequest, message, ErrorType.Badrequest, metadata);
    public static Error NotFound(string message, Dictionary<string, object>? metadata = null)
        => new(ErrorCodes.NotFound, message, ErrorType.NotFound, metadata);

    public static Error Conflict(string message, Dictionary<string, object>? metadata = null)
        => new(ErrorCodes.Conflict, message, ErrorType.Conflict, metadata);

    public static Error Unauthorized(string message, Dictionary<string, object>? metadata = null)
        => new(ErrorCodes.Unauthorized, message, ErrorType.Unauthorized, metadata);

    public static Error Forbidden(string message, Dictionary<string, object>? metadata = null)
        => new(ErrorCodes.Forbidden, message, ErrorType.Forbidden, metadata);

    public static Error Unexpected(string message, Dictionary<string, object>? metadata = null)
        => new(ErrorCodes.InternalServerError, message, ErrorType.Unexpected, metadata);

    public Error WithMetadata(string key, object value)
    {
        var newMetadata = Metadata != null 
            ? new Dictionary<string, object>(Metadata) 
            : [];
        
        newMetadata[key] = value;
        
        return new Error(Code, Message, Type, newMetadata);
    }

    public bool Equals(Error? other)
    {
        if (other is null)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return Code == other.Code && Type == other.Type;
    }

    public override bool Equals(object? obj) => obj is Error error && Equals(error);

    public override int GetHashCode() => HashCode.Combine(Code, Type);

    public static bool operator ==(Error? left, Error? right) => Equals(left, right);
    public static bool operator !=(Error? left, Error? right) => !Equals(left, right);

    public override string ToString() => $"[{Type}] {Code}: {Message}";
}