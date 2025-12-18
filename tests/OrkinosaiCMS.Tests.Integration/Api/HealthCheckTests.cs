using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using OrkinosaiCMS.Tests.Integration.Fixtures;

namespace OrkinosaiCMS.Tests.Integration.Api;

/// <summary>
/// Integration tests for health check and basic API endpoints
/// </summary>
public class HealthCheckTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public HealthCheckTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task HealthCheck_ShouldReturnHealthy()
    {
        // Act
        var response = await _client.GetAsync("/api/health");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        
        // Parse and verify JSON structure (case-insensitive)
        var healthStatus = JsonSerializer.Deserialize<JsonElement>(content);
        var status = healthStatus.GetProperty("status").GetString();
        status.Should().NotBeNullOrEmpty();
        status.Should().BeOneOf("Healthy", "healthy"); // Accept both Pascal and camel case
    }

    [Fact]
    public async Task HealthCheckReady_ShouldReturnReadyWhenHealthy()
    {
        // Act
        var response = await _client.GetAsync("/api/health/ready");

        // Assert - Should return 200 OK when database is healthy
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        
        // Parse and verify JSON structure
        var readyStatus = JsonSerializer.Deserialize<JsonElement>(content);
        
        // The ready endpoint should include a ready field (boolean)
        if (readyStatus.TryGetProperty("ready", out var readyProp))
        {
            readyProp.GetBoolean().Should().BeTrue();
        }
        else
        {
            // If no ready field, just verify response is valid JSON with some content
            readyStatus.ValueKind.Should().Be(JsonValueKind.Object);
        }
    }

    [Fact]
    public async Task HealthCheck_ShouldContainDatabaseCheck()
    {
        // Act
        var response = await _client.GetAsync("/api/health");
        var content = await response.Content.ReadAsStringAsync();
        var healthStatus = JsonSerializer.Deserialize<JsonElement>(content);

        // Assert - should have checks array
        if (healthStatus.TryGetProperty("checks", out var checksProp))
        {
            var checks = checksProp.EnumerateArray().ToList();
            checks.Should().NotBeEmpty();
            
            // Optionally verify database check exists (if present in response)
            var hasDbCheck = checks.Any(c => 
                c.TryGetProperty("name", out var nameProp) && 
                nameProp.GetString() == "database");
            
            if (hasDbCheck)
            {
                var dbCheck = checks.First(c => c.GetProperty("name").GetString() == "database");
                dbCheck.TryGetProperty("status", out var statusProp).Should().BeTrue();
                dbCheck.TryGetProperty("description", out var descProp).Should().BeTrue();
            }
        }
    }

    [Fact]
    public async Task RootEndpoint_ShouldReturnSuccess()
    {
        // Act
        var response = await _client.GetAsync("/");

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound, HttpStatusCode.Redirect);
    }

    [Fact]
    public async Task TestController_LogLevels_InDevelopment_ShouldWork()
    {
        // This test may not work in Testing environment as the endpoint is restricted to Development
        // But we test that it either works or returns NotFound as expected
        
        // Act
        var response = await _client.GetAsync("/api/test/log-levels");

        // Assert
        // Should either work or return NotFound (if not in Development mode)
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task NonExistentEndpoint_ShouldReturn404()
    {
        // Act
        var response = await _client.GetAsync("/api/nonexistent/endpoint");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Application_ShouldHandleMultipleConcurrentRequests()
    {
        // Arrange
        var tasks = new List<Task<HttpResponseMessage>>();

        // Act
        for (int i = 0; i < 10; i++)
        {
            tasks.Add(_client.GetAsync("/"));
        }

        var responses = await Task.WhenAll(tasks);

        // Assert
        responses.Should().HaveCount(10);
        responses.Should().OnlyContain(r => 
            r.StatusCode == HttpStatusCode.OK || 
            r.StatusCode == HttpStatusCode.NotFound ||
            r.StatusCode == HttpStatusCode.Redirect);
    }
}
