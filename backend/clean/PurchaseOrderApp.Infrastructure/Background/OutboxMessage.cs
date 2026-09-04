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
        Status = OutboxMessageStatus.Pending;
        AttemptCount = 0;
        NextAttemptUtc = createdUtc;
        CreatedUtc = createdUtc;
        UpdatedUtc = createdUtc;
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
    public OutboxMessageStatus Status { get; private set; } = OutboxMessageStatus.Pending;
    public int AttemptCount { get; private set; }
    public DateTimeOffset NextAttemptUtc { get; private set; }
    public string? LockedBy { get; private set; }
    public DateTimeOffset? LockedUntilUtc { get; private set; }
    public DateTimeOffset? ProcessedUtc { get; private set; }
    public string? LastError { get; private set; }
    public DateTimeOffset CreatedUtc { get; private set; }
    public DateTimeOffset UpdatedUtc { get; private set; }

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

    public void MarkProcessed(DateTimeOffset processedUtc)
    {
        Status = OutboxMessageStatus.Processed;
        ProcessedUtc = processedUtc.ToUniversalTime();
        LockedBy = null;
        LockedUntilUtc = null;
        LastError = null;
        UpdatedUtc = ProcessedUtc.Value;
    }

    public void MarkFailed(string error, DateTimeOffset nowUtc, int maxAttempts)
    {
        var sanitizedError = SanitizeError(error);
        var nextAttemptDelay = TimeSpan.FromSeconds(Math.Min(Math.Pow(2, Math.Max(AttemptCount, 1)) * 15, 300));

        Status = AttemptCount >= maxAttempts
            ? OutboxMessageStatus.DeadLettered
            : OutboxMessageStatus.Failed;
        LastError = sanitizedError;
        LockedBy = null;
        LockedUntilUtc = null;
        NextAttemptUtc = nowUtc.ToUniversalTime().Add(nextAttemptDelay);
        UpdatedUtc = nowUtc.ToUniversalTime();
    }

    private static string Required(string value, string name)
    {
        if (string.IsNullOrWhiteSpace(value)) throw new ArgumentException($"{name} is required.", name);
        return value.Trim();
    }

    private static string SanitizeError(string error)
    {
        const int maxLength = 1000;
        var value = string.IsNullOrWhiteSpace(error) ? "Unknown outbox processing error." : error.Trim();
        return value.Length <= maxLength ? value : value[..maxLength];
    }
}
