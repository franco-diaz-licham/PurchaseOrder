using System.Diagnostics;
using System.Globalization;
using System.Text.Json;
using PurchaseOrderApp.Domain.Events;
using PurchaseOrderApp.Domain.ValueObjects;

namespace PurchaseOrderApp.Infrastructure.Background;

internal static class AuditLogOutboxMapper
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public const string StockReservedMessageType = "AuditLog.StockReserved";
    public const string StockReleasedMessageType = "AuditLog.StockReleased";
    public const string StockReservationEntityType = "StockReservation";

    public static OutboxMessage ToOutboxMessage(StockReservedEvent domainEvent)
    {
        var payload = new AuditLogOutboxPayload(
            domainEvent.StockReservationId.Value,
            domainEvent.PurchaseOrderLineId.Value,
            domainEvent.WarehouseId.Value,
            domainEvent.InventoryItemId.Value,
            domainEvent.QuantityReserved.Value,
            domainEvent.ResultingAvailableQuantity.Value,
            domainEvent.User,
            domainEvent.OccurredAt);

        return OutboxMessage.Create(
            StockReservedMessageType,
            StockReservationEntityType,
            domainEvent.StockReservationId.Value,
            JsonSerializer.Serialize(payload, JsonOptions),
            domainEvent.OccurredAt,
            GetCorrelationId(),
            $"audit:stock-reserved:{domainEvent.StockReservationId.Value}");
    }

    public static OutboxMessage ToOutboxMessage(StockReleasedEvent domainEvent)
    {
        var payload = new AuditLogOutboxPayload(
            domainEvent.StockReservationId.Value,
            domainEvent.PurchaseOrderLineId.Value,
            domainEvent.WarehouseId.Value,
            domainEvent.InventoryItemId.Value,
            domainEvent.QuantityReleased.Value,
            domainEvent.ResultingAvailableQuantity.Value,
            domainEvent.User,
            domainEvent.OccurredAt);

        return OutboxMessage.Create(
            StockReleasedMessageType,
            StockReservationEntityType,
            domainEvent.StockReservationId.Value,
            JsonSerializer.Serialize(payload, JsonOptions),
            domainEvent.OccurredAt,
            GetCorrelationId(),
            $"audit:stock-released:{domainEvent.StockReservationId.Value}:{domainEvent.OccurredAt.UtcTicks}:{domainEvent.QuantityReleased.Value.ToString(CultureInfo.InvariantCulture)}");
    }

    public static StockReservedEvent ToStockReservedEvent(string payload)
    {
        var message = Deserialize(payload);

        return new StockReservedEvent(
            new StockReservationId(message.StockReservationId),
            new PurchaseOrderLineId(message.PurchaseOrderLineId),
            new WarehouseId(message.WarehouseId),
            new InventoryItemId(message.InventoryItemId),
            new Quantity(message.Quantity),
            new Quantity(message.ResultingAvailableQuantity),
            message.User,
            message.OccurredAt);
    }

    public static StockReleasedEvent ToStockReleasedEvent(string payload)
    {
        var message = Deserialize(payload);

        return new StockReleasedEvent(
            new StockReservationId(message.StockReservationId),
            new PurchaseOrderLineId(message.PurchaseOrderLineId),
            new WarehouseId(message.WarehouseId),
            new InventoryItemId(message.InventoryItemId),
            new Quantity(message.Quantity),
            new Quantity(message.ResultingAvailableQuantity),
            message.User,
            message.OccurredAt);
    }

    private static AuditLogOutboxPayload Deserialize(string payload)
    {
        return JsonSerializer.Deserialize<AuditLogOutboxPayload>(payload, JsonOptions)
            ?? throw new InvalidOperationException("Audit log outbox payload is empty.");
    }

    private static string GetCorrelationId()
    {
        return Activity.Current?.TraceId.ToString() ?? Guid.NewGuid().ToString("N");
    }

    private sealed record AuditLogOutboxPayload(
        Guid StockReservationId,
        Guid PurchaseOrderLineId,
        Guid WarehouseId,
        Guid InventoryItemId,
        decimal Quantity,
        decimal ResultingAvailableQuantity,
        string User,
        DateTimeOffset OccurredAt);
}
