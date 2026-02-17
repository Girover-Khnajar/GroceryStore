namespace GroceryStore.Domain.Common.DomainEvents;

/// <summary>
/// Base entity with identity.
/// </summary>
public abstract class Entity
{
    public Guid Id { get; protected set; } = Guid.NewGuid( );

    public override bool Equals(object? obj)
        => obj is Entity other && Id == other.Id;

    public override int GetHashCode()
        => Id.GetHashCode( );
}