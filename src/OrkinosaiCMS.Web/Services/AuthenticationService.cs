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

    public AuthenticationService(
        AuthenticationStateProvider authStateProvider,
        IUserService userService,
        IRoleService roleService)
    {
        _authStateProvider = (CustomAuthenticationStateProvider)authStateProvider;
        _userService = userService;
        _roleService = roleService;
    }

    public async Task<bool> LoginAsync(string username, string password)
    {
        try
        {
            // Verify password
            var isValid = await _userService.VerifyPasswordAsync(username, password);
            if (!isValid)
            {
                return false;
            }

            // Get user
            var user = await _userService.GetByUsernameAsync(username);
            if (user == null || !user.IsActive)
            {
                return false;
            }

            // Get user's primary role (assuming first role or "Administrator")
            var userRoles = await _userService.GetUserRolesAsync(user.Id);
            var primaryRole = "User";
            
            if (userRoles != null && userRoles.Any())
            {
                primaryRole = userRoles.First().Name;
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

            return true;
        }
        catch
        {
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
