using Microsoft.Extensions.DependencyInjection;
using PurchaseOrderApp.BL.Commands.Inventory;
using PurchaseOrderApp.BL.Commands.PurchaseOrders;
using PurchaseOrderApp.BL.Commands.Reservations;
using PurchaseOrderApp.BL.Common;
using PurchaseOrderApp.BL.Policies;
using PurchaseOrderApp.BL.Queries.AuditLogs;
using PurchaseOrderApp.BL.Queries.Finance;
using PurchaseOrderApp.BL.Queries.Inventory;
using PurchaseOrderApp.BL.Queries.PurchaseOrders;
using PurchaseOrderApp.BL.Queries.Reservations;
using PurchaseOrderApp.BL.Queries.Warehouses;

namespace PurchaseOrderApp.BL;

public static class DependencyInjection
{
    public static IServiceCollection AddBusinessLayer(this IServiceCollection services)
    {
        services.AddScoped<InventoryQuantityPolicy>();
        services.AddScoped<PurchaseOrderLinePolicy>();
        services.AddScoped<PurchaseOrderPolicy>();
        services.AddScoped<ReservationPolicy>();
        services.AddScoped<StockAvailabilityPolicy>();
        services.AddScoped<TransactionCoordinator>();
        services.AddScoped<PurchaseOrderMutationCoordinator>();
        services.AddScoped<SubmitPurchaseOrderCommandHandler>();
        services.AddScoped<AddPurchaseOrderLineCommandHandler>();
        services.AddScoped<UpdatePurchaseOrderLineCommandHandler>();
        services.AddScoped<RemovePurchaseOrderLineCommandHandler>();
        services.AddScoped<ApprovePurchaseOrderCommandHandler>();
        services.AddScoped<ClosePurchaseOrderCommandHandler>();
        services.AddScoped<CancelPurchaseOrderCommandHandler>();
        services.AddScoped<ReserveStockCommandHandler>();
        services.AddScoped<ReleaseReservationCommandHandler>();
        services.AddScoped<ChangeInventoryItemStandardCostCommandHandler>();
        services.AddScoped<GetPurchaseOrderQueryHandler>();
        services.AddScoped<ListPurchaseOrderSummariesQueryHandler>();
        services.AddScoped<ListReservationsQueryHandler>();
        services.AddScoped<ListInventoryItemsQueryHandler>();
        services.AddScoped<ListWarehousesQueryHandler>();
        services.AddScoped<ListWarehouseStockQueryHandler>();
        services.AddScoped<ListAuditLogEntriesQueryHandler>();
        services.AddScoped<ListWarehouseCommittedStockValuesQueryHandler>();

        return services;
    }
}
