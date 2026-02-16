namespace CQRS.CqrsResult;

public static class ErrorCodes
{
    public const string BadRequest = "bad_request";
    public const string NotFound = "not_found";
    public const string ValidationError = "validation_error";
    public const string Unauthorized = "unauthorized";
    public const string Forbidden = "forbidden";
    public const string Conflict = "conflict";
    public const string InternalServerError = "internal_server_error";

    public static class User
    {
        public const string UserNotFound = "user.not_found";
        public const string UserEmailAlreadyExists = "user.email.exists";
        public const string UserInvalidCredentials = "user.invalid_credentials";
    }
    
}
