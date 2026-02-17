using GroceryStore.Domain.Common;

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
        ValidateNonNegative(caloriesKcal,nameof(caloriesKcal));
        ValidateNonNegative(proteinG,nameof(proteinG));
        ValidateNonNegative(carbsG,nameof(carbsG));
        ValidateNonNegative(fatG,nameof(fatG));
        ValidateNonNegative(saltG,nameof(saltG));

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

    private static void ValidateNonNegative(decimal? value,string fieldName)
    {
        if (value.HasValue)
            Guard.NonNegative(value.Value,fieldName);
    }
}