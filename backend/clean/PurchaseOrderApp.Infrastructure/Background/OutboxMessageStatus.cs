namespace PurchaseOrderApp.Infrastructure.Background;

public enum OutboxMessageStatus
{
    Unspecified = 0,
    Pending = 1,
    Processing = 2,
    Processed = 3,
    Failed = 4,
    DeadLettered = 5
}
