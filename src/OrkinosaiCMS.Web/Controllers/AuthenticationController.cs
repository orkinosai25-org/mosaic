using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrkinosaiCMS.Core.Interfaces.Services;
using OrkinosaiCMS.Shared.DTOs.Authentication;
using System.Security.Claims;

namespace OrkinosaiCMS.Web.Controllers;

/// <summary>
/// Authentication API controller for user login/logout operations
/// Follows patterns from Oqtane and Umbraco for production-ready authentication
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AuthenticationController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IRoleService _roleService;
    private readonly ILogger<AuthenticationController> _logger;

    public AuthenticationController(
        IUserService userService,
        IRoleService roleService,
        ILogger<AuthenticationController> logger)
    {
        _userService = userService;
        _roleService = roleService;
        _logger = logger;
    }

    /// <summary>
    /// Login endpoint that authenticates a user and sets authentication cookie
    /// POST /api/authentication/login
    /// </summary>
    /// <param name="request">Login credentials</param>
    /// <returns>Login response with user information or error</returns>
    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        _logger.LogInformation("Login attempt for username: {Username}", request.Username);

        if (!ModelState.IsValid)
        {
            _logger.LogWarning("Login request validation failed for username: {Username}", request.Username);
            return BadRequest(new LoginResponse
            {
                Success = false,
                ErrorMessage = "Invalid request. Username and password are required."
            });
        }

        try
        {
            // Verify password using existing UserService
            _logger.LogDebug("Verifying password for user: {Username}", request.Username);
            var isValid = await _userService.VerifyPasswordAsync(request.Username, request.Password);

            if (!isValid)
            {
                _logger.LogWarning("Password verification failed for user: {Username}", request.Username);
                return Unauthorized(new LoginResponse
                {
                    Success = false,
                    ErrorMessage = "Invalid username or password."
                });
            }

            // Get user details
            _logger.LogDebug("Fetching user details for: {Username}", request.Username);
            var user = await _userService.GetByUsernameAsync(request.Username);
            
            if (user == null)
            {
                _logger.LogError("User not found after password verification: {Username}", request.Username);
                return Unauthorized(new LoginResponse
                {
                    Success = false,
                    ErrorMessage = "User not found."
                });
            }

            if (!user.IsActive)
            {
                _logger.LogWarning("Inactive user attempted login: {Username}", request.Username);
                return Unauthorized(new LoginResponse
                {
                    Success = false,
                    ErrorMessage = "User account is inactive."
                });
            }

            if (user.IsDeleted)
            {
                _logger.LogWarning("Deleted user attempted login: {Username}", request.Username);
                return Unauthorized(new LoginResponse
                {
                    Success = false,
                    ErrorMessage = "User account has been deleted."
                });
            }

            // Get user's roles
            _logger.LogDebug("Fetching roles for user: {Username}", request.Username);
            var userRoles = await _userService.GetUserRolesAsync(user.Id);
            var primaryRole = userRoles?.FirstOrDefault()?.Name ?? "User";

            // Create claims for the user (similar to Oqtane's approach)
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim("DisplayName", user.DisplayName),
                new Claim(ClaimTypes.Role, primaryRole)
            };

            // Add all roles as claims
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

            // Create claims identity with authentication type
            var claimsIdentity = new ClaimsIdentity(claims, "DefaultAuthScheme");
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

            // Sign in the user with cookie authentication (following Oqtane pattern)
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = request.RememberMe,
                ExpiresUtc = request.RememberMe 
                    ? DateTimeOffset.UtcNow.AddDays(30) 
                    : DateTimeOffset.UtcNow.AddHours(8),
                AllowRefresh = true,
                IssuedUtc = DateTimeOffset.UtcNow
            };

            await HttpContext.SignInAsync(
                "DefaultAuthScheme",
                claimsPrincipal,
                authProperties);

            // Update last login timestamp
            await _userService.UpdateLastLoginAsync(user.Id);

            _logger.LogInformation("User login successful: {Username}, Role: {Role}", user.Username, primaryRole);

            // Return success response with user info
            return Ok(new LoginResponse
            {
                Success = true,
                User = new UserInfo
                {
                    UserId = user.Id,
                    Username = user.Username,
                    Email = user.Email,
                    DisplayName = user.DisplayName,
                    Role = primaryRole,
                    IsAuthenticated = true,
                    LastLoginOn = user.LastLoginOn
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during login for username: {Username}", request.Username);
            
            return StatusCode(StatusCodes.Status500InternalServerError, new LoginResponse
            {
                Success = false,
                ErrorMessage = "An unexpected error occurred during login. Please try again later."
            });
        }
    }

    /// <summary>
    /// Logout endpoint that signs out the user and clears authentication cookie
    /// POST /api/authentication/logout
    /// </summary>
    /// <returns>Success status</returns>
    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Logout()
    {
        var username = User.Identity?.Name ?? "Unknown";
        _logger.LogInformation("User logout: {Username}", username);

        try
        {
            await HttpContext.SignOutAsync("DefaultAuthScheme");
            
            _logger.LogInformation("User logout successful: {Username}", username);
            
            return Ok(new { success = true, message = "Logout successful" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during logout for user: {Username}", username);
            return StatusCode(StatusCodes.Status500InternalServerError, new { success = false, message = "Logout failed" });
        }
    }

    /// <summary>
    /// Check current authentication status
    /// GET /api/authentication/status
    /// </summary>
    /// <returns>Authentication status and user info if authenticated</returns>
    [HttpGet("status")]
    [ProducesResponseType(typeof(AuthenticateResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAuthenticationStatus()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            
            if (int.TryParse(userIdClaim, out int userId))
            {
                try
                {
                    // Verify user still exists and is active
                    var user = await _userService.GetByIdAsync(userId);
                    
                    if (user != null && user.IsActive && !user.IsDeleted)
                    {
                        var userRoles = await _userService.GetUserRolesAsync(user.Id);
                        var primaryRole = userRoles?.FirstOrDefault()?.Name ?? "User";

                        return Ok(new AuthenticateResponse
                        {
                            IsAuthenticated = true,
                            User = new UserInfo
                            {
                                UserId = user.Id,
                                Username = user.Username,
                                Email = user.Email,
                                DisplayName = user.DisplayName,
                                Role = primaryRole,
                                IsAuthenticated = true,
                                LastLoginOn = user.LastLoginOn
                            }
                        });
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error retrieving user info for authenticated user ID: {UserId}", userId);
                }
            }
        }

        return Ok(new AuthenticateResponse
        {
            IsAuthenticated = false,
            User = null
        });
    }

    /// <summary>
    /// Validates user credentials without creating a session (for API clients)
    /// POST /api/authentication/validate
    /// </summary>
    /// <param name="request">Login credentials</param>
    /// <returns>Validation result</returns>
    [HttpPost("validate")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ValidateCredentials([FromBody] LoginRequest request)
    {
        _logger.LogInformation("Credential validation attempt for username: {Username}", request.Username);

        if (!ModelState.IsValid)
        {
            return BadRequest(new LoginResponse
            {
                Success = false,
                ErrorMessage = "Invalid request. Username and password are required."
            });
        }

        try
        {
            var isValid = await _userService.VerifyPasswordAsync(request.Username, request.Password);

            if (!isValid)
            {
                return Ok(new LoginResponse
                {
                    Success = false,
                    ErrorMessage = "Invalid username or password."
                });
            }

            var user = await _userService.GetByUsernameAsync(request.Username);
            
            if (user == null || !user.IsActive || user.IsDeleted)
            {
                return Ok(new LoginResponse
                {
                    Success = false,
                    ErrorMessage = "User account is not available."
                });
            }

            var userRoles = await _userService.GetUserRolesAsync(user.Id);
            var primaryRole = userRoles?.FirstOrDefault()?.Name ?? "User";

            return Ok(new LoginResponse
            {
                Success = true,
                User = new UserInfo
                {
                    UserId = user.Id,
                    Username = user.Username,
                    Email = user.Email,
                    DisplayName = user.DisplayName,
                    Role = primaryRole,
                    IsAuthenticated = false, // Not creating a session
                    LastLoginOn = user.LastLoginOn
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during credential validation for username: {Username}", request.Username);
            
            return StatusCode(StatusCodes.Status500InternalServerError, new LoginResponse
            {
                Success = false,
                ErrorMessage = "An unexpected error occurred. Please try again later."
            });
        }
    }
}
