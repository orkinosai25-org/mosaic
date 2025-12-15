using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OrkinosaiCMS.Core.Entities.Identity;
using OrkinosaiCMS.Shared.DTOs.Authentication;
using OrkinosaiCMS.Web.Services;
using System.Security.Claims;

namespace OrkinosaiCMS.Web.Controllers;

/// <summary>
/// Authentication API controller using ASP.NET Core Identity
/// Copied from Oqtane's proven implementation for production-ready authentication
/// Supports both cookie-based authentication (for Blazor) and JWT tokens (for APIs)
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AuthenticationController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthenticationController> _logger;

    public AuthenticationController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IJwtTokenService jwtTokenService,
        IConfiguration configuration,
        ILogger<AuthenticationController> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _jwtTokenService = jwtTokenService;
        _configuration = configuration;
        _logger = logger;
    }

    /// <summary>
    /// Login endpoint using ASP.NET Core Identity (Oqtane pattern)
    /// POST /api/authentication/login
    /// </summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        _logger.LogInformation("Login attempt for username: {Username}", request.Username);

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
            // Find user by username using Identity's UserManager (Oqtane approach)
            var identityUser = await _userManager.FindByNameAsync(request.Username);
            
            if (identityUser == null)
            {
                _logger.LogWarning("User not found: {Username}", request.Username);
                return Unauthorized(new LoginResponse
                {
                    Success = false,
                    ErrorMessage = "Invalid username or password."
                });
            }

            // Check if account is deleted (soft delete)
            if (identityUser.IsDeleted)
            {
                _logger.LogWarning("Deleted user attempted login: {Username}", request.Username);
                return Unauthorized(new LoginResponse
                {
                    Success = false,
                    ErrorMessage = "User account has been deleted."
                });
            }

            // Use SignInManager to check password (Oqtane pattern)
            // This handles password verification, lockout, and two-factor
            var result = await _signInManager.CheckPasswordSignInAsync(identityUser, request.Password, lockoutOnFailure: true);

            if (result.Succeeded)
            {
                // Check if email confirmation is required
                if (!identityUser.EmailConfirmed && _userManager.Options.SignIn.RequireConfirmedEmail)
                {
                    _logger.LogWarning("User login denied - Email not confirmed: {Username}", request.Username);
                    return Unauthorized(new LoginResponse
                    {
                        Success = false,
                        ErrorMessage = "Please confirm your email address before logging in."
                    });
                }

                // Sign in the user using SignInManager (Oqtane approach)
                await _signInManager.SignInAsync(identityUser, request.RememberMe);

                // Update last login information
                identityUser.LastLoginOn = DateTime.UtcNow;
                identityUser.LastIPAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
                await _userManager.UpdateAsync(identityUser);

                _logger.LogInformation("User login successful: {Username} from IP: {IP}", 
                    identityUser.UserName, identityUser.LastIPAddress);

                // Get user's roles
                var roles = await _userManager.GetRolesAsync(identityUser);
                var primaryRole = roles.FirstOrDefault() ?? "User";

                return Ok(new LoginResponse
                {
                    Success = true,
                    User = new UserInfo
                    {
                        UserId = identityUser.Id,
                        Username = identityUser.UserName ?? string.Empty,
                        Email = identityUser.Email ?? string.Empty,
                        DisplayName = identityUser.DisplayName,
                        Role = primaryRole,
                        IsAuthenticated = true,
                        LastLoginOn = identityUser.LastLoginOn
                    }
                });
            }
            else if (result.IsLockedOut)
            {
                _logger.LogWarning("User account locked out: {Username}", request.Username);
                return Unauthorized(new LoginResponse
                {
                    Success = false,
                    ErrorMessage = "Account locked due to multiple failed login attempts. Please try again later."
                });
            }
            else if (result.IsNotAllowed)
            {
                _logger.LogWarning("User login not allowed: {Username}", request.Username);
                return Unauthorized(new LoginResponse
                {
                    Success = false,
                    ErrorMessage = "Login not allowed. Please confirm your account."
                });
            }
            else if (result.RequiresTwoFactor)
            {
                _logger.LogInformation("Two-factor authentication required for: {Username}", request.Username);
                return Ok(new LoginResponse
                {
                    Success = false,
                    ErrorMessage = "Two-factor authentication required."
                });
            }
            else
            {
                _logger.LogWarning("Invalid password for user: {Username}", request.Username);
                return Unauthorized(new LoginResponse
                {
                    Success = false,
                    ErrorMessage = "Invalid username or password."
                });
            }
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
    /// Logout endpoint using Identity's SignInManager (Oqtane pattern)
    /// POST /api/authentication/logout
    /// </summary>
    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Logout()
    {
        var username = User.Identity?.Name ?? "Unknown";
        _logger.LogInformation("User logout: {Username}", username);

        try
        {
            // Use SignInManager.SignOutAsync (Oqtane approach)
            await _signInManager.SignOutAsync();
            
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
                    var identityUser = await _userManager.FindByIdAsync(userId.ToString());
                    
                    if (identityUser != null && !identityUser.IsDeleted)
                    {
                        var roles = await _userManager.GetRolesAsync(identityUser);
                        var primaryRole = roles.FirstOrDefault() ?? "User";

                        return Ok(new AuthenticateResponse
                        {
                            IsAuthenticated = true,
                            User = new UserInfo
                            {
                                UserId = identityUser.Id,
                                Username = identityUser.UserName ?? string.Empty,
                                Email = identityUser.Email ?? string.Empty,
                                DisplayName = identityUser.DisplayName,
                                Role = primaryRole,
                                IsAuthenticated = true,
                                LastLoginOn = identityUser.LastLoginOn
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
    /// Validates user credentials without creating a session
    /// POST /api/authentication/validate
    /// </summary>
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
            var identityUser = await _userManager.FindByNameAsync(request.Username);
            
            if (identityUser == null || identityUser.IsDeleted)
            {
                return Ok(new LoginResponse
                {
                    Success = false,
                    ErrorMessage = "Invalid username or password."
                });
            }

            // Check password without signing in
            var result = await _signInManager.CheckPasswordSignInAsync(identityUser, request.Password, lockoutOnFailure: false);

            if (result.Succeeded)
            {
                var roles = await _userManager.GetRolesAsync(identityUser);
                var primaryRole = roles.FirstOrDefault() ?? "User";

                return Ok(new LoginResponse
                {
                    Success = true,
                    User = new UserInfo
                    {
                        UserId = identityUser.Id,
                        Username = identityUser.UserName ?? string.Empty,
                        Email = identityUser.Email ?? string.Empty,
                        DisplayName = identityUser.DisplayName,
                        Role = primaryRole,
                        IsAuthenticated = false,
                        LastLoginOn = identityUser.LastLoginOn
                    }
                });
            }

            return Ok(new LoginResponse
            {
                Success = false,
                ErrorMessage = "Invalid username or password."
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

    /// <summary>
    /// Generate JWT token for API authentication (Oqtane pattern)
    /// POST /api/authentication/token
    /// This endpoint generates a JWT Bearer token for use with API clients, mobile apps, and external integrations.
    /// Cookie authentication is used for the Blazor admin portal, JWT for programmatic access.
    /// </summary>
    [HttpPost("token")]
    [ProducesResponseType(typeof(JwtTokenResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GenerateToken([FromBody] LoginRequest request)
    {
        _logger.LogInformation("JWT token generation attempt for username: {Username}", request.Username);

        if (!ModelState.IsValid)
        {
            return BadRequest(new JwtTokenResponse
            {
                Success = false,
                ErrorMessage = "Invalid request. Username and password are required."
            });
        }

        try
        {
            // Find user by username
            var identityUser = await _userManager.FindByNameAsync(request.Username);
            
            if (identityUser == null || identityUser.IsDeleted)
            {
                _logger.LogWarning("JWT token generation failed - User not found: {Username}", request.Username);
                return Unauthorized(new JwtTokenResponse
                {
                    Success = false,
                    ErrorMessage = "Invalid username or password."
                });
            }

            // Verify password without creating a session
            var result = await _signInManager.CheckPasswordSignInAsync(identityUser, request.Password, lockoutOnFailure: true);

            if (result.Succeeded)
            {
                // Get user's roles
                var roles = await _userManager.GetRolesAsync(identityUser);
                
                // Generate JWT token
                var accessToken = await _jwtTokenService.GenerateAccessTokenAsync(identityUser, roles);
                var refreshToken = _jwtTokenService.GenerateRefreshToken();
                
                // Get token expiration from configuration
                var expirationMinutes = int.Parse(_configuration["Authentication:Jwt:ExpirationMinutes"] ?? "480");

                // Update last login information
                identityUser.LastLoginOn = DateTime.UtcNow;
                identityUser.LastIPAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
                await _userManager.UpdateAsync(identityUser);

                _logger.LogInformation("JWT token generated successfully for user: {Username}", identityUser.UserName);

                var primaryRole = roles.FirstOrDefault() ?? "User";

                return Ok(new JwtTokenResponse
                {
                    Success = true,
                    AccessToken = accessToken,
                    TokenType = "Bearer",
                    ExpiresIn = expirationMinutes * 60, // Convert to seconds
                    RefreshToken = refreshToken,
                    User = new UserInfo
                    {
                        UserId = identityUser.Id,
                        Username = identityUser.UserName ?? string.Empty,
                        Email = identityUser.Email ?? string.Empty,
                        DisplayName = identityUser.DisplayName,
                        Role = primaryRole,
                        IsAuthenticated = true,
                        LastLoginOn = identityUser.LastLoginOn
                    }
                });
            }
            else if (result.IsLockedOut)
            {
                _logger.LogWarning("JWT token generation failed - Account locked out: {Username}", request.Username);
                return Unauthorized(new JwtTokenResponse
                {
                    Success = false,
                    ErrorMessage = "Account locked due to multiple failed login attempts. Please try again later."
                });
            }
            else if (result.IsNotAllowed)
            {
                _logger.LogWarning("JWT token generation failed - Login not allowed: {Username}", request.Username);
                return Unauthorized(new JwtTokenResponse
                {
                    Success = false,
                    ErrorMessage = "Login not allowed. Please confirm your account."
                });
            }
            else
            {
                _logger.LogWarning("JWT token generation failed - Invalid password: {Username}", request.Username);
                return Unauthorized(new JwtTokenResponse
                {
                    Success = false,
                    ErrorMessage = "Invalid username or password."
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during JWT token generation for username: {Username}", request.Username);
            
            return StatusCode(StatusCodes.Status500InternalServerError, new JwtTokenResponse
            {
                Success = false,
                ErrorMessage = "An unexpected error occurred. Please try again later."
            });
        }
    }
}
