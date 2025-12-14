namespace OrkinosaiCMS.Shared.DTOs.Authentication;

/// <summary>
/// Response model for authentication status check
/// </summary>
public class AuthenticateResponse
{
    /// <summary>
    /// Whether the user is authenticated
    /// </summary>
    public bool IsAuthenticated { get; set; }

    /// <summary>
    /// User information if authenticated
    /// </summary>
    public UserInfo? User { get; set; }
}
