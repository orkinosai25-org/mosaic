using Microsoft.AspNetCore.Components.Authorization;
using OrkinosaiCMS.Core.Interfaces.Services;

namespace OrkinosaiCMS.Web.Services;

/// <summary>
/// Service for handling authentication operations
/// </summary>
public class AuthenticationService : IAuthenticationService
{
    private readonly CustomAuthenticationStateProvider _authStateProvider;
    private readonly IUserService _userService;
    private readonly IRoleService _roleService;
    private readonly ILogger<AuthenticationService> _logger;

    public AuthenticationService(
        AuthenticationStateProvider authStateProvider,
        IUserService userService,
        IRoleService roleService,
        ILogger<AuthenticationService> logger)
    {
        _authStateProvider = (CustomAuthenticationStateProvider)authStateProvider;
        _userService = userService;
        _roleService = roleService;
        _logger = logger;
    }

    public async Task<bool> LoginAsync(string username, string password)
    {
        try
        {
            _logger.LogInformation("Starting authentication for user: {Username}", username);
            
            // Verify password
            var isValid = await _userService.VerifyPasswordAsync(username, password);
            if (!isValid)
            {
                _logger.LogWarning("Password verification failed for user: {Username}", username);
                return false;
            }

            // Get user
            var user = await _userService.GetByUsernameAsync(username);
            if (user == null)
            {
                _logger.LogWarning("User not found: {Username}", username);
                return false;
            }
            
            if (!user.IsActive)
            {
                _logger.LogWarning("User account is inactive: {Username}", username);
                return false;
            }

            // Get user's primary role (assuming first role or "Administrator")
            var userRoles = await _userService.GetUserRolesAsync(user.Id);
            var primaryRole = "User";
            
            if (userRoles != null && userRoles.Any())
            {
                primaryRole = userRoles.First().Name;
                _logger.LogInformation("User {Username} has role: {Role}", username, primaryRole);
            }
            else
            {
                _logger.LogWarning("User {Username} has no roles assigned, using default 'User' role", username);
            }

            // Create user session
            var userSession = new UserSession
            {
                UserId = user.Id,
                Username = user.Username,
                Email = user.Email,
                DisplayName = user.DisplayName,
                Role = primaryRole
            };

            // Update authentication state
            await _authStateProvider.UpdateAuthenticationState(userSession);

            // Update last login
            await _userService.UpdateLastLoginAsync(user.Id);
            
            _logger.LogInformation("Authentication successful for user: {Username}", username);

            return true;
        }
        catch (Microsoft.Data.SqlClient.SqlException sqlEx)
        {
            _logger.LogError(sqlEx, "SQL error during authentication for user: {Username}", username);
            // Re-throw SQL exceptions so they can be handled by the caller with specific messaging
            throw;
        }
        catch (TimeoutException timeoutEx)
        {
            _logger.LogError(timeoutEx, "Timeout during authentication for user: {Username}", username);
            // Re-throw timeout exceptions so they can be handled by the caller
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during authentication for user: {Username}", username);
            // For other exceptions, return false
            return false;
        }
    }

    public async Task LogoutAsync()
    {
        await _authStateProvider.UpdateAuthenticationState(null);
    }

    public async Task<UserSession?> GetCurrentUserAsync()
    {
        var authState = await _authStateProvider.GetAuthenticationStateAsync();
        var user = authState.User;

        if (!user.Identity?.IsAuthenticated ?? true)
        {
            return null;
        }

        // Extract required claims
        var userIdClaim = user.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
        {
            // Missing or invalid user ID claim - authentication is invalid
            return null;
        }

        return new UserSession
        {
            UserId = userId,
            Username = user.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value ?? string.Empty,
            Email = user.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value ?? string.Empty,
            DisplayName = user.FindFirst("DisplayName")?.Value ?? string.Empty,
            Role = user.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value ?? "User"
        };
    }
}
