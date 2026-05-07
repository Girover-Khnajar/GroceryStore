using System.Globalization;
using System.Text.RegularExpressions;

namespace GroceryStore.Web.Extensions;

public static class OpeningHoursLocalization
{
    private static readonly (string Name, DayOfWeek Day)[] DayMappings =
    [
        ("Monday", DayOfWeek.Monday),
        ("Tuesday", DayOfWeek.Tuesday),
        ("Wednesday", DayOfWeek.Wednesday),
        ("Thursday", DayOfWeek.Thursday),
        ("Friday", DayOfWeek.Friday),
        ("Saturday", DayOfWeek.Saturday),
        ("Sunday", DayOfWeek.Sunday),
    ];

    private static readonly (string Name, DayOfWeek Day)[] ShortDayMappings =
    [
        ("Mon", DayOfWeek.Monday),
        ("Tue", DayOfWeek.Tuesday),
        ("Wed", DayOfWeek.Wednesday),
        ("Thu", DayOfWeek.Thursday),
        ("Fri", DayOfWeek.Friday),
        ("Sat", DayOfWeek.Saturday),
        ("Sun", DayOfWeek.Sunday),
    ];

    public static string LocalizeDayNames(string? openingHoursText, CultureInfo culture)
    {
        if (string.IsNullOrWhiteSpace(openingHoursText))
            return string.Empty;

        var localized = openingHoursText;

        foreach (var (name, day) in DayMappings)
        {
            var translated = culture.DateTimeFormat.GetDayName(day);
            localized = Regex.Replace(localized, $@"\b{name}\b", translated, RegexOptions.IgnoreCase);
        }

        foreach (var (name, day) in ShortDayMappings)
        {
            var translated = culture.DateTimeFormat.GetAbbreviatedDayName(day);
            localized = Regex.Replace(localized, $@"\b{name}\.?(?=\b)", translated, RegexOptions.IgnoreCase);
        }

        return localized;
    }
}