using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using OrkinosaiCMS.Core.Entities.Identity;
using OrkinosaiCMS.Core.Interfaces.Services;
using OrkinosaiCMS.Web.Constants;
using System.Security.Claims;

namespace OrkinosaiCMS.Web.Services;

/// <summary>
/// Service for handling authentication operations
/// Uses ASP.NET Core Identity SignInManager following Oqtane's exact pattern
/// </summary>
public class AuthenticationService : IAuthenticationService
{
    private readonly CustomAuthenticationStateProvider _authStateProvider;
    private readonly IUserService _userService;
    private readonly IRoleService _roleService;
    private readonly ILogger<AuthenticationService> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;

    public AuthenticationService(
        AuthenticationStateProvider authStateProvider,
        IUserService userService,
        IRoleService roleService,
        ILogger<AuthenticationService> logger,
        IHttpContextAccessor httpContextAccessor,
        SignInManager<ApplicationUser> signInManager,
        UserManager<ApplicationUser> userManager)
    {
        _authStateProvider = (CustomAuthenticationStateProvider)authStateProvider;
        _userService = userService;
        _roleService = roleService;
        _logger = logger;
        _httpContextAccessor = httpContextAccessor;
        _signInManager = signInManager;
        _userManager = userManager;
    }

    public async Task<bool> LoginAsync(string username, string password)
    {
        try
        {
            _logger.LogInformation("AuthenticationService.LoginAsync called for user: {Username} (using Oqtane pattern with SignInManager)", username);
            
            // OQTANE PATTERN: Find user by username using UserManager
            _logger.LogInformation("Finding ApplicationUser by username: {Username}", username);
            var applicationUser = await _userManager.FindByNameAsync(username);
            
            if (applicationUser == null)
            {
                _logger.LogWarning("ApplicationUser not found in AspNetUsers: {Username}", username);
                return false;
            }
            
            _logger.LogInformation("ApplicationUser found - Id: {UserId}, Username: {Username}, Email: {Email}, EmailConfirmed: {EmailConfirmed}", 
                applicationUser.Id, applicationUser.UserName, applicationUser.Email, applicationUser.EmailConfirmed);
            
            // OQTANE PATTERN: Use SignInManager.CheckPasswordSignInAsync for password verification
            // This handles password verification, lockout, and two-factor WITHOUT requiring HttpContext
            // Perfect for both HTTP requests and testing scenarios
            _logger.LogInformation("Verifying password for user {Username} with SignInManager.CheckPasswordSignInAsync", username);
            var result = await _signInManager.CheckPasswordSignInAsync(
                applicationUser,
                password,
                lockoutOnFailure: true); // Enable lockout protection as per Oqtane

            _logger.LogInformation("Password check result for user {Username}: Succeeded={Succeeded}, IsLockedOut={IsLockedOut}, IsNotAllowed={IsNotAllowed}, RequiresTwoFactor={RequiresTwoFactor}", 
                username, result.Succeeded, result.IsLockedOut, result.IsNotAllowed, result.RequiresTwoFactor);

            if (result.IsLockedOut)
            {
                _logger.LogWarning("User account is locked out: {Username} (UserId: {UserId})", username, applicationUser.Id);
                return false;
            }

            if (result.IsNotAllowed)
            {
                _logger.LogWarning("User sign-in is not allowed (email not confirmed?): {Username} (UserId: {UserId})", username, applicationUser.Id);
                return false;
            }

            if (result.RequiresTwoFactor)
            {
                _logger.LogInformation("User requires two-factor authentication: {Username} (UserId: {UserId})", username, applicationUser.Id);
                // TODO: Implement 2FA flow in future
                return false;
            }

            if (!result.Succeeded)
            {
                _logger.LogWarning("Password verification failed for user: {Username}", username);
                return false;
            }

            // Password verified successfully
            _logger.LogInformation("Password verification successful for user: {Username}", username);

            // If we have an HttpContext, use SignInManager to create the authentication cookie
            // Otherwise (e.g., in tests), we'll just update the Blazor state
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext != null)
            {
                _logger.LogInformation("HttpContext available, signing in user {Username} with SignInManager", username);
                await _signInManager.SignInAsync(
                    applicationUser,
                    isPersistent: false); // Session cookie by default
                _logger.LogInformation("SignInManager successfully created authentication cookie for user: {Username}", username);
            }
            else
            {
                _logger.LogInformation("HttpContext not available (likely testing scenario), skipping cookie creation for user: {Username}", username);
            }

            // Get user roles from Identity for Blazor state
            _logger.LogInformation("Fetching roles for ApplicationUser: {Username} (UserId: {UserId})", username, applicationUser.Id);
            var roles = await _userManager.GetRolesAsync(applicationUser);
            var primaryRole = roles.FirstOrDefault() ?? "User";
            
            if (roles.Any())
            {
                _logger.LogInformation("User {Username} has roles: {Roles}", username, string.Join(", ", roles));
            }
            else
            {
                _logger.LogWarning("User {Username} has no roles assigned in Identity, using default 'User' role", username);
            }

            // Create user session for Blazor state provider
            _logger.LogInformation("Creating user session for Blazor state: {Username}", username);
            var userSession = new UserSession
            {
                UserId = applicationUser.Id,
                Username = applicationUser.UserName ?? username,
                Email = applicationUser.Email ?? string.Empty,
                DisplayName = applicationUser.DisplayName,
                Role = primaryRole
            };

            // Update Blazor authentication state
            _logger.LogInformation("Updating Blazor authentication state for user: {Username}", username);
            await _authStateProvider.UpdateAuthenticationState(userSession);
            _logger.LogInformation("Blazor authentication state updated for user: {Username}", username);

            // Update last login timestamp in ApplicationUser
            _logger.LogInformation("Updating last login timestamp for ApplicationUser: {Username}", username);
            applicationUser.LastLoginOn = DateTime.UtcNow;
            var updateResult = await _userManager.UpdateAsync(applicationUser);
            if (updateResult.Succeeded)
            {
                _logger.LogInformation("Last login timestamp updated for user: {Username}", username);
            }
            else
            {
                _logger.LogWarning("Failed to update last login timestamp for user: {Username}. Errors: {Errors}", 
                    username, string.Join(", ", updateResult.Errors.Select(e => e.Description)));
            }
            
            _logger.LogInformation("Authentication successful for user: {Username} (Oqtane pattern complete)", username);

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
        // OQTANE PATTERN: Use SignInManager.SignOutAsync instead of manual cookie sign-out
        // Only call SignOutAsync if we have an HttpContext (skip in testing scenarios)
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext != null)
        {
            _logger.LogInformation("Signing out user using SignInManager (Oqtane pattern)");
            await _signInManager.SignOutAsync();
        }
        else
        {
            _logger.LogInformation("HttpContext not available (likely testing scenario), skipping SignInManager.SignOutAsync");
        }

        // Clear Blazor authentication state
        await _authStateProvider.UpdateAuthenticationState(null);
        
        _logger.LogInformation("User signed out successfully");
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
