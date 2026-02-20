using CQRS.CqrsResult;

namespace GroceryStore.Api.Extensions;

public static class ResultExtensions
{
    public static IResult ToHttpResult(this Result result)
    {
        if (result.IsSuccess)
            return Results.NoContent();

        return ToProblemResult(result.Errors);
    }

    public static IResult ToHttpResult<T>(this Result<T> result)
    {
        if (result.IsSuccess)
            return Results.Ok(result.Value);

        return ToProblemResult(result.Errors);
    }

    public static IResult ToCreatedHttpResult<T>(
        this Result<T> result,
        string routeName,
        Func<T, object> routeValues)
    {
        if (result.IsSuccess)
            return Results.CreatedAtRoute(routeName, routeValues(result.Value!), result.Value);

        return ToProblemResult(result.Errors);
    }

    public static IResult ToCreatedHttpResult(
        this Result<Guid> result,
        string routeName)
    {
        if (result.IsSuccess)
            return Results.CreatedAtRoute(routeName, new { id = result.Value }, new { Id = result.Value });

        return ToProblemResult(result.Errors);
    }

    private static IResult ToProblemResult(IReadOnlyList<Error> errors)
    {
        if (errors.Count == 0)
            return Results.Problem(statusCode: 500);

        var firstError = errors[0];

        var statusCode = firstError.Type switch
        {
            ErrorType.Badrequest => StatusCodes.Status400BadRequest,
            ErrorType.Validation => StatusCodes.Status422UnprocessableEntity,
            ErrorType.NotFound => StatusCodes.Status404NotFound,
            ErrorType.Conflict => StatusCodes.Status409Conflict,
            ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
            ErrorType.Forbidden => StatusCodes.Status403Forbidden,
            ErrorType.Failure => StatusCodes.Status500InternalServerError,
            ErrorType.Unexpected => StatusCodes.Status500InternalServerError,
            _ => StatusCodes.Status500InternalServerError
        };

        if (firstError.Metadata is not null
            && firstError.Metadata.TryGetValue("statusCode", out var statusCodeObj)
            && statusCodeObj is int statusCodeOverride)
        {
            statusCode = statusCodeOverride;
        }

        if (errors.Count == 1)
        {
            return Results.Problem(
                statusCode: statusCode,
                title: GetTitle(firstError.Type),
                detail: firstError.Message,
                extensions: new Dictionary<string, object?>
                {
                    { "code", firstError.Code }
                });
        }

        return Results.Problem(
            statusCode: statusCode,
            title: GetTitle(firstError.Type),
            extensions: new Dictionary<string, object?>
            {
                {
                    "errors", errors.Select(e => new
                    {
                        e.Code,
                        e.Message,
                        Type = e.Type.ToString()
                    }).ToArray()
                }
            });
    }

    private static string GetTitle(ErrorType type) => type switch
    {
        ErrorType.Badrequest => "Bad Request",
        ErrorType.Validation => "Validation Error",
        ErrorType.NotFound => "Not Found",
        ErrorType.Conflict => "Conflict",
        ErrorType.Unauthorized => "Unauthorized",
        ErrorType.Forbidden => "Forbidden",
        ErrorType.Failure => "Server Error",
        ErrorType.Unexpected => "Unexpected Error",
        _ => "An error occurred"
    };
}
