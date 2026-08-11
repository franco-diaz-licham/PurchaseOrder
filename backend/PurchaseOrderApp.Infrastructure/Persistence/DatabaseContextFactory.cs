using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace PurchaseOrderApp.Infrastructure.Persistence;

public sealed class DatabaseContextFactory : IDesignTimeDbContextFactory<DatabaseContext>
{
    public DatabaseContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__Db");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            connectionString = "Host=localhost;Port=55433;Database=purchase_order;Username=local-dev;Password=local-dev";
        }

        var options = new DbContextOptionsBuilder<DatabaseContext>()
            .UseNpgsql(connectionString)
            .Options;

        return new DatabaseContext(options);
    }
}
