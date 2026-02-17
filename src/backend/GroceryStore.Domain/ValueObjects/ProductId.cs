namespace GroceryStore.Domain.ValueObjects;

/// <summary>
/// Strongly-typed identifier for the Product aggregate.
/// </summary>
public sealed record ProductId
{
    public Guid Value { get; }

    private ProductId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("ProductId cannot be empty.", nameof(value));

        Value = value;
    }

    public static ProductId Create(Guid value) => new(value);
    public static ProductId CreateNew() => new(Guid.NewGuid());

    public override string ToString() => Value.ToString();

    // Implicit conversions for convenience at the edges
    public static implicit operator Guid(ProductId id) => id.Value;
    public static implicit operator ProductId(Guid id) => new(id);
}
