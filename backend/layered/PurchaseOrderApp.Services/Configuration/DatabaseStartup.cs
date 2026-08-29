using Microsoft.EntityFrameworkCore;
using PurchaseOrderApp.DAL;

namespace PurchaseOrderApp.Services.Configuration;

public static class DatabaseStartup
{
    public static async Task<WebApplication> MigrateDatabaseAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var database = scope.ServiceProvider.GetRequiredService<PurchaseOrderDbContext>();
        await database.Database.MigrateAsync();
        return app;
    }
}
