using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using OrkinosaiCMS.Core.Entities.Identity;

namespace OrkinosaiCMS.Web.Services;

/// <summary>
/// JWT Token Service implementation following Oqtane's pattern
/// Provides JWT token generation and validation for API authentication
/// </summary>
public class JwtTokenService : IJwtTokenService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<JwtTokenService> _logger;

    public JwtTokenService(IConfiguration configuration, ILogger<JwtTokenService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    /// <summary>
    /// Generates a JWT access token for the specified user
    /// Following Oqtane's pattern with claims-based authentication
    /// </summary>
    public Task<string> GenerateAccessTokenAsync(ApplicationUser user, IList<string> roles)
    {
        var secretKey = _configuration["Authentication:Jwt:SecretKey"] 
            ?? throw new InvalidOperationException("JWT SecretKey is not configured");
        
        if (secretKey.Length < 32)
        {
            throw new InvalidOperationException("JWT SecretKey must be at least 32 characters long");
        }

        var issuer = _configuration["Authentication:Jwt:Issuer"] ?? "OrkinosaiCMS";
        var audience = _configuration["Authentication:Jwt:Audience"] ?? "OrkinosaiCMS.API";
        var expirationMinutes = int.Parse(_configuration["Authentication:Jwt:ExpirationMinutes"] ?? "480");

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        // Build claims following Oqtane's pattern
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.UserName ?? string.Empty),
            new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
            new Claim("DisplayName", user.DisplayName),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
        };

        // Add role claims
        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expirationMinutes),
            signingCredentials: credentials
        );

        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
        
        _logger.LogInformation("Generated JWT token for user {UserId} ({Username}), expires in {Minutes} minutes", 
            user.Id, user.UserName, expirationMinutes);

        return Task.FromResult(tokenString);
    }

    /// <summary>
    /// Generates a cryptographically secure refresh token
    /// Used for long-term authentication without requiring password
    /// </summary>
    public string GenerateRefreshToken()
    {
        var randomNumber = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }

    /// <summary>
    /// Validates a JWT token and extracts the user ID
    /// </summary>
    public int? ValidateToken(string token)
    {
        if (string.IsNullOrEmpty(token))
            return null;

        var secretKey = _configuration["Authentication:Jwt:SecretKey"];
        if (string.IsNullOrEmpty(secretKey))
            return null;

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var tokenHandler = new JwtSecurityTokenHandler();

        try
        {
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = key,
                ValidateIssuer = true,
                ValidIssuer = _configuration["Authentication:Jwt:Issuer"] ?? "OrkinosaiCMS",
                ValidateAudience = true,
                ValidAudience = _configuration["Authentication:Jwt:Audience"] ?? "OrkinosaiCMS.API",
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };

            var principal = tokenHandler.ValidateToken(token, validationParameters, out var validatedToken);
            
            if (validatedToken is JwtSecurityToken jwtToken &&
                jwtToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim != null && int.TryParse(userIdClaim.Value, out var userId))
                {
                    return userId;
                }
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "JWT token validation failed");
            return null;
        }
    }
}
