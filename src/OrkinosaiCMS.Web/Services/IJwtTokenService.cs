using OrkinosaiCMS.Core.Entities.Identity;

namespace OrkinosaiCMS.Web.Services;

/// <summary>
/// Service for generating and validating JWT tokens
/// Following Oqtane's pattern for JWT authentication
/// </summary>
public interface IJwtTokenService
{
    /// <summary>
    /// Generates a JWT access token for the specified user
    /// </summary>
    /// <param name="user">The user to generate a token for</param>
    /// <param name="roles">The roles assigned to the user</param>
    /// <returns>JWT access token string</returns>
    Task<string> GenerateAccessTokenAsync(ApplicationUser user, IList<string> roles);

    /// <summary>
    /// Generates a refresh token for long-term authentication
    /// </summary>
    /// <returns>Refresh token string</returns>
    string GenerateRefreshToken();

    /// <summary>
    /// Validates a JWT token and extracts the user ID
    /// </summary>
    /// <param name="token">The JWT token to validate</param>
    /// <returns>User ID if valid, null otherwise</returns>
    int? ValidateToken(string token);
}
