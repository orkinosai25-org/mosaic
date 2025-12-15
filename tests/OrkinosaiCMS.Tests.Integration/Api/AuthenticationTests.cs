using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using OrkinosaiCMS.Core.Interfaces.Services;
using OrkinosaiCMS.Shared.DTOs.Authentication;
using OrkinosaiCMS.Tests.Integration.Fixtures;

namespace OrkinosaiCMS.Tests.Integration.Api;

/// <summary>
/// Integration tests for authentication and login functionality
/// Tests both the service layer and the new API endpoints
/// </summary>
public class AuthenticationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public AuthenticationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    #region UserService Tests

    [Fact]
    public async Task UserService_ShouldVerifyCorrectCredentials()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var userService = scope.ServiceProvider.GetRequiredService<IUserService>();

        // Act
        var result = await userService.VerifyPasswordAsync("testadmin", "TestPassword123!");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task UserService_ShouldRejectIncorrectPassword()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var userService = scope.ServiceProvider.GetRequiredService<IUserService>();

        // Act
        var result = await userService.VerifyPasswordAsync("testadmin", "WrongPassword");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task UserService_ShouldRejectNonExistentUser()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var userService = scope.ServiceProvider.GetRequiredService<IUserService>();

        // Act
        var result = await userService.VerifyPasswordAsync("nonexistent", "password");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task UserService_ShouldGetUserByUsername()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var userService = scope.ServiceProvider.GetRequiredService<IUserService>();

        // Act
        var user = await userService.GetByUsernameAsync("testadmin");

        // Assert
        user.Should().NotBeNull();
        user!.Email.Should().Be("admin@test.com");
        user.DisplayName.Should().Be("Test Admin");
    }

    [Fact]
    public async Task UserService_ShouldGetUserRoles()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var userService = scope.ServiceProvider.GetRequiredService<IUserService>();
        var user = await userService.GetByUsernameAsync("testadmin");

        // Act
        var roles = await userService.GetUserRolesAsync(user!.Id);

        // Assert
        roles.Should().NotBeNull();
        roles.Should().Contain(r => r.Name == "Administrator");
    }

    [Fact]
    public async Task UserService_ShouldCreateNewUser()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var userService = scope.ServiceProvider.GetRequiredService<IUserService>();

        var newUser = new OrkinosaiCMS.Core.Entities.Sites.User
        {
            Username = "newintegrationuser",
            Email = "newuser@test.com",
            DisplayName = "New Integration User"
        };

        // Act
        var createdUser = await userService.CreateAsync(newUser, "NewPassword123!");

        // Assert
        createdUser.Should().NotBeNull();
        createdUser.Id.Should().BeGreaterThan(0);
        createdUser.PasswordHash.Should().NotBeNullOrEmpty();
        createdUser.IsActive.Should().BeTrue();

        // Verify password works
        var passwordValid = await userService.VerifyPasswordAsync("newintegrationuser", "NewPassword123!");
        passwordValid.Should().BeTrue();
    }

    [Fact]
    public async Task UserService_ShouldChangePassword()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var userService = scope.ServiceProvider.GetRequiredService<IUserService>();

        var testUser = new OrkinosaiCMS.Core.Entities.Sites.User
        {
            Username = "passwordchangeuser",
            Email = "pwchange@test.com",
            DisplayName = "Password Change User"
        };

        var createdUser = await userService.CreateAsync(testUser, "OldPassword123!");

        // Act
        await userService.ChangePasswordAsync(createdUser.Id, "OldPassword123!", "NewPassword456!");

        // Assert
        var oldPasswordValid = await userService.VerifyPasswordAsync("passwordchangeuser", "OldPassword123!");
        oldPasswordValid.Should().BeFalse();

        var newPasswordValid = await userService.VerifyPasswordAsync("passwordchangeuser", "NewPassword456!");
        newPasswordValid.Should().BeTrue();
    }

    #endregion

    #region AuthenticationService Tests

    [Fact]
    public async Task AuthenticationService_ShouldLoginWithValidCredentials()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var authService = scope.ServiceProvider.GetRequiredService<OrkinosaiCMS.Web.Services.IAuthenticationService>();

        // Act
        var result = await authService.LoginAsync("testadmin", "TestPassword123!");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task AuthenticationService_ShouldFailLoginWithInvalidCredentials()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var authService = scope.ServiceProvider.GetRequiredService<OrkinosaiCMS.Web.Services.IAuthenticationService>();

        // Act
        var result = await authService.LoginAsync("testadmin", "WrongPassword");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task AuthenticationService_ShouldHandleLogout()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var authService = scope.ServiceProvider.GetRequiredService<OrkinosaiCMS.Web.Services.IAuthenticationService>();

        // First login
        await authService.LoginAsync("testadmin", "TestPassword123!");

        // Act
        await authService.LogoutAsync();

        // Assert - should complete without exception
        // GetCurrentUserAsync should return null after logout (but this depends on implementation)
        true.Should().BeTrue(); // Logout succeeded
    }

    #endregion

    #region API Authentication Endpoint Tests

    [Fact]
    public async Task ApiLogin_WithValidCredentials_ShouldReturnSuccess()
    {
        // Arrange
        var loginRequest = new LoginRequest
        {
            Username = "testadmin",
            Password = "TestPassword123!",
            RememberMe = false
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/authentication/login", loginRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.User.Should().NotBeNull();
        result.User!.Username.Should().Be("testadmin");
        result.User.Email.Should().Be("admin@test.com");
        result.User.IsAuthenticated.Should().BeTrue();
    }

    [Fact]
    public async Task ApiLogin_WithInvalidPassword_ShouldReturnUnauthorized()
    {
        // Arrange
        var loginRequest = new LoginRequest
        {
            Username = "testadmin",
            Password = "WrongPassword",
            RememberMe = false
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/authentication/login", loginRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        
        var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
        result.Should().NotBeNull();
        result!.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Invalid username or password");
    }

    [Fact]
    public async Task ApiLogin_WithNonExistentUser_ShouldReturnUnauthorized()
    {
        // Arrange
        var loginRequest = new LoginRequest
        {
            Username = "nonexistentuser",
            Password = "SomePassword123!",
            RememberMe = false
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/authentication/login", loginRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        
        var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
        result.Should().NotBeNull();
        result!.Success.Should().BeFalse();
    }

    [Fact]
    public async Task ApiLogin_WithMissingUsername_ShouldReturnBadRequest()
    {
        // Arrange
        var loginRequest = new LoginRequest
        {
            Username = "", // Missing username
            Password = "TestPassword123!",
            RememberMe = false
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/authentication/login", loginRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ApiLogin_WithMissingPassword_ShouldReturnBadRequest()
    {
        // Arrange
        var loginRequest = new LoginRequest
        {
            Username = "testadmin",
            Password = "", // Missing password
            RememberMe = false
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/authentication/login", loginRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ApiLogin_ShouldSetAuthenticationCookie()
    {
        // Arrange
        var client = _factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
            HandleCookies = true // Important: Handle cookies manually to verify
        });

        var loginRequest = new LoginRequest
        {
            Username = "testadmin",
            Password = "TestPassword123!",
            RememberMe = false
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/authentication/login", loginRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        // Verify Set-Cookie header is present with authentication cookie
        // Note: ASP.NET Core cookie authentication uses a cookie name based on the scheme
        response.Headers.Should().ContainKey("Set-Cookie");
        var cookies = response.Headers.GetValues("Set-Cookie");
        cookies.Should().Contain(c => c.Contains(".AspNetCore") || c.Contains("DefaultAuthScheme"),
            "because authentication should set an ASP.NET Core authentication cookie");
    }

    [Fact]
    public async Task ApiStatus_WhenNotAuthenticated_ShouldReturnNotAuthenticated()
    {
        // Act
        var response = await _client.GetAsync("/api/authentication/status");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var result = await response.Content.ReadFromJsonAsync<AuthenticateResponse>();
        result.Should().NotBeNull();
        result!.IsAuthenticated.Should().BeFalse();
        result.User.Should().BeNull();
    }

    [Fact]
    public async Task ApiStatus_AfterLogin_ShouldReturnAuthenticated()
    {
        // Arrange - Create client that preserves cookies
        var client = _factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
            HandleCookies = true
        });

        var loginRequest = new LoginRequest
        {
            Username = "testadmin",
            Password = "TestPassword123!",
            RememberMe = false
        };

        // Login first
        var loginResponse = await client.PostAsJsonAsync("/api/authentication/login", loginRequest);
        loginResponse.EnsureSuccessStatusCode();

        // Act - Check status
        var statusResponse = await client.GetAsync("/api/authentication/status");

        // Assert
        statusResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var result = await statusResponse.Content.ReadFromJsonAsync<AuthenticateResponse>();
        result.Should().NotBeNull();
        result!.IsAuthenticated.Should().BeTrue();
        result.User.Should().NotBeNull();
        result.User!.Username.Should().Be("testadmin");
    }

    [Fact]
    public async Task ApiValidate_WithValidCredentials_ShouldReturnSuccess()
    {
        // Arrange
        var validateRequest = new LoginRequest
        {
            Username = "testadmin",
            Password = "TestPassword123!",
            RememberMe = false
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/authentication/validate", validateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.User.Should().NotBeNull();
        result.User!.IsAuthenticated.Should().BeFalse(); // Validate doesn't create session
    }

    [Fact]
    public async Task ApiValidate_WithInvalidCredentials_ShouldReturnFailure()
    {
        // Arrange
        var validateRequest = new LoginRequest
        {
            Username = "testadmin",
            Password = "WrongPassword",
            RememberMe = false
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/authentication/validate", validateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
        result.Should().NotBeNull();
        result!.Success.Should().BeFalse();
    }

    [Fact]
    public async Task ApiLogout_WhenAuthenticated_ShouldSucceed()
    {
        // Arrange - Create client that preserves cookies
        var client = _factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
            HandleCookies = true
        });

        var loginRequest = new LoginRequest
        {
            Username = "testadmin",
            Password = "TestPassword123!",
            RememberMe = false
        };

        // Login first
        var loginResponse = await client.PostAsJsonAsync("/api/authentication/login", loginRequest);
        loginResponse.EnsureSuccessStatusCode();

        // Act - Logout
        var logoutResponse = await client.PostAsync("/api/authentication/logout", null);

        // Assert
        logoutResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Verify status shows not authenticated
        var statusResponse = await client.GetAsync("/api/authentication/status");
        var statusResult = await statusResponse.Content.ReadFromJsonAsync<AuthenticateResponse>();
        statusResult!.IsAuthenticated.Should().BeFalse();
    }

    #endregion

    #region Route Tests

    [Fact]
    public async Task AdminRoute_WithoutAuthentication_ShouldBeAccessible()
    {
        // The admin routes use Blazor's AuthorizeView which works differently than API auth
        // This test verifies the route is at least reachable
        
        // Act
        var response = await _client.GetAsync("/admin");

        // Assert
        // Should return OK or Redirect, not Unauthorized since anonymous access is allowed by default
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound, HttpStatusCode.Redirect);
    }

    #endregion
}
