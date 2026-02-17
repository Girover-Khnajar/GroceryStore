using FluentAssertions;
using GroceryStore.Domain.Exceptions;
using GroceryStore.Domain.ValueObjects;

namespace GroceryStore.Domain.Tests.ValueObjects;

public class MoneyTests
{
    // ───────────────── Creation ─────────────────

    [Fact]
    public void Create_ValidAmountAndCurrency_ReturnsInstance()
    {
        var money = Money.Create(9.99m, "CHF");

        money.Amount.Should().Be(9.99m);
        money.Currency.Should().Be("CHF");
    }

    [Fact]
    public void Create_RoundsToTwoDecimalPlaces()
    {
        var money = Money.Create(1.999m, "EUR");

        money.Amount.Should().Be(2.00m);
    }

    [Fact]
    public void Create_NormalizeCurrencyToUpperCase()
    {
        var money = Money.Create(5m, "chf");

        money.Currency.Should().Be("CHF");
    }

    [Fact]
    public void Create_ZeroAmount_IsAllowed()
    {
        var act = () => Money.Create(0, "USD");

        act.Should().NotThrow();
    }

    // ───────────────── Validation ─────────────────

    [Fact]
    public void Create_NegativeAmount_Throws()
    {
        var act = () => Money.Create(-1m, "CHF");

        act.Should().Throw<ValidationException>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_NullOrEmptyCurrency_Throws(string? currency)
    {
        var act = () => Money.Create(10, currency!);

        act.Should().Throw<ValidationException>();
    }

    [Fact]
    public void Create_CurrencyTooLong_Throws()
    {
        var act = () => Money.Create(10, "ABCD");

        act.Should().Throw<ValidationException>();
    }

    // ───────────────── Value equality ─────────────────

    [Fact]
    public void Equality_SameAmountAndCurrency_AreEqual()
    {
        var a = Money.Create(10, "CHF");
        var b = Money.Create(10, "CHF");

        a.Should().Be(b);
    }

    [Fact]
    public void Equality_DifferentAmount_NotEqual()
    {
        var a = Money.Create(10, "CHF");
        var b = Money.Create(20, "CHF");

        a.Should().NotBe(b);
    }

    [Fact]
    public void Equality_DifferentCurrency_NotEqual()
    {
        var a = Money.Create(10, "CHF");
        var b = Money.Create(10, "EUR");

        a.Should().NotBe(b);
    }

    // ───────────────── ToString ─────────────────

    [Fact]
    public void ToString_FormatsCorrectly()
    {
        var money = Money.Create(9.5m, "CHF");

        money.ToString().Should().Be("9.50 CHF");
    }
}
