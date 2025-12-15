namespace OrkinosaiCMS.Web.Constants;

/// <summary>
/// Authentication-related constants used throughout the application
/// Centralized to ensure consistency across the authentication system
/// </summary>
public static class AuthenticationConstants
{
    /// <summary>
    /// The default authentication scheme name used for cookie authentication
    /// This should match the scheme configured in Program.cs
    /// </summary>
    public const string DefaultAuthScheme = "DefaultAuthScheme";

    /// <summary>
    /// Default session cookie expiration in hours (non-persistent)
    /// </summary>
    public const int DefaultCookieExpirationHours = 8;

    /// <summary>
    /// "Remember Me" cookie expiration in days (persistent)
    /// </summary>
    public const int RememberMeCookieExpirationDays = 30;

    /// <summary>
    /// Antiforgery cookie name
    /// </summary>
    public const string AntiforgeryCookieName = ".AspNetCore.Antiforgery";
}
