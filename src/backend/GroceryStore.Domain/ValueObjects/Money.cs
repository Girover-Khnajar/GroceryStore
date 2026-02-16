using GroceryStore.Domain.Common;

namespace GroceryStore.Domain.ValueObjects;

public sealed record Money
{
    public decimal Amount { get; }
    public string Currency { get; }

    private Money(decimal amount,string currency)
    {
        Guard.NonNegative(amount,nameof(amount));
        Currency = Guard.NotEmpty(currency,nameof(currency),maxLen: 3).ToUpperInvariant( );
        Amount = decimal.Round(amount,2,MidpointRounding.AwayFromZero);
    }

    public static Money Create(decimal amount,string currency) => new(amount,currency);

    public override string ToString() => $"{Amount:0.00} {Currency}";
}
