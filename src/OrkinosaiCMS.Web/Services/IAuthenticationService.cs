namespace OrkinosaiCMS.Web.Services;

/// <summary>
/// Service for handling authentication operations
/// </summary>
public interface IAuthenticationService
{
    Task<bool> LoginAsync(string username, string password);
    Task LogoutAsync();
    Task<UserSession?> GetCurrentUserAsync();
}
