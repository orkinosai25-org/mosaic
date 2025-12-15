namespace OrkinosaiCMS.Shared.DTOs.Authentication;

/// <summary>
/// Response DTO for JWT token generation
/// Following Oqtane's pattern for API authentication
/// </summary>
public class JwtTokenResponse
{
    /// <summary>
    /// Indicates if token generation was successful
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// JWT access token (Bearer token for API requests)
    /// </summary>
    public string? AccessToken { get; set; }

    /// <summary>
    /// Token type (always "Bearer")
    /// </summary>
    public string TokenType { get; set; } = "Bearer";

    /// <summary>
    /// Token expiration time in seconds
    /// </summary>
    public int ExpiresIn { get; set; }

    /// <summary>
    /// Optional refresh token for obtaining new access tokens
    /// </summary>
    public string? RefreshToken { get; set; }

    /// <summary>
    /// User information included with the token
    /// </summary>
    public UserInfo? User { get; set; }

    /// <summary>
    /// Error message if token generation failed
    /// </summary>
    public string? ErrorMessage { get; set; }
}
