using GroceryStore.Domain.Exceptions;

namespace GroceryStore.Domain.Common;

/// <summary>
/// Simple guard helpers for domain invariants.
/// </summary>
public static class Guard
{
    public static string NotEmpty(string value,string fieldName,int maxLen = 0)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ValidationException($"{fieldName} is required.");

        value = value.Trim( );

        if (maxLen > 0 && value.Length > maxLen)
            throw new ValidationException($"{fieldName} must be at most {maxLen} characters.");

        return value;
    }

    public static void InRange(int value,string fieldName,int min,int max)
    {
        if (value < min || value > max)
            throw new ValidationException($"{fieldName} must be between {min} and {max}.");
    }

    public static void NonNegative(decimal value,string fieldName)
    {
        if (value < 0)
            throw new ValidationException($"{fieldName} must be non-negative.");
    }

    public static void NotNull(object? obj,string fieldName)
    {
        if (obj is null)
            throw new ValidationException($"{fieldName} is required.");
    }
}
