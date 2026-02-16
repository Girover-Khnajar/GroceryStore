namespace CQRS.CqrsResult;

public enum ErrorType
{
    Badrequest,
    Validation,
    NotFound,
    Conflict,
    Unauthorized,
    Forbidden,
    Failure,
    Unexpected
}
