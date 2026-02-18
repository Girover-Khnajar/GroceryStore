using GroceryStore.Domain.Exceptions;

namespace GroceryStore.Domain.ValueObjects;

public sealed record Money
{
    public decimal Amount { get; }
    public string Currency { get; }

    private Money(decimal amount,string currency)
    {
        ValidationException.ThrowIfNegative(amount);
        ValidationException.ThrowIfNullOrWhiteSpace(currency);

        ValidationException.ThrowIfTooLong(currency, maxLen: 3);
        Currency = currency.Trim().ToUpperInvariant();
        Amount = decimal.Round(amount,2,MidpointRounding.AwayFromZero);
    }

    public static Money Create(decimal amount,string currency) => new(amount,currency);

    public override string ToString() => $"{Amount:0.00} {Currency}";
}
