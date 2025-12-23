using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using OrkinosaiCMS.Tests.Integration.Fixtures;

namespace OrkinosaiCMS.Tests.Integration.Api;

/// <summary>
/// Integration tests for diagnostics endpoints
/// Tests require administrator authentication
/// </summary>
public class DiagnosticsTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;
    
    // Test credentials - matches the seeded test user in CustomWebApplicationFactory
    private const string TestAdminUsername = "testadmin";
    private const string TestAdminPassword = "TestPassword123!";

    public DiagnosticsTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task DiagnosticsStatus_WithoutAuth_ShouldReturnUnauthorized()
    {
        // Act
        var response = await _client.GetAsync("/api/diagnostics/status");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task DiagnosticsConfig_WithoutAuth_ShouldReturnUnauthorized()
    {
        // Act
        var response = await _client.GetAsync("/api/diagnostics/config");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task DiagnosticsEnvironment_WithoutAuth_ShouldReturnUnauthorized()
    {
        // Act
        var response = await _client.GetAsync("/api/diagnostics/environment");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task DiagnosticsLogs_WithoutAuth_ShouldReturnUnauthorized()
    {
        // Act
        var response = await _client.GetAsync("/api/diagnostics/logs");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task DiagnosticsErrors_WithoutAuth_ShouldReturnUnauthorized()
    {
        // Act
        var response = await _client.GetAsync("/api/diagnostics/errors");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task DiagnosticsReport_WithoutAuth_ShouldReturnUnauthorized()
    {
        // Act
        var response = await _client.GetAsync("/api/diagnostics/report");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task DiagnosticsStatus_WithAdminAuth_ShouldReturnSuccessWithData()
    {
        // Arrange
        var authenticatedClient = await GetAuthenticatedAdminClient();

        // Act
        var response = await authenticatedClient.GetAsync("/api/diagnostics/status");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        
        // Parse and verify JSON structure
        var result = JsonSerializer.Deserialize<JsonElement>(content);
        result.TryGetProperty("success", out var successProp).Should().BeTrue();
        successProp.GetBoolean().Should().BeTrue();
        
        result.TryGetProperty("data", out var dataProp).Should().BeTrue();
        dataProp.ValueKind.Should().Be(JsonValueKind.Object);
        
        // Verify that application status contains expected fields
        dataProp.TryGetProperty("applicationName", out _).Should().BeTrue();
        dataProp.TryGetProperty("environment", out _).Should().BeTrue();
        dataProp.TryGetProperty("healthStatus", out _).Should().BeTrue();
    }

    [Fact]
    public async Task DiagnosticsConfig_WithAdminAuth_ShouldReturnConfigWithRedaction()
    {
        // Arrange
        var authenticatedClient = await GetAuthenticatedAdminClient();

        // Act
        var response = await authenticatedClient.GetAsync("/api/diagnostics/config");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        
        // Parse and verify JSON structure
        var result = JsonSerializer.Deserialize<JsonElement>(content);
        result.TryGetProperty("success", out var successProp).Should().BeTrue();
        successProp.GetBoolean().Should().BeTrue();
        
        result.TryGetProperty("note", out var noteProp).Should().BeTrue();
        noteProp.GetString()!.ToLower().Should().Contain("redacted");
        
        result.TryGetProperty("data", out var dataProp).Should().BeTrue();
        dataProp.ValueKind.Should().Be(JsonValueKind.Object);
    }

    [Fact]
    public async Task DiagnosticsEnvironment_WithAdminAuth_ShouldReturnEnvironmentVars()
    {
        // Arrange
        var authenticatedClient = await GetAuthenticatedAdminClient();

        // Act
        var response = await authenticatedClient.GetAsync("/api/diagnostics/environment");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        
        // Parse and verify JSON structure
        var result = JsonSerializer.Deserialize<JsonElement>(content);
        result.TryGetProperty("success", out var successProp).Should().BeTrue();
        successProp.GetBoolean().Should().BeTrue();
        
        result.TryGetProperty("data", out var dataProp).Should().BeTrue();
        dataProp.ValueKind.Should().Be(JsonValueKind.Object);
        
        result.TryGetProperty("count", out var countProp).Should().BeTrue();
        countProp.GetInt32().Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task DiagnosticsLogs_WithAdminAuth_ShouldReturnRecentLogs()
    {
        // Arrange
        var authenticatedClient = await GetAuthenticatedAdminClient();

        // Act
        var response = await authenticatedClient.GetAsync("/api/diagnostics/logs?maxLines=50");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        
        // Parse and verify JSON structure
        var result = JsonSerializer.Deserialize<JsonElement>(content);
        result.TryGetProperty("success", out var successProp).Should().BeTrue();
        successProp.GetBoolean().Should().BeTrue();
        
        result.TryGetProperty("data", out var dataProp).Should().BeTrue();
        dataProp.ValueKind.Should().Be(JsonValueKind.Array);
    }

    [Fact]
    public async Task DiagnosticsLogs_WithInvalidMaxLines_ShouldReturnBadRequest()
    {
        // Arrange
        var authenticatedClient = await GetAuthenticatedAdminClient();

        // Act - maxLines > 1000
        var response = await authenticatedClient.GetAsync("/api/diagnostics/logs?maxLines=2000");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task DiagnosticsErrors_WithAdminAuth_ShouldReturnFilteredErrors()
    {
        // Arrange
        var authenticatedClient = await GetAuthenticatedAdminClient();

        // Act
        var response = await authenticatedClient.GetAsync("/api/diagnostics/errors?maxLines=20");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        
        // Parse and verify JSON structure
        var result = JsonSerializer.Deserialize<JsonElement>(content);
        result.TryGetProperty("success", out var successProp).Should().BeTrue();
        successProp.GetBoolean().Should().BeTrue();
        
        result.TryGetProperty("data", out var dataProp).Should().BeTrue();
        dataProp.ValueKind.Should().Be(JsonValueKind.Array);
    }

    [Fact]
    public async Task DiagnosticsReport_WithAdminAuth_ShouldReturnComprehensiveReport()
    {
        // Arrange
        var authenticatedClient = await GetAuthenticatedAdminClient();

        // Act
        var response = await authenticatedClient.GetAsync("/api/diagnostics/report");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        
        // Parse and verify JSON structure
        var result = JsonSerializer.Deserialize<JsonElement>(content);
        result.TryGetProperty("success", out var successProp).Should().BeTrue();
        successProp.GetBoolean().Should().BeTrue();
        
        result.TryGetProperty("data", out var dataProp).Should().BeTrue();
        dataProp.ValueKind.Should().Be(JsonValueKind.Object);
        
        // Verify comprehensive report contains all sections
        dataProp.TryGetProperty("status", out _).Should().BeTrue();
        dataProp.TryGetProperty("configuration", out _).Should().BeTrue();
        dataProp.TryGetProperty("environment", out _).Should().BeTrue();
        dataProp.TryGetProperty("recentLogs", out _).Should().BeTrue();
        dataProp.TryGetProperty("recentErrors", out _).Should().BeTrue();
    }

    /// <summary>
    /// Helper method to create an authenticated admin client
    /// </summary>
    private async Task<HttpClient> GetAuthenticatedAdminClient()
    {
        // The test admin user is seeded in CustomWebApplicationFactory
        
        // Create authenticated client
        var authenticatedClient = _factory.CreateClient();
        
        // Perform login to get authentication cookie
        var loginRequest = new
        {
            username = TestAdminUsername,
            password = TestAdminPassword
        };
        
        var loginResponse = await authenticatedClient.PostAsJsonAsync("/api/authentication/login", loginRequest);
        loginResponse.EnsureSuccessStatusCode();
        
        return authenticatedClient;
    }
}
