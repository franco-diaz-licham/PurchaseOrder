using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using PurchaseOrderApp.Infrastructure;

namespace PurchaseOrderApp.Tests.Shared;

/// <summary>
/// Creates the API host against the PostgreSQL database used by the test fixture.
/// </summary>
public sealed class ApiTestApplicationFactory(string connectionString) : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration(configuration => {
            configuration.AddInMemoryCollection(new Dictionary<string, string?> {
                ["Database:PurchaseOrderDb"] = connectionString,
                ["BackgroundProcessing:Enabled"] = "false",
                ["Cors:AllowedOrigins:0"] = "http://localhost"
            });
        });

        builder.ConfigureTestServices(services => {
            // The test explicitly starts a worker only after checking pre-execution state.
            services.RemoveAll<Microsoft.Extensions.Hosting.IHostedService>();
            // Hangfire caches some loggers statically across hosts. Use a non-disposable factory.
            services.AddSingleton<Microsoft.Extensions.Logging.ILoggerFactory>(
                Microsoft.Extensions.Logging.Abstractions.NullLoggerFactory.Instance);
            services.RemoveAll<DbContextOptions<DatabaseContext>>();
            services.AddDbContext<DatabaseContext>(options => {
                options.UseNpgsql(connectionString).UseSnakeCaseNamingConvention();
            });
        });
    }
}
