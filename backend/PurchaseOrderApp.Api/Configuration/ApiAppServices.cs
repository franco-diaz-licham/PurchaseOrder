using Microsoft.EntityFrameworkCore;
using PurchaseOrderApp.Application.Ports;
using PurchaseOrderApp.Infrastructure.Persistence;
using PurchaseOrderApp.Infrastructure.Persistence.Repositories;

namespace PurchaseOrderApp.Api.Configuration;

public static class ApiAppServices
{
    public static IServiceCollection AddAppServices(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddDatabase(configuration)
            .AddPersistenceServices();

        return services;
    }

    private static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Db");

        if (string.IsNullOrWhiteSpace(connectionString)) throw new InvalidOperationException("ConnectionStrings:Db is required.");
        services.AddDbContext<DatabaseContext>(options => {
            options.UseNpgsql(connectionString);
        });

        return services;
    }

    private static IServiceCollection AddPersistenceServices(this IServiceCollection services)
    {
        services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IAuditLogRepository, AuditLogRepository>();
        services.AddScoped<IFinanceQueryRepository, FinanceRepository>();
        services.AddScoped<IInventoryItemRepository, InventoryItemRepository>();
        services.AddScoped<IPurchaseOrderRepository, PurchaseOrderRepository>();
        services.AddScoped<IStockReservationRepository, StockReservationRepository>();
        services.AddScoped<IWarehouseRepository, WarehouseRepository>();
        services.AddScoped<IWarehouseStockRepository, WarehouseStockRepository>();

        return services;
    }
}
