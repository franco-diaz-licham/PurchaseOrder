using Microsoft.EntityFrameworkCore;
using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.Extensions.Options;
using PurchaseOrderApp.Application.Ports;
using PurchaseOrderApp.Application.UseCases;
using PurchaseOrderApp.Infrastructure;
using PurchaseOrderApp.Infrastructure.Background;
using PurchaseOrderApp.Infrastructure.Repositories;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PurchaseOrderApp.Api.Configuration;

public static class ApiAppServices
{
    public static IServiceCollection AddAppServices(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddControllerServices()
            .AddCorsServices(configuration)
            .AddUseCaseServices()
            .AddPersistenceServices(configuration)
            .AddBackgroundProcessingServices(configuration);

        return services;
    }

    private static IServiceCollection AddControllerServices(this IServiceCollection services)
    {
        services.AddControllers(options => {
            options.Conventions.Add(new RouteTokenTransformerConvention(new RouteTransformer()));
        }).AddJsonOptions(options => {
            options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase, allowIntegerValues: false));
        });

        return services;
    }

    private static IServiceCollection AddCorsServices(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddOptions<CorsOptions>()
            .Bind(configuration.GetSection(CorsOptions.SectionName))
            .Validate(options => options.AllowedOrigins.All(origin => !string.IsNullOrWhiteSpace(origin)), "Cors origins cannot be empty.")
            .ValidateOnStart();

        var corsOptions = configuration.GetSection(CorsOptions.SectionName).Get<CorsOptions>() ?? new CorsOptions();

        services.AddCors(options => {
            options.AddPolicy(CorsOptions.PolicyName, policy => {
                policy
                    .WithOrigins(corsOptions.AllowedOrigins)
                    .AllowAnyHeader()
                    .AllowAnyMethod();
            });
        });

        return services;
    }

    private static IServiceCollection AddPersistenceServices(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddOptions<DatabaseOptions>()
            .Bind(configuration.GetSection(DatabaseOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddDbContext<DatabaseContext>((sp, options) => {
            var databaseOptions = sp.GetRequiredService<IOptions<DatabaseOptions>>().Value;
            options.UseNpgsql(databaseOptions.PurchaseOrderDb)
                .UseSnakeCaseNamingConvention();
        });

        services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();
        services.AddScoped<IOutboxProcessor, OutboxProcessor>();
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

    private static IServiceCollection AddBackgroundProcessingServices(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddOptions<BackgroundProcessingOptions>()
            .Bind(configuration.GetSection(BackgroundProcessingOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        var backgroundOptions = configuration
            .GetSection(BackgroundProcessingOptions.SectionName)
            .Get<BackgroundProcessingOptions>() ?? new BackgroundProcessingOptions();

        if (!backgroundOptions.Enabled) return services;

        services.AddHangfire((sp, hangfire) => {
            var databaseOptions = sp.GetRequiredService<IOptions<DatabaseOptions>>().Value;
            var storageOptions = new PostgreSqlStorageOptions {
                SchemaName = "hangfire"
            };

            hangfire
                .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UsePostgreSqlStorage(
                    options => options.UseNpgsqlConnection(databaseOptions.PurchaseOrderDb),
                    storageOptions);
        });

        services.AddHangfireServer(options => {
            options.WorkerCount = backgroundOptions.WorkerCount;
            options.Queues = ["default"];
        });

        return services;
    }

    private static IServiceCollection AddUseCaseServices(this IServiceCollection services)
    {
        services.AddScoped<IAuditLogService, AuditLogService>();
        services.AddScoped<IFinanceService, FinanceService>();
        services.AddScoped<IInventoryItemService, InventoryItemService>();
        services.AddScoped<IPurchaseOrderService, PurchaseOrderService>();
        services.AddScoped<IReservationService, ReservationService>();
        services.AddScoped<IWarehouseService, WarehouseService>();
        services.AddScoped<IWarehouseStockService, WarehouseStockService>();

        return services;
    }
}
