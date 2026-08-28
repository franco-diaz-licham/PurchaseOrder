using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace PurchaseOrderApp.Infrastructure;

/// <summary>
/// Creates <see cref="DatabaseContext"/> instances for EF Core design-time tooling.
/// </summary>
/// <remarks>
/// The API creates <see cref="DatabaseContext"/> at runtime through dependency injection.
/// EF Core commands run before the API host is running, so they need a direct way to build
/// <see cref="DbContextOptions{TContext}"/>.
/// </remarks>
public sealed class DatabaseContextFactory : IDesignTimeDbContextFactory<DatabaseContext>
{
    /// <summary>
    /// Builds the database context used by EF Core migrations and model inspection.
    /// </summary>
    /// <param name="args">Arguments passed by EF Core tooling. They are not currently required.</param>
    /// <returns>A configured <see cref="DatabaseContext"/> instance.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when <c>Database:PurchaseOrderDb</c> cannot be found in appsettings or environment variables.
    /// </exception>
    public DatabaseContext CreateDbContext(string[] args)
    {
        var apiProjectPath = Path.GetFullPath(Path.Combine(
            Directory.GetCurrentDirectory(),
            "..",
            "PurchaseOrderApp.Api"));

        var configuration = new ConfigurationBuilder()
            .AddJsonFile(Path.Combine(apiProjectPath, "appsettings.json"), optional: false)
            .AddJsonFile(Path.Combine(apiProjectPath, "appsettings.Development.json"), optional: true)
            .Build();

        var connectionString =
            configuration["Database:PurchaseOrderDb"] ??
            Environment.GetEnvironmentVariable("Database__PurchaseOrderDb") ??
            Environment.GetEnvironmentVariable("Database:PurchaseOrderDb");

        if (string.IsNullOrWhiteSpace(connectionString)) {
            throw new InvalidOperationException("Database:PurchaseOrderDb is required to create the migrations DbContext.");
        }

        var options = new DbContextOptionsBuilder<DatabaseContext>()
            .UseNpgsql(connectionString)
            .UseSnakeCaseNamingConvention()
            .Options;

        return new DatabaseContext(options);
    }
}
