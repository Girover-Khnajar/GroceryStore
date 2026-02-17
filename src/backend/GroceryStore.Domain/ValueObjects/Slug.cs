using GroceryStore.Domain.Exceptions;
using System.Text;
using System.Text.RegularExpressions;

namespace GroceryStore.Domain.ValueObjects;

/// <summary>
/// URL-friendly slug value object.
/// </summary>
public sealed record Slug
{
    private static readonly Regex Allowed = new("^[a-z0-9]+(?:-[a-z0-9]+)*$",RegexOptions.Compiled);

    public string Value { get; }

    private Slug(string value) => Value = value;

    public static Slug Create(string input)
    {
        ValidationException.ThrowIfNullOrWhiteSpace(input);

        ValidationException.ThrowIfTooLong(input, maxLen: 120);
        input = input.Trim().ToLowerInvariant();

        // Basic slugify:
        // - replace spaces/underscores with dash
        // - remove invalid chars
        var sb = new StringBuilder(input.Length);
        foreach (var ch in input.Replace('_',' '))
        {
            if (char.IsLetterOrDigit(ch))
                sb.Append(ch);
            else if (char.IsWhiteSpace(ch) || ch == '-')
                sb.Append('-');
        }

        var slug = Regex.Replace(sb.ToString( ),"-{2,}","-").Trim('-');

        if (string.IsNullOrWhiteSpace(slug) || !Allowed.IsMatch(slug))
            throw new ValidationException("Invalid slug format.");

        return new Slug(slug);
    }

    public override string ToString() => Value;
}
