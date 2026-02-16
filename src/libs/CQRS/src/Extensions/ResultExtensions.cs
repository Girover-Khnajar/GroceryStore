// using FluentResults;
// using Microsoft.AspNetCore.Http;
// using Microsoft.AspNetCore.Mvc;

// namespace CQRS.Extensions;
// /// <summary>
// /// Extension methods for converting FluentResults to HTTP responses
// /// </summary>
// public static class ResultExtensions
// {
//     /// <summary>
//     /// Converts a Result to an IResult for Minimal APIs
//     /// </summary>
//     public static IResult ToHttpResult(this Result result)
//     {
//         if (result.IsSuccess)
//         {
//             return Results.NoContent();
//         }

//         return CreateProblemResult(result.Errors);
//     }

//     /// <summary>
//     /// Converts a Result<T> to an IResult for Minimal APIs
//     /// </summary>
//     public static IResult ToHttpResult<T>(this Result<T> result)
//     {
//         if (result.IsSuccess)
//         {
//             return Results.Ok(result.Value);
//         }

//         return CreateProblemResult(result.Errors);
//     }

//     /// <summary>
//     /// Matches the result to different outcomes for Minimal APIs
//     /// </summary>
//     public static IResult Match<T>(
//         this Result<T> result,
//         Func<T, IResult> onSuccess,
//         Func<List<IError>, IResult> onFailure)
//     {
//         return result.IsSuccess ? onSuccess(result.Value) : onFailure(result.Errors);
//     }

//     /// <summary>
//     /// Matches the result to different outcomes for Minimal APIs
//     /// </summary>
//     public static IResult Match(
//         this Result result,
//         Func<IResult> onSuccess,
//         Func<List<IError>, IResult> onFailure)
//     {
//         return result.IsSuccess ? onSuccess() : onFailure(result.Errors);
//     }

//     /// <summary>
//     /// Converts a Result to an ActionResult for MVC controllers
//     /// </summary>
//     public static ActionResult ToActionResult(this Result result)
//     {
//         if (result.IsSuccess)
//         {
//             return new NoContentResult();
//         }

//         return CreateProblemActionResult(result.Errors);
//     }

//     /// <summary>
//     /// Converts a Result<T> to an ActionResult<T> for MVC controllers
//     /// </summary>
//     public static ActionResult<T> ToActionResult<T>(this Result<T> result)
//     {
//         if (result.IsSuccess)
//         {
//             return new OkObjectResult(result.Value);
//         }

//         return CreateProblemActionResult(result.Errors);
//     }

//     private static IResult CreateProblemResult(List<IError> errors)
//     {
//         var firstError = errors.FirstOrDefault();
        
//         if (firstError?.Metadata.TryGetValue("ErrorType", out var errorType) == true)
//         {
//             return errorType switch
//             {
//                 "NotFound" => Results.NotFound(new ProblemDetails
//                 {
//                     Title = "Not Found",
//                     Detail = firstError.Message,
//                     Status = StatusCodes.Status404NotFound
//                 }),
//                 "Validation" => Results.ValidationProblem(
//                     GetValidationErrors(firstError),
//                     detail: firstError.Message,
//                     title: "Validation Error"),
//                 "Conflict" => Results.Conflict(new ProblemDetails
//                 {
//                     Title = "Conflict",
//                     Detail = firstError.Message,
//                     Status = StatusCodes.Status409Conflict
//                 }),
//                 "Unauthorized" => Results.Problem(
//                     detail: firstError.Message,
//                     statusCode: StatusCodes.Status401Unauthorized,
//                     title: "Unauthorized"),
//                 "Forbidden" => Results.Problem(
//                     detail: firstError.Message,
//                     statusCode: StatusCodes.Status403Forbidden,
//                     title: "Forbidden"),
//                 "BusinessRule" => Results.UnprocessableEntity(new ProblemDetails
//                 {
//                     Title = "Business Rule Violation",
//                     Detail = firstError.Message,
//                     Status = StatusCodes.Status422UnprocessableEntity
//                 }),
//                 _ => Results.Problem(
//                     detail: firstError.Message,
//                     title: "Error",
//                     statusCode: StatusCodes.Status500InternalServerError)
//             };
//         }

//         return Results.Problem(
//             detail: string.Join("; ", errors.Select(e => e.Message)),
//             title: "Error",
//             statusCode: StatusCodes.Status500InternalServerError);
//     }

//     private static ActionResult CreateProblemActionResult(List<IError> errors)
//     {
//         var firstError = errors.FirstOrDefault();
        
//         if (firstError?.Metadata.TryGetValue("ErrorType", out var errorType) == true)
//         {
//             return errorType switch
//             {
//                 "NotFound" => new NotFoundObjectResult(new ProblemDetails
//                 {
//                     Title = "Not Found",
//                     Detail = firstError.Message,
//                     Status = StatusCodes.Status404NotFound
//                 }),
//                 "Validation" => new BadRequestObjectResult(new ValidationProblemDetails(GetValidationErrors(firstError))
//                 {
//                     Title = "Validation Error",
//                     Detail = firstError.Message
//                 }),
//                 "Conflict" => new ConflictObjectResult(new ProblemDetails
//                 {
//                     Title = "Conflict",
//                     Detail = firstError.Message,
//                     Status = StatusCodes.Status409Conflict
//                 }),
//                 "Unauthorized" => new UnauthorizedObjectResult(new ProblemDetails
//                 {
//                     Title = "Unauthorized",
//                     Detail = firstError.Message,
//                     Status = StatusCodes.Status401Unauthorized
//                 }),
//                 "Forbidden" => new ObjectResult(new ProblemDetails
//                 {
//                     Title = "Forbidden",
//                     Detail = firstError.Message,
//                     Status = StatusCodes.Status403Forbidden
//                 })
//                 { StatusCode = StatusCodes.Status403Forbidden },
//                 "BusinessRule" => new UnprocessableEntityObjectResult(new ProblemDetails
//                 {
//                     Title = "Business Rule Violation",
//                     Detail = firstError.Message,
//                     Status = StatusCodes.Status422UnprocessableEntity
//                 }),
//                 _ => new ObjectResult(new ProblemDetails
//                 {
//                     Title = "Error",
//                     Detail = firstError.Message,
//                     Status = StatusCodes.Status500InternalServerError
//                 })
//                 { StatusCode = StatusCodes.Status500InternalServerError }
//             };
//         }

//         return new ObjectResult(new ProblemDetails
//         {
//             Title = "Error",
//             Detail = string.Join("; ", errors.Select(e => e.Message)),
//             Status = StatusCodes.Status500InternalServerError
//         })
//         { StatusCode = StatusCodes.Status500InternalServerError };
//     }

//     private static Dictionary<string, string[]> GetValidationErrors(IError error)
//     {
//         if (error.Metadata.TryGetValue("ValidationErrors", out var validationErrors) 
//             && validationErrors is Dictionary<string, string[]> errors)
//         {
//             return errors;
//         }

//         return new Dictionary<string, string[]>();
//     }
// }