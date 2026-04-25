using CQRS.CqrsResult;
using Microsoft.AspNetCore.Mvc;

namespace GroceryStore.Web.Extensions;

/// <summary>
/// Translates CQRS <see cref="Result{T}"/> into MVC-friendly action results.
/// </summary>
public static class ResultExtensions
{
    /// <summary>
    /// Returns the value on success, or sets TempData["Error"] and returns null on failure.
    /// Controllers should check for null and redirect / return an error view.
    /// </summary>
    public static T? ValueOrDefault<T>(this Result<T> result, Controller controller)
    {
        if (result.IsSuccess)
            return result.Value;

        controller.TempData["Error"] = result.Errors.FirstOrDefault()?.Message ?? "An error occurred.";
        return default;
    }

    /// <summary>
    /// Sets TempData["Error"] from a non-generic Result and returns false on failure.
    /// </summary>
    public static bool Succeeded(this Result result, Controller controller)
    {
        if (result.IsSuccess)
            return true;

        controller.TempData["Error"] = result.Errors.FirstOrDefault()?.Message ?? "An error occurred.";
        return false;
    }

    /// <summary>
    /// Sets TempData["Error"] from a generic Result&lt;T&gt; and returns false on failure.
    /// </summary>
    public static bool Succeeded<T>(this Result<T> result, Controller controller)
    {
        if (result.IsSuccess)
            return true;

        controller.TempData["Error"] = result.Errors.FirstOrDefault()?.Message ?? "An error occurred.";
        return false;
    }

    /// <summary>
    /// Returns true when the first error is NotFound.
    /// </summary>
    public static bool IsNotFound(this Result result)
        => !result.IsSuccess
           && result.Errors.Count > 0
           && result.Errors[0].Type == ErrorType.NotFound;

    public static bool IsNotFound<T>(this Result<T> result)
        => !result.IsSuccess
           && result.Errors.Count > 0
           && result.Errors[0].Type == ErrorType.NotFound;
}
