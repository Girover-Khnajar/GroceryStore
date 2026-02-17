namespace GroceryStore.Domain.ValueObjects;

/// <summary>
/// Strongly-typed identifier for the ImageAsset aggregate.
/// Shared across Catalog and Media bounded contexts as a cross-aggregate reference.
/// </summary>
public sealed record ImageId
{
    public Guid Value { get; }

    private ImageId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("ImageId cannot be empty.", nameof(value));

        Value = value;
    }

    public static ImageId Create(Guid value) => new(value);
    public static ImageId CreateNew() => new(Guid.NewGuid());

    public override string ToString() => Value.ToString();

    // Implicit conversions for convenience at the edges
    public static implicit operator Guid(ImageId id) => id.Value;
    public static implicit operator ImageId(Guid id) => new(id);
}
