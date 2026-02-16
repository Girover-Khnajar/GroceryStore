namespace GroceryStore.Domain.Common.DomainEvents;

/// <summary>
/// Entity that can raise domain events.
/// </summary>
public abstract class BaseEntity : Entity
{
    private readonly List<IDomainEvent> _domainEvents = new( );

    public IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents;

    protected void AddDomainEvent(IDomainEvent domainEvent) => _domainEvents.Add(domainEvent);

    public void ClearDomainEvents() => _domainEvents.Clear( );
}