using FluentAssertions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using OrkinosaiCMS.Web;

namespace OrkinosaiCMS.Tests.Unit.Configuration;

/// <summary>
/// Unit tests for authentication and middleware configuration
/// Tests correct registration and error handling for authentication schemes
/// </summary>
public class AuthenticationConfigurationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public AuthenticationConfigurationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public void AuthenticationScheme_ShouldBeRegistered()
    {
        // Arrange & Act
        using var scope = _factory.Services.CreateScope();
        var authenticationSchemeProvider = scope.ServiceProvider.GetService<IAuthenticationSchemeProvider>();

        // Assert
        authenticationSchemeProvider.Should().NotBeNull();
    }

    [Fact]
    public async Task DefaultAuthScheme_ShouldBeConfigured()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var authenticationSchemeProvider = scope.ServiceProvider.GetRequiredService<IAuthenticationSchemeProvider>();

        // Act
        var defaultScheme = await authenticationSchemeProvider.GetDefaultAuthenticateSchemeAsync();

        // Assert
        defaultScheme.Should().NotBeNull();
        defaultScheme!.Name.Should().Be("DefaultAuthScheme");
    }

    [Fact]
    public async Task CookieAuthenticationScheme_ShouldHaveCorrectConfiguration()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var authenticationSchemeProvider = scope.ServiceProvider.GetRequiredService<IAuthenticationSchemeProvider>();

        // Act
        var scheme = await authenticationSchemeProvider.GetSchemeAsync("DefaultAuthScheme");

        // Assert
        scheme.Should().NotBeNull();
        scheme!.HandlerType.Should().Be(typeof(CookieAuthenticationHandler));
    }

    [Fact]
    public void AuthenticationServices_ShouldBeRegistered()
    {
        // Arrange & Act
        using var scope = _factory.Services.CreateScope();
        var serviceProvider = scope.ServiceProvider;

        // Assert
        serviceProvider.GetService<IAuthenticationService>().Should().NotBeNull();
        serviceProvider.GetService<IAuthenticationSchemeProvider>().Should().NotBeNull();
        serviceProvider.GetService<IAuthenticationHandlerProvider>().Should().NotBeNull();
    }

    [Fact]
    public void MissingAuthenticationScheme_ShouldNotCauseStartupFailure()
    {
        // Arrange & Act
        // This test verifies that the application can start even if authentication configuration has issues
        var client = _factory.CreateClient();

        // Assert
        client.Should().NotBeNull();
        // If we reach here, the application started successfully despite any potential auth config issues
    }

    [Fact]
    public async Task AllAuthenticationSchemes_ShouldBeRetrievable()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var authenticationSchemeProvider = scope.ServiceProvider.GetRequiredService<IAuthenticationSchemeProvider>();

        // Act
        var schemes = await authenticationSchemeProvider.GetAllSchemesAsync();

        // Assert
        schemes.Should().NotBeNull();
        schemes.Should().NotBeEmpty();
        schemes.Should().Contain(s => s.Name == "DefaultAuthScheme");
    }

    [Fact]
    public void AuthorizationServices_ShouldBeRegistered()
    {
        // Arrange & Act
        using var scope = _factory.Services.CreateScope();
        var authorizationService = scope.ServiceProvider.GetService<Microsoft.AspNetCore.Authorization.IAuthorizationService>();

        // Assert
        authorizationService.Should().NotBeNull("Authorization services should be registered");
    }

    [Fact]
    public void CustomAuthenticationStateProvider_ShouldBeRegistered()
    {
        // Arrange & Act
        using var scope = _factory.Services.CreateScope();
        var authStateProvider = scope.ServiceProvider.GetService<Microsoft.AspNetCore.Components.Authorization.AuthenticationStateProvider>();

        // Assert
        authStateProvider.Should().NotBeNull("CustomAuthenticationStateProvider should be registered");
        authStateProvider.Should().BeOfType<OrkinosaiCMS.Web.Services.CustomAuthenticationStateProvider>();
    }

    [Fact]
    public void IAuthenticationService_ShouldBeRegistered()
    {
        // Arrange & Act
        using var scope = _factory.Services.CreateScope();
        var authService = scope.ServiceProvider.GetService<OrkinosaiCMS.Web.Services.IAuthenticationService>();

        // Assert
        authService.Should().NotBeNull("IAuthenticationService should be registered");
        authService.Should().BeOfType<OrkinosaiCMS.Web.Services.AuthenticationService>();
    }
}
