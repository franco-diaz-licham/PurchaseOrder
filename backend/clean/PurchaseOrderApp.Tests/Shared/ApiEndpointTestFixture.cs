using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using PurchaseOrderApp.Api.Models;
using PurchaseOrderApp.Application.Ports;
using Shouldly;

namespace PurchaseOrderApp.Tests.Shared;

/// <summary>
/// Base fixture for endpoint tests that call the real API over an in-memory test server.
/// </summary>
[NonParallelizable]
public abstract class ApiEndpointTestFixture : DatabaseFixture, IDisposable
{
    private static readonly JsonSerializerOptions JsonOptions = new() {
        PropertyNameCaseInsensitive = true
    };

    private ApiTestApplicationFactory? _factory;
    private HttpClient? _client;

    /// <summary>
    /// Gets the HTTP client used to call the in-memory API host.
    /// </summary>
    protected HttpClient Client => _client ?? throw new InvalidOperationException("The API client has not been created.");

    /// <summary>
    /// Creates a fresh API host and HTTP client before each endpoint test.
    /// </summary>
    [SetUp]
    public void CreateApiClient()
    {
        _factory = new ApiTestApplicationFactory(ConnectionString);
        _client = _factory.CreateClient();
    }

    /// <summary>
    /// Disposes the API host and HTTP client after each endpoint test.
    /// </summary>
    [TearDown]
    public void DisposeApiClient()
    {
        _client?.Dispose();
        _factory?.Dispose();
        _client = null;
        _factory = null;
    }

    /// <summary>
    /// Reads the standard API response wrapper and returns its data payload.
    /// </summary>
    protected static async Task<T> ReadDataAsync<T>(HttpResponseMessage response)
    {
        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<T>>(JsonOptions);
        apiResponse.ShouldNotBeNull();
        return apiResponse.Data;
    }

    /// <summary>
    /// Runs background outbox processing once for tests that assert deferred side effects.
    /// </summary>
    protected async Task ProcessBackgroundOutboxAsync(CancellationToken cancellationToken = default)
    {
        if (_factory is null) throw new InvalidOperationException("The API factory has not been created.");

        using var scope = _factory.Services.CreateScope();
        var processor = scope.ServiceProvider.GetRequiredService<IOutboxProcessor>();
        await processor.ProcessPendingAsync(cancellationToken);
    }

    /// <summary>
    /// Disposes the API host and HTTP client owned by the fixture.
    /// </summary>
    public void Dispose()
    {
        _client?.Dispose();
        _factory?.Dispose();
    }
}
