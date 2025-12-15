# JWT Authentication Implementation - Oqtane Pattern

## Overview

Following Oqtane's proven pattern, Mosaic CMS now supports **dual authentication**:
- **Cookie-based authentication** for the Blazor admin portal (/admin/login)
- **JWT Bearer tokens** for API clients, mobile apps, SPAs, and external integrations

This provides the flexibility needed for modern application architectures while maintaining secure, production-ready authentication.

## Architecture

### Dual Authentication Scheme

```
┌─────────────────────────────────────────────────────────┐
│                    Mosaic CMS                           │
├─────────────────────────────────────────────────────────┤
│  Cookie Auth (Identity)      │  JWT Bearer Auth         │
│  - Blazor admin portal       │  - API clients           │
│  - /admin/login              │  - Mobile apps           │
│  - Server-side rendering     │  - SPAs                  │
│  - Session-based             │  - External integrations │
│  - Automatic renewal         │  - Microservices         │
└─────────────────────────────────────────────────────────┘
```

### Authentication Flow

**1. Cookie Authentication (Blazor Portal)**
```
User → /admin/login (Blazor UI)
     → POST /api/authentication/login
     → Cookie set (.OrkinosaiCMS.Auth)
     → Redirect to /admin
     → Blazor components access HttpContext.User
```

**2. JWT Authentication (API Clients)**
```
Client → POST /api/authentication/token
       → Receive JWT access token
       → Store token securely
       → Include in API requests: Authorization: Bearer {token}
       → API controllers validate JWT
```

## Configuration

### appsettings.json

```json
{
  "Authentication": {
    "Jwt": {
      "SecretKey": "your-super-secret-jwt-key-min-32-chars-change-in-production",
      "Issuer": "OrkinosaiCMS",
      "Audience": "OrkinosaiCMS.API",
      "ExpirationMinutes": 480,
      "RefreshTokenExpirationDays": 30
    }
  }
}
```

**IMPORTANT**: 
- `SecretKey` must be at least 32 characters
- In production, set via environment variable: `Authentication__Jwt__SecretKey`
- Use Azure Key Vault or secure secrets management
- Never commit secrets to source control

### Environment Variables (Production)

```bash
# Azure App Service Configuration
Authentication__Jwt__SecretKey="your-production-secret-key-min-32-chars"
Authentication__Jwt__Issuer="OrkinosaiCMS.Production"
Authentication__Jwt__Audience="OrkinosaiCMS.API"
Authentication__Jwt__ExpirationMinutes="480"
```

## API Endpoints

### 1. Generate JWT Token

**Endpoint**: `POST /api/authentication/token`

**Request**:
```json
{
  "username": "admin",
  "password": "Admin@123",
  "rememberMe": false
}
```

**Success Response** (200 OK):
```json
{
  "success": true,
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "tokenType": "Bearer",
  "expiresIn": 28800,
  "refreshToken": "abc123...",
  "user": {
    "userId": 1,
    "username": "admin",
    "email": "admin@mosaicms.com",
    "displayName": "Administrator",
    "role": "Administrator",
    "isAuthenticated": true,
    "lastLoginOn": "2024-12-15T02:00:00Z"
  }
}
```

**Error Response** (401 Unauthorized):
```json
{
  "success": false,
  "errorMessage": "Invalid username or password."
}
```

### 2. Use JWT Token for API Calls

**Example**: Get authentication status

```bash
curl -X GET http://localhost:5000/api/authentication/status \
  -H "Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
```

## Usage Examples

### JavaScript/TypeScript (SPA, React, Vue, Angular)

```javascript
// 1. Get JWT token
async function login(username, password) {
  const response = await fetch('/api/authentication/token', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ username, password, rememberMe: false })
  });

  const data = await response.json();
  
  if (data.success) {
    // Store token securely
    localStorage.setItem('access_token', data.accessToken);
    localStorage.setItem('refresh_token', data.refreshToken);
    return data.user;
  } else {
    throw new Error(data.errorMessage);
  }
}

// 2. Use token for API calls
async function callAPI(endpoint) {
  const token = localStorage.getItem('access_token');
  
  const response = await fetch(endpoint, {
    headers: {
      'Authorization': `Bearer ${token}`,
      'Content-Type': 'application/json'
    }
  });

  if (response.status === 401) {
    // Token expired - redirect to login or refresh token
    throw new Error('Unauthorized - please login again');
  }

  return response.json();
}

// Example usage
const status = await callAPI('/api/authentication/status');
console.log('User:', status.user);
```

### C# (Mobile Apps, Desktop Apps, Microservices)

```csharp
using System.Net.Http;
using System.Net.Http.Json;
using System.Net.Http.Headers;

public class MosaicApiClient
{
    private readonly HttpClient _httpClient;
    private string? _accessToken;

    public MosaicApiClient(string baseUrl)
    {
        _httpClient = new HttpClient { BaseAddress = new Uri(baseUrl) };
    }

    public async Task<bool> LoginAsync(string username, string password)
    {
        var request = new { username, password, rememberMe = false };
        var response = await _httpClient.PostAsJsonAsync("/api/authentication/token", request);

        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<JwtTokenResponse>();
            if (result?.Success == true)
            {
                _accessToken = result.AccessToken;
                _httpClient.DefaultRequestHeaders.Authorization = 
                    new AuthenticationHeaderValue("Bearer", _accessToken);
                return true;
            }
        }

        return false;
    }

    public async Task<T?> GetAsync<T>(string endpoint)
    {
        var response = await _httpClient.GetAsync(endpoint);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<T>();
    }
}

// Usage
var client = new MosaicApiClient("https://your-app.azurewebsites.net");
await client.LoginAsync("admin", "Admin@123");
var status = await client.GetAsync<AuthenticationStatus>("/api/authentication/status");
```

### Python (Scripts, Automation)

```python
import requests

class MosaicClient:
    def __init__(self, base_url):
        self.base_url = base_url
        self.access_token = None

    def login(self, username, password):
        response = requests.post(
            f"{self.base_url}/api/authentication/token",
            json={"username": username, "password": password, "rememberMe": False}
        )
        
        data = response.json()
        if data.get("success"):
            self.access_token = data["accessToken"]
            return True
        return False

    def get(self, endpoint):
        headers = {"Authorization": f"Bearer {self.access_token}"}
        response = requests.get(f"{self.base_url}{endpoint}", headers=headers)
        return response.json()

# Usage
client = MosaicClient("https://your-app.azurewebsites.net")
client.login("admin", "Admin@123")
status = client.get("/api/authentication/status")
print(f"User: {status['user']}")
```

### cURL (Testing, Debugging)

```bash
# 1. Get token
TOKEN=$(curl -X POST http://localhost:5000/api/authentication/token \
  -H "Content-Type: application/json" \
  -d '{"username":"admin","password":"Admin@123","rememberMe":false}' \
  | jq -r '.accessToken')

# 2. Use token
curl -X GET http://localhost:5000/api/authentication/status \
  -H "Authorization: Bearer $TOKEN" \
  | jq .

# 3. Call protected endpoint
curl -X GET http://localhost:5000/api/subscription/current?userEmail=admin@mosaicms.com \
  -H "Authorization: Bearer $TOKEN" \
  | jq .
```

## Security Considerations

### Token Storage

**Browser/SPA Applications**:
- ❌ **Avoid localStorage** - vulnerable to XSS attacks
- ✅ **Use httpOnly cookies** - set by server, not accessible to JavaScript
- ✅ **sessionStorage** - cleared when tab closes (better than localStorage)
- ✅ **Memory only** - most secure for short-lived tokens

**Mobile/Desktop Apps**:
- ✅ **Secure storage** - Use platform-specific secure storage (Keychain, KeyStore)
- ✅ **Encrypted storage** - Encrypt tokens before storing

### Token Expiration

**Default Settings**:
- Access Token: 480 minutes (8 hours)
- Refresh Token: 30 days

**Best Practices**:
- Keep access tokens short-lived (< 1 hour for high-security apps)
- Use refresh tokens for long-term sessions
- Implement automatic token refresh
- Revoke tokens on logout

### HTTPS Required

**⚠️ CRITICAL**: Always use HTTPS in production
- JWT tokens are bearer tokens - anyone with the token has access
- HTTP transmits tokens in plain text
- Man-in-the-middle attacks can steal tokens

## Authorization with JWT

### Protecting API Endpoints

**Option 1**: Authorize attribute (works with both Cookie and JWT)
```csharp
[ApiController]
[Route("api/[controller]")]
[Authorize] // Accepts both Cookie and JWT authentication
public class MyController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var username = User.Identity?.Name;
        return Ok(new { userId, username });
    }
}
```

**Option 2**: Specific scheme
```csharp
[Authorize(AuthenticationSchemes = "JwtBearer")] // JWT only
public class ApiController : ControllerBase
{
    // JWT authentication required
}

[Authorize(AuthenticationSchemes = "Identity.Application")] // Cookie only
public class AdminController : ControllerBase
{
    // Cookie authentication required
}
```

**Option 3**: Role-based authorization
```csharp
[Authorize(Roles = "Administrator")]
public class AdminApiController : ControllerBase
{
    // Only users with Administrator role
}
```

## Troubleshooting

### "JWT authentication not configured"

**Cause**: SecretKey missing or too short

**Solution**:
```json
{
  "Authentication": {
    "Jwt": {
      "SecretKey": "minimum-32-characters-required-for-security-purposes"
    }
  }
}
```

### "401 Unauthorized" on API calls

**Cause**: Token expired or invalid

**Solution**:
1. Check token expiration: Decode JWT at jwt.io
2. Verify token is included: `Authorization: Bearer {token}`
3. Check server logs for validation errors
4. Regenerate token if expired

### "JWT token validation failed"

**Cause**: Issuer/Audience mismatch or wrong secret key

**Solution**:
1. Verify Issuer and Audience match between token generation and validation
2. Ensure SecretKey is the same in all environments
3. Check for whitespace in configuration values

## Comparison: Cookie vs JWT

| Feature | Cookie Auth | JWT Auth |
|---------|-------------|----------|
| **Use Case** | Blazor admin portal | API clients, mobile apps |
| **Storage** | Server-side session | Client-side token |
| **Scalability** | Requires sticky sessions | Stateless (scales easily) |
| **Security** | HttpOnly, SameSite | HTTPS required |
| **Expiration** | Sliding expiration | Fixed expiration |
| **Revocation** | Immediate | Requires token blacklist |
| **CSRF Protection** | Required | Not vulnerable |
| **Best For** | Traditional web apps | Distributed systems |

## Migration from Cookie-Only

Existing applications using cookie authentication continue to work without changes. JWT is additive:

1. ✅ Existing `/admin/login` (Blazor) → Uses cookies (no change)
2. ✅ New `/api/authentication/token` → Returns JWT for API clients
3. ✅ API endpoints work with both authentication schemes
4. ✅ No breaking changes

## Future Enhancements

Optional features that can be added following Oqtane's pattern:

- [ ] **Refresh Token Rotation** - Automatic token refresh for long-term sessions
- [ ] **Token Revocation** - Blacklist compromised tokens
- [ ] **Multi-Factor Authentication** - Require 2FA for JWT generation
- [ ] **Scope-based Authorization** - Fine-grained API permissions
- [ ] **OAuth 2.0 Support** - External identity providers
- [ ] **OpenID Connect** - Federated authentication

## Summary

Mosaic CMS now supports industry-standard JWT authentication following Oqtane's proven pattern:

✅ **Dual Authentication**: Cookie (Blazor) + JWT (APIs)  
✅ **Production Ready**: Secure token generation and validation  
✅ **Flexible**: Works with mobile apps, SPAs, and microservices  
✅ **Backward Compatible**: Existing authentication continues to work  
✅ **Oqtane Tested**: Based on battle-tested implementation  
✅ **Configurable**: Easy configuration via appsettings.json  

---

**Status**: ✅ **IMPLEMENTED**  
**Pattern**: Oqtane Framework  
**Date**: December 15, 2024  
**Security**: Production-ready with proper configuration
