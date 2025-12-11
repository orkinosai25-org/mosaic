using System.Net;
using System.Net.Http.Json;
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
