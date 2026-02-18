using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace GroceryStore.Domain.Exceptions;

public class DomainException : Exception
{
    public DomainException(string message) : base(message) { }
}

public sealed class ValidationException : DomainException
{
    public ValidationException(string message) : base(message) { }

    /// <summary>
    /// Throws if <paramref name="value"/> is null, empty, or whitespace.
    /// </summary>
    public static void ThrowIfNullOrWhiteSpace(
        [NotNull] string? value,
        [CallerArgumentExpression(nameof(value))] string? paramName = null)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ValidationException($"{paramName} is required.");
    }

    /// <summary>
    /// Throws if <paramref name="value"/> trimmed length exceeds <paramref name="maxLen"/>.
    /// </summary>
    public static void ThrowIfTooLong(
        string? value,
        int maxLen,
        [CallerArgumentExpression(nameof(value))] string? paramName = null)
    {
        if (string.IsNullOrWhiteSpace(value))
            return; // Null/empty is not a length concern — use ThrowIfNullOrWhiteSpace for that.

        if (value.Trim().Length > maxLen)
            throw new ValidationException($"{paramName} must be at most {maxLen} characters.");
    }

    /// <summary>
    /// Throws if <paramref name="value"/> is outside the inclusive range [<paramref name="min"/>, <paramref name="max"/>].
    /// </summary>
    public static void ThrowIfOutOfRange(
        int value,
        int min,
        int max,
        [CallerArgumentExpression(nameof(value))] string? paramName = null)
    {
        if (value < min || value > max)
            throw new ValidationException($"{paramName} must be between {min} and {max}.");
    }

    /// <summary>
    /// Throws if <paramref name="value"/> is negative.
    /// </summary>
    public static void ThrowIfNegative(
        decimal value,
        [CallerArgumentExpression(nameof(value))] string? paramName = null)
    {
        if (value < 0)
            throw new ValidationException($"{paramName} must be non-negative.");
    }

    /// <summary>
    /// Throws if <paramref name="obj"/> is null.
    /// </summary>
    public static void ThrowIfNull<T>(
        [NotNull] T? obj,
        [CallerArgumentExpression(nameof(obj))] string? paramName = null) where T : class
    {
        if (obj is null)
            throw new ValidationException($"{paramName} is required.");
    }

    public static void ThrowIfInvalid<T>(T value, Func<T, bool> predicate, string message)
    {
        if (!predicate(value))
            throw new ValidationException(message);
    }
}