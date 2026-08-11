using Microsoft.EntityFrameworkCore;
using PurchaseOrderApp.Application.Ports;
using PurchaseOrderApp.Application.UseCases;
using PurchaseOrderApp.Infrastructure.Persistence;
using PurchaseOrderApp.Infrastructure.Persistence.Repositories;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PurchaseOrderApp.Api.Configuration;

public static class ApiAppServices
{
    public static IServiceCollection AddAppServices(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddControllerServices()
            .AddDatabase(configuration)
            .AddUseCaseServices()
            .AddPersistenceServices();

        return services;
    }

    private static IServiceCollection AddControllerServices(this IServiceCollection services)
    {
        services.AddControllers()
            .AddJsonOptions(options => {
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase, allowIntegerValues: false));
            });

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

    private static IServiceCollection AddUseCaseServices(this IServiceCollection services)
    {
        services.AddScoped<IFinanceService, FinanceService>();
        services.AddScoped<IPurchaseOrderService, PurchaseOrderService>();
        services.AddScoped<IReservationService, ReservationService>();

        return services;
    }
}
