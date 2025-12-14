namespace OrkinosaiCMS.Shared.DTOs.Authentication;

/// <summary>
/// Response model for login operations
/// </summary>
public class LoginResponse
{
    /// <summary>
    /// Whether the login was successful
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Error message if login failed
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// User information if login was successful
    /// </summary>
    public UserInfo? User { get; set; }
}

/// <summary>
/// User information returned on successful login
/// </summary>
public class UserInfo
{
    /// <summary>
    /// User ID
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// Username
    /// </summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// Email address
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Display name
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// Primary role
    /// </summary>
    public string Role { get; set; } = string.Empty;

    /// <summary>
    /// Whether the user is authenticated
    /// </summary>
    public bool IsAuthenticated { get; set; }

    /// <summary>
    /// Last login timestamp
    /// </summary>
    public DateTime? LastLoginOn { get; set; }
}
