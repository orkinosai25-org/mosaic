using System.ComponentModel.DataAnnotations;

namespace OrkinosaiCMS.Shared.DTOs.Authentication;

/// <summary>
/// Request model for user login
/// </summary>
public class LoginRequest
{
    /// <summary>
    /// Username for authentication
    /// </summary>
    [Required(ErrorMessage = "Username is required")]
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// Password for authentication
    /// </summary>
    [Required(ErrorMessage = "Password is required")]
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// Whether to persist the authentication cookie (remember me)
    /// </summary>
    public bool RememberMe { get; set; } = false;
}
