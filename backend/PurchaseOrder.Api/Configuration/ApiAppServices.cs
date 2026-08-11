using Microsoft.EntityFrameworkCore;
using PurchaseOrder.Application.Ports;
using PurchaseOrder.Infrastructure.Persistence;

namespace PurchaseOrder.Api.Configuration;

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

        if (string.IsNullOrWhiteSpace(connectionString)) {
            throw new InvalidOperationException("ConnectionStrings:Db is required.");
        }

        services.AddDbContext<DatabaseContext>(options => {
            options.UseNpgsql(connectionString);
        });

        return services;
    }

    private static IServiceCollection AddPersistenceServices(this IServiceCollection services)
    {
        services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }
}
