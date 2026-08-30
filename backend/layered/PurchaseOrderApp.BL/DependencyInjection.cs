using Microsoft.Extensions.DependencyInjection;
using PurchaseOrderApp.BL.Policies;
using PurchaseOrderApp.BL.Workflows;
using PurchaseOrderApp.BL.Workflows.PurchaseOrders;
using PurchaseOrderApp.BL.Workflows.Reservations;

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
        services.AddScoped<TransactionRunner>();
        services.AddScoped<PurchaseOrderMutationRunner>();
        services.AddScoped<SubmitPurchaseOrderWorkflow>();
        services.AddScoped<AddPurchaseOrderLineWorkflow>();
        services.AddScoped<UpdatePurchaseOrderLineWorkflow>();
        services.AddScoped<RemovePurchaseOrderLineWorkflow>();
        services.AddScoped<ChangePurchaseOrderStatusWorkflow>();
        services.AddScoped<ReserveStockWorkflow>();
        services.AddScoped<ReleaseReservationWorkflow>();
        services.AddScoped<IAuditLogService, AuditLogService>();
        services.AddScoped<IFinanceService, FinanceService>();
        services.AddScoped<IInventoryItemService, InventoryItemService>();
        services.AddScoped<IPurchaseOrderWorkflowService, PurchaseOrderWorkflowService>();
        services.AddScoped<IReservationService, ReservationService>();
        services.AddScoped<IWarehouseService, WarehouseService>();
        services.AddScoped<IWarehouseStockService, WarehouseStockService>();

        return services;
    }
}
