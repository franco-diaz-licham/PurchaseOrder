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

    /// <summary>
    /// Scoped database context reset before every test.
    /// </summary>
    protected DatabaseContext Db { get; private set; } = default!;

    /// <summary>
    /// PostgreSQL connection string for the running test container.
    /// </summary>
    protected string ConnectionString => _postgres.GetConnectionString();

    /// <summary>
    /// Starts PostgreSQL, applies migrations, and prepares Respawn for database resets.
    /// </summary>
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

    /// <summary>
    /// Clears database data and creates a fresh scoped context before each test.
    /// </summary>
    [SetUp]
    public async Task ResetDatabaseAsync()
    {
        await using var connection = new NpgsqlConnection(ConnectionString);
        await connection.OpenAsync();
        await _respawner.ResetAsync(connection);
        _scope = _serviceProvider.CreateScope();
        Db = _scope.ServiceProvider.GetRequiredService<DatabaseContext>();
    }

    /// <summary>
    /// Disposes the per-test context and service scope.
    /// </summary>
    [TearDown]
    public async Task DisposeContextAsync()
    {
        await Db.DisposeAsync();
        _scope.Dispose();
    }

    /// <summary>
    /// Stops and disposes the PostgreSQL test container.
    /// </summary>
    [OneTimeTearDown]
    public async Task StopDatabaseAsync()
    {
        await _serviceProvider.DisposeAsync();
        await _postgres.DisposeAsync();
    }

    /// <summary>
    /// Creates an independent database context for tests that need multiple transactions.
    /// </summary>
    /// <returns>A new <see cref="DatabaseContext"/> using the shared test database connection.</returns>
    protected DatabaseContext CreateDatabaseContext()
    {
        var options = new DbContextOptionsBuilder<DatabaseContext>()
            .UseNpgsql(ConnectionString)
            .UseSnakeCaseNamingConvention()
            .Options;

        return new DatabaseContext(options);
    }

    /// <summary>
    /// Builds the service provider used by the default per-test database context.
    /// </summary>
    /// <returns>A service provider configured for the test database.</returns>
    private ServiceProvider CreateServiceProvider()
    {
        var services = new ServiceCollection();
        services.AddDbContext<DatabaseContext>(options => {
            options.UseNpgsql(ConnectionString).UseSnakeCaseNamingConvention();
        });

        return services.BuildServiceProvider();
    }
}
