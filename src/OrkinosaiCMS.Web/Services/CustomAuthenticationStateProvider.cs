using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using System.Security.Claims;
using OrkinosaiCMS.Core.Interfaces.Services;

namespace OrkinosaiCMS.Web.Services;

/// <summary>
/// Custom authentication state provider for admin users
/// </summary>
public class CustomAuthenticationStateProvider : AuthenticationStateProvider
{
    private readonly ProtectedSessionStorage _sessionStorage;
    private readonly IUserService _userService;
    private ClaimsPrincipal _anonymous = new ClaimsPrincipal(new ClaimsIdentity());

    public CustomAuthenticationStateProvider(
        ProtectedSessionStorage sessionStorage,
        IUserService userService)
    {
        _sessionStorage = sessionStorage;
        _userService = userService;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        try
        {
            var userSessionResult = await _sessionStorage.GetAsync<UserSession>("UserSession");
            if (!userSessionResult.Success || userSessionResult.Value == null)
            {
                return await Task.FromResult(new AuthenticationState(_anonymous));
            }

            var userSession = userSessionResult.Value;
            
            // Verify user still exists and is active
            var user = await _userService.GetByIdAsync(userSession.UserId);
            if (user == null || !user.IsActive || user.IsDeleted)
            {
                return await Task.FromResult(new AuthenticationState(_anonymous));
            }

            var claimsPrincipal = CreateClaimsPrincipal(userSession);
            return await Task.FromResult(new AuthenticationState(claimsPrincipal));
        }
        catch
        {
            return await Task.FromResult(new AuthenticationState(_anonymous));
        }
    }

    public async Task UpdateAuthenticationState(UserSession? userSession)
    {
        ClaimsPrincipal claimsPrincipal;

        if (userSession != null)
        {
            await _sessionStorage.SetAsync("UserSession", userSession);
            claimsPrincipal = CreateClaimsPrincipal(userSession);
        }
        else
        {
            await _sessionStorage.DeleteAsync("UserSession");
            claimsPrincipal = _anonymous;
        }

        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(claimsPrincipal)));
    }

    private ClaimsPrincipal CreateClaimsPrincipal(UserSession userSession)
    {
        var claimsIdentity = new ClaimsIdentity(new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, userSession.UserId.ToString()),
            new Claim(ClaimTypes.Name, userSession.Username),
            new Claim(ClaimTypes.Email, userSession.Email),
            new Claim("DisplayName", userSession.DisplayName),
            new Claim(ClaimTypes.Role, userSession.Role)
        }, "CustomAuth");

        return new ClaimsPrincipal(claimsIdentity);
    }
}

public class UserSession
{
    public int UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Role { get; set; } = "User";
}
