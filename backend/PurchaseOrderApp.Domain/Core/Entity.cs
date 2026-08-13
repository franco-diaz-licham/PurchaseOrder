namespace PurchaseOrderApp.Domain.Core;

/// <summary>
/// Base type for domain entities with lifecycle metadata and domain events.
/// </summary>
public abstract class Entity
{
    private readonly List<IDomainEvent> _domainEvents = [];

    /// <summary>
    /// Date and time the entity was first created.
    /// </summary>
    public DateTimeOffset CreatedAt { get; private set; }

    /// <summary>
    /// User or system actor that created the entity.
    /// </summary>
    public string CreatedBy { get; private set; } = default!;

    /// <summary>
    /// Date and time the entity was most recently changed.
    /// </summary>
    public DateTimeOffset? UpdatedAt { get; private set; }

    /// <summary>
    /// User or system actor that most recently changed the entity.
    /// </summary>
    public string? UpdatedBy { get; private set; }

    /// <summary>
    /// Domain events raised by the entity during the current unit of work.
    /// </summary>
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    /// <summary>
    /// Sets the initial lifecycle metadata for a newly created entity.
    /// </summary>
    protected void SetCreated(string user, DateTimeOffset occurredAt)
    {
        CreatedBy = Required(user, nameof(user));
        CreatedAt = occurredAt;
    }

    /// <summary>
    /// Sets the lifecycle metadata for the most recent entity change.
    /// </summary>
    protected void SetUpdated(string user, DateTimeOffset occurredAt)
    {
        UpdatedBy = Required(user, nameof(user));
        UpdatedAt = occurredAt;
    }

    /// <summary>
    /// Adds a domain event to be dispatched by the current unit of work.
    /// </summary>
    protected void RaiseDomainEvent(IDomainEvent domainEvent) => _domainEvents.Add(domainEvent);

    /// <summary>
    /// Clears domain events after they have been dispatched.
    /// </summary>
    public void ClearDomainEvents() => _domainEvents.Clear();

    /// <summary>
    /// Trims and validates required text values.
    /// </summary>
    protected static string Required(string value, string name)
    {
        if (string.IsNullOrWhiteSpace(value)) throw new DomainException($"{name} is required.");
        return value.Trim();
    }
}

/// <summary>
/// Base type for domain entities with a strongly typed primary identifier.
/// </summary>
public abstract class Entity<TId> : Entity where TId : notnull
{
    /// <summary>
    /// Strongly typed primary identifier for the entity.
    /// </summary>
    public TId Id { get; protected set; } = default!;
}
