using System.Net.Http.Json;
using System.Text.Json;
using PurchaseOrderApp.Api.Models;
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

    protected HttpClient Client => _client ?? throw new InvalidOperationException("The API client has not been created.");

    [SetUp]
    public void CreateApiClient()
    {
        _factory = new ApiTestApplicationFactory(ConnectionString);
        _client = _factory.CreateClient();
    }

    [TearDown]
    public void DisposeApiClient()
    {
        _client?.Dispose();
        _factory?.Dispose();
        _client = null;
        _factory = null;
    }

    protected static async Task<T> ReadDataAsync<T>(HttpResponseMessage response)
    {
        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<T>>(JsonOptions);
        apiResponse.ShouldNotBeNull();
        return apiResponse.Data;
    }

    public void Dispose()
    {
        _client?.Dispose();
        _factory?.Dispose();
    }
}
