namespace GroceryStore.Domain.Common.DomainEvents;

/// <summary>
/// Adds auditing fields.
/// </summary>
public abstract class AuditableEntity : BaseEntity
{
    public DateTime CreatedOnUtc { get; protected set; } = DateTime.UtcNow;
    public DateTime? ModifiedOnUtc { get; protected set; }

    protected void Touch() => ModifiedOnUtc = DateTime.UtcNow;
}