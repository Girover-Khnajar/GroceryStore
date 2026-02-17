using FluentAssertions;
using GroceryStore.Domain.Exceptions;
using GroceryStore.Domain.ValueObjects;

namespace GroceryStore.Domain.Tests.ValueObjects;

public class NutritionFactsTests
{
    // ───────────────── Creation ─────────────────

    [Fact]
    public void Create_AllNulls_IsAllowed()
    {
        var facts = NutritionFacts.Create(null, null, null, null, null);

        facts.CaloriesKcal.Should().BeNull();
        facts.ProteinG.Should().BeNull();
        facts.CarbsG.Should().BeNull();
        facts.FatG.Should().BeNull();
        facts.SaltG.Should().BeNull();
    }

    [Fact]
    public void Create_ValidValues_StoresCorrectly()
    {
        var facts = NutritionFacts.Create(250, 10.5m, 30, 8, 1.2m);

        facts.CaloriesKcal.Should().Be(250);
        facts.ProteinG.Should().Be(10.5m);
        facts.CarbsG.Should().Be(30);
        facts.FatG.Should().Be(8);
        facts.SaltG.Should().Be(1.2m);
    }

    [Fact]
    public void Create_ZeroValues_IsAllowed()
    {
        var act = () => NutritionFacts.Create(0, 0, 0, 0, 0);

        act.Should().NotThrow();
    }

    // ───────────────── Validation ─────────────────

    [Fact]
    public void Create_NegativeCalories_Throws()
    {
        var act = () => NutritionFacts.Create(-1, null, null, null, null);

        act.Should().Throw<ValidationException>();
    }

    [Fact]
    public void Create_NegativeProtein_Throws()
    {
        var act = () => NutritionFacts.Create(null, -0.1m, null, null, null);

        act.Should().Throw<ValidationException>();
    }

    [Fact]
    public void Create_NegativeCarbs_Throws()
    {
        var act = () => NutritionFacts.Create(null, null, -1, null, null);

        act.Should().Throw<ValidationException>();
    }

    [Fact]
    public void Create_NegativeFat_Throws()
    {
        var act = () => NutritionFacts.Create(null, null, null, -1, null);

        act.Should().Throw<ValidationException>();
    }

    [Fact]
    public void Create_NegativeSalt_Throws()
    {
        var act = () => NutritionFacts.Create(null, null, null, null, -1);

        act.Should().Throw<ValidationException>();
    }

    // ───────────────── Value equality ─────────────────

    [Fact]
    public void Equality_SameValues_AreEqual()
    {
        var a = NutritionFacts.Create(100, 5, 20, 3, 0.5m);
        var b = NutritionFacts.Create(100, 5, 20, 3, 0.5m);

        a.Should().Be(b);
    }

    [Fact]
    public void Equality_DifferentValues_NotEqual()
    {
        var a = NutritionFacts.Create(100, 5, 20, 3, 0.5m);
        var b = NutritionFacts.Create(200, 5, 20, 3, 0.5m);

        a.Should().NotBe(b);
    }
}
