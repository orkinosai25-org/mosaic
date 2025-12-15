using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using OrkinosaiCMS.Core.Interfaces.Services;
using OrkinosaiCMS.Web.Constants;
using System.Security.Claims;

namespace OrkinosaiCMS.Web.Services;

/// <summary>
/// Service for handling authentication operations
/// Enhanced to use ASP.NET Core cookie authentication following Oqtane/Umbraco patterns
/// </summary>
public class AuthenticationService : IAuthenticationService
{
    private readonly CustomAuthenticationStateProvider _authStateProvider;
    private readonly IUserService _userService;
    private readonly IRoleService _roleService;
    private readonly ILogger<AuthenticationService> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuthenticationService(
        AuthenticationStateProvider authStateProvider,
        IUserService userService,
        IRoleService roleService,
        ILogger<AuthenticationService> logger,
        IHttpContextAccessor httpContextAccessor)
    {
        _authStateProvider = (CustomAuthenticationStateProvider)authStateProvider;
        _userService = userService;
        _roleService = roleService;
        _logger = logger;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<bool> LoginAsync(string username, string password)
    {
        try
        {
            _logger.LogInformation("AuthenticationService.LoginAsync called for user: {Username}", username);
            
            // Verify password
            _logger.LogInformation("Verifying password for user: {Username}", username);
            var isValid = await _userService.VerifyPasswordAsync(username, password);
            
            _logger.LogInformation("Password verification result for user {Username}: {IsValid}", username, isValid);
            
            if (!isValid)
            {
                _logger.LogWarning("Password verification failed for user: {Username}", username);
                return false;
            }

            // Get user
            _logger.LogInformation("Fetching user details for: {Username}", username);
            var user = await _userService.GetByUsernameAsync(username);
            if (user == null)
            {
                _logger.LogWarning("User not found after password verification: {Username}", username);
                return false;
            }
            
            _logger.LogInformation("User found - Id: {UserId}, Username: {Username}, Email: {Email}, IsActive: {IsActive}", 
                user.Id, user.Username, user.Email, user.IsActive);
            
            if (!user.IsActive)
            {
                _logger.LogWarning("User account is inactive: {Username} (UserId: {UserId})", username, user.Id);
                return false;
            }

            // Get user's primary role (assuming first role or "Administrator")
            _logger.LogInformation("Fetching roles for user: {Username} (UserId: {UserId})", username, user.Id);
            var userRoles = await _userService.GetUserRolesAsync(user.Id);
            var primaryRole = "User";
            
            if (userRoles != null && userRoles.Any())
            {
                primaryRole = userRoles.First().Name;
                _logger.LogInformation("User {Username} has role: {Role} (Total roles: {RoleCount})", 
                    username, primaryRole, userRoles.Count());
            }
            else
            {
                _logger.LogWarning("User {Username} has no roles assigned, using default 'User' role", username);
            }

            // Create claims for cookie authentication (following Oqtane/Umbraco pattern)
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim("DisplayName", user.DisplayName),
                new Claim(ClaimTypes.Role, primaryRole)
            };

            // Add all user roles as claims
            if (userRoles != null)
            {
                foreach (var role in userRoles)
                {
                    if (role.Name != primaryRole) // Avoid duplicate primary role
                    {
                        claims.Add(new Claim(ClaimTypes.Role, role.Name));
                    }
                }
            }

            // Sign in using ASP.NET Core Identity cookie authentication scheme
            // Use IdentityConstants.ApplicationScheme instead of custom scheme for consistency
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext != null)
            {
                _logger.LogInformation("Setting up cookie authentication for user: {Username}", username);
                
                var claimsIdentity = new ClaimsIdentity(claims, IdentityConstants.ApplicationScheme);
                var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = false, // Default to session cookie for Blazor login
                    ExpiresUtc = DateTimeOffset.UtcNow.AddHours(AuthenticationConstants.DefaultCookieExpirationHours),
                    AllowRefresh = true,
                    IssuedUtc = DateTimeOffset.UtcNow
                };

                await httpContext.SignInAsync(
                    IdentityConstants.ApplicationScheme,
                    claimsPrincipal,
                    authProperties);

                _logger.LogInformation("Cookie authentication set for user: {Username}", username);
            }

            // Create user session for Blazor state provider
            _logger.LogInformation("Creating user session for Blazor state: {Username}", username);
            var userSession = new UserSession
            {
                UserId = user.Id,
                Username = user.Username,
                Email = user.Email,
                DisplayName = user.DisplayName,
                Role = primaryRole
            };

            // Update Blazor authentication state
            _logger.LogInformation("Updating Blazor authentication state for user: {Username}", username);
            await _authStateProvider.UpdateAuthenticationState(userSession);
            _logger.LogInformation("Blazor authentication state updated for user: {Username}", username);

            // Update last login
            _logger.LogInformation("Updating last login timestamp for user: {Username}", username);
            await _userService.UpdateLastLoginAsync(user.Id);
            _logger.LogInformation("Last login updated for user: {Username}", username);
            
            _logger.LogInformation("Authentication successful for user: {Username}", username);

            return true;
        }
        catch (Microsoft.Data.SqlClient.SqlException sqlEx)
        {
            _logger.LogError(sqlEx, 
                "SQL error during authentication for user: {Username} - ErrorNumber: {ErrorNumber}, State: {State}, Server: {Server}, Message: {Message}", 
                username, sqlEx.Number, sqlEx.State, sqlEx.Server, sqlEx.Message);
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
            _logger.LogError(ex, 
                "Unexpected error during authentication for user: {Username} - Type: {ExceptionType}, Message: {Message}", 
                username, ex.GetType().FullName, ex.Message);
            
            // Log inner exception if present
            if (ex.InnerException != null)
            {
                _logger.LogError("Inner exception - Type: {ExceptionType}, Message: {Message}", 
                    ex.InnerException.GetType().FullName, ex.InnerException.Message);
            }
            
            // For other exceptions, return false
            return false;
        }
    }

    public async Task LogoutAsync()
    {
        // Sign out from cookie authentication using Identity's scheme
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext != null)
        {
            _logger.LogInformation("Signing out user from cookie authentication");
            await httpContext.SignOutAsync(IdentityConstants.ApplicationScheme);
        }

        // Clear Blazor authentication state
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
