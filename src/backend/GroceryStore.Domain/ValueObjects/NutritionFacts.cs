using GroceryStore.Domain.Exceptions;

namespace GroceryStore.Domain.ValueObjects;

/// <summary>
/// Optional nutrition facts per 100g/100ml.
/// </summary>
public sealed record NutritionFacts
{
    public decimal? CaloriesKcal { get; }
    public decimal? ProteinG { get; }
    public decimal? CarbsG { get; }
    public decimal? FatG { get; }
    public decimal? SaltG { get; }

    private NutritionFacts(decimal? caloriesKcal,decimal? proteinG,decimal? carbsG,decimal? fatG,decimal? saltG)
    {
        ValidateNonNegative(caloriesKcal);
        ValidateNonNegative(proteinG);
        ValidateNonNegative(carbsG);
        ValidateNonNegative(fatG);
        ValidateNonNegative(saltG);

        CaloriesKcal = caloriesKcal;
        ProteinG = proteinG;
        CarbsG = carbsG;
        FatG = fatG;
        SaltG = saltG;
    }

    public static NutritionFacts Create(
        decimal? caloriesKcal,
        decimal? proteinG,
        decimal? carbsG,
        decimal? fatG,
        decimal? saltG)
        => new(caloriesKcal,proteinG,carbsG,fatG,saltG);

    private static void ValidateNonNegative(decimal? value)
    {
        if (value.HasValue)
            ValidationException.ThrowIfNegative(value.Value);
    }
}