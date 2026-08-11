using Microsoft.EntityFrameworkCore;
using PurchaseOrderApp.Infrastructure.Persistence;

namespace PurchaseOrderApp.Api.Configuration;

public static class DatabaseStartup
{
    public static async Task<WebApplication> MigrateDatabaseAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var database = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
        await database.Database.MigrateAsync();
        return app;
    }
}
