using Microsoft.Extensions.DependencyInjection;
using PurchaseOrderApp.BL.Policies;
using PurchaseOrderApp.BL.Workflows;

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
