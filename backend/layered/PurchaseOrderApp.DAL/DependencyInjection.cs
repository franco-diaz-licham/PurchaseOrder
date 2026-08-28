using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PurchaseOrderApp.BL.Ports;
using PurchaseOrderApp.DAL.Repositories;

namespace PurchaseOrderApp.DAL;

public static class DependencyInjection
{
    public static IServiceCollection AddDataAccessLayer(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration["Database:PurchaseOrderDb"];
        if (string.IsNullOrWhiteSpace(connectionString)) {
            throw new InvalidOperationException("Database:PurchaseOrderDb is required.");
        }

        services.AddDbContext<PurchaseOrderDbContext>(options => {
            options.UseNpgsql(connectionString)
                .UseSnakeCaseNamingConvention();
        });

        services.AddScoped<IPurchaseOrderRepository, PurchaseOrderRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }
}
