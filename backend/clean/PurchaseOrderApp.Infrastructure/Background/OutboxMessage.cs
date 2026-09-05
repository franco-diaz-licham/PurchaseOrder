namespace PurchaseOrderApp.Infrastructure.Background;

public sealed class OutboxMessage
{
    private OutboxMessage() { }

    private OutboxMessage(
        Guid id,
        string messageType,
        string entityType,
        Guid entityId,
        string payload,
        DateTimeOffset occurredUtc,
        string correlationId,
        Guid? actorUserId,
        string? idempotencyKey,
        DateTimeOffset createdUtc)
    {
        Id = id;
        MessageType = Required(messageType, nameof(messageType));
        EntityType = Required(entityType, nameof(entityType));
        EntityId = entityId == Guid.Empty ? throw new ArgumentException("Entity id is required.", nameof(entityId)) : entityId;
        Payload = string.IsNullOrWhiteSpace(payload) ? "{}" : payload;
        OccurredUtc = occurredUtc;
        CorrelationId = Required(correlationId, nameof(correlationId));
        ActorUserId = actorUserId;
        IdempotencyKey = string.IsNullOrWhiteSpace(idempotencyKey) ? null : idempotencyKey.Trim();
        CreatedUtc = createdUtc;
    }

    public Guid Id { get; private set; }
    public string MessageType { get; private set; } = default!;
    public string EntityType { get; private set; } = default!;
    public Guid EntityId { get; private set; }
    public string Payload { get; private set; } = "{}";
    public DateTimeOffset OccurredUtc { get; private set; }
    public string CorrelationId { get; private set; } = default!;
    public Guid? ActorUserId { get; private set; }
    public string? IdempotencyKey { get; private set; }
    public string? HangfireJobId { get; private set; }
    public DateTimeOffset? PublishedUtc { get; private set; }
    public DateTimeOffset CreatedUtc { get; private set; }

    public static OutboxMessage Create(
        string messageType,
        string entityType,
        Guid entityId,
        string payload,
        DateTimeOffset occurredUtc,
        string correlationId,
        string? idempotencyKey)
    {
        var now = DateTimeOffset.UtcNow;

        return new OutboxMessage(
            Guid.NewGuid(),
            messageType,
            entityType,
            entityId,
            payload,
            occurredUtc.ToUniversalTime(),
            correlationId,
            actorUserId: null,
            idempotencyKey,
            now);
    }

    private static string Required(string value, string name)
    {
        if (string.IsNullOrWhiteSpace(value)) throw new ArgumentException($"{name} is required.", name);
        return value.Trim();
    }
}
