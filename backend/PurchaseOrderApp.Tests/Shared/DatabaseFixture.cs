using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using PurchaseOrderApp.Infrastructure;
using Respawn;
using Respawn.Graph;
using Testcontainers.PostgreSql;

namespace PurchaseOrderApp.Tests.Shared;

/// <summary>
/// Provides a reusable PostgreSQL database fixture for integration-style tests.
/// </summary>
public abstract class DatabaseFixture
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder()
        .WithImage("postgres:17-alpine")
        .WithDatabase("purchase_order_tests")
        .WithUsername("purchase_order")
        .WithPassword("purchase_order")
        .Build();

    private Respawner _respawner = default!;
    private ServiceProvider _serviceProvider = default!;
    private IServiceScope _scope = default!;

    protected DatabaseContext Db { get; private set; } = default!;

    protected string ConnectionString => _postgres.GetConnectionString();

    [OneTimeSetUp]
    public async Task StartDatabaseAsync()
    {
        await _postgres.StartAsync();
        _serviceProvider = CreateServiceProvider();

        using var setupScope = _serviceProvider.CreateScope();
        var setupContext = setupScope.ServiceProvider.GetRequiredService<DatabaseContext>();
        await setupContext.Database.MigrateAsync();

        await using var connection = new NpgsqlConnection(ConnectionString);
        await connection.OpenAsync();
        _respawner = await Respawner.CreateAsync(connection, new RespawnerOptions {
            DbAdapter = DbAdapter.Postgres,
            SchemasToInclude = ["public"],
            TablesToIgnore = [new Table("public", "__EFMigrationsHistory")]
        });
    }

    [SetUp]
    public async Task ResetDatabaseAsync()
    {
        await using var connection = new NpgsqlConnection(ConnectionString);
        await connection.OpenAsync();
        await _respawner.ResetAsync(connection);
        _scope = _serviceProvider.CreateScope();
        Db = _scope.ServiceProvider.GetRequiredService<DatabaseContext>();
    }

    [TearDown]
    public async Task DisposeContextAsync()
    {
        await Db.DisposeAsync();
        _scope.Dispose();
    }

    [OneTimeTearDown]
    public async Task StopDatabaseAsync()
    {
        await _serviceProvider.DisposeAsync();
        await _postgres.DisposeAsync();
    }

    private ServiceProvider CreateServiceProvider()
    {
        var services = new ServiceCollection();

        services.AddDbContext<DatabaseContext>(options => {
            options.UseNpgsql(ConnectionString).UseSnakeCaseNamingConvention();
        });

        return services.BuildServiceProvider();
    }
}
