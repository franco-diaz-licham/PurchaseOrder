using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace PurchaseOrderApp.DAL;

public sealed class PurchaseOrderDbContextFactory : IDesignTimeDbContextFactory<PurchaseOrderDbContext>
{
    public PurchaseOrderDbContext CreateDbContext(string[] args)
    {
        var servicesProjectPath = FindServicesProjectPath();

        var configuration = new ConfigurationBuilder()
            .AddJsonFile(Path.Combine(servicesProjectPath, "appsettings.json"), optional: false)
            .AddJsonFile(Path.Combine(servicesProjectPath, "appsettings.Development.json"), optional: true)
            .Build();

        var connectionString =
            configuration["Database:PurchaseOrderDb"] ??
            Environment.GetEnvironmentVariable("Database__PurchaseOrderDb") ??
            Environment.GetEnvironmentVariable("Database:PurchaseOrderDb");

        if (string.IsNullOrWhiteSpace(connectionString)) {
            throw new InvalidOperationException("Database:PurchaseOrderDb is required to create the migrations DbContext.");
        }

        var options = new DbContextOptionsBuilder<PurchaseOrderDbContext>()
            .UseNpgsql(connectionString)
            .UseSnakeCaseNamingConvention()
            .Options;

        return new PurchaseOrderDbContext(options);
    }

    private static string FindServicesProjectPath()
    {
        var current = new DirectoryInfo(Directory.GetCurrentDirectory());
        while (current is not null) {
            var siblingCandidate = Path.Combine(current.FullName, "PurchaseOrderApp.Services");
            if (File.Exists(Path.Combine(siblingCandidate, "appsettings.json"))) return siblingCandidate;

            var repoCandidate = Path.Combine(current.FullName, "backend", "layered", "PurchaseOrderApp.Services");
            if (File.Exists(Path.Combine(repoCandidate, "appsettings.json"))) return repoCandidate;

            current = current.Parent;
        }

        throw new InvalidOperationException("Could not find the layered Services project path.");
    }
}
