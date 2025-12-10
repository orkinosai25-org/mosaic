# MOSAIC SaaS Deployment Architecture

## Overview

This document describes the deployment architecture for MOSAIC, a multi-tenant SaaS platform combining a React Portal frontend with a .NET 10 Blazor CMS backend.

## Architecture Diagram

```
┌─────────────────────────────────────────────────────────────────┐
│                     Azure Web App (mosaic-saas)                  │
│                                                                   │
│  ┌───────────────────────────────────────────────────────────┐  │
│  │                    wwwroot (Static Files)                  │  │
│  │                                                             │  │
│  │  ┌──────────────────────────────────────────────────────┐  │  │
│  │  │  React Portal (SPA)                                   │  │  │
│  │  │  • index.html                                        │  │  │
│  │  │  • /assets/*.js, *.css                               │  │  │
│  │  │  • /mosaic-*.svg (branding)                          │  │  │
│  │  │                                                       │  │  │
│  │  │  Serves at: /                                        │  │  │
│  │  └──────────────────────────────────────────────────────┘  │  │
│  │                                                             │  │
│  │  ┌──────────────────────────────────────────────────────┐  │  │
│  │  │  Blazor Static Assets                                │  │  │
│  │  │  • /_framework/*                                     │  │  │
│  │  │  • /_content/*                                       │  │  │
│  │  │  • app.css, OrkinosaiCMS.Web.styles.css            │  │  │
│  │  └──────────────────────────────────────────────────────┘  │  │
│  └───────────────────────────────────────────────────────────┘  │
│                                                                   │
│  ┌───────────────────────────────────────────────────────────┐  │
│  │              .NET 10 Application Routes                    │  │
│  │                                                             │  │
│  │  /                   → React Portal (index.html)          │  │
│  │  /api/*              → API Controllers                     │  │
│  │  /admin              → Blazor Admin (authenticated)       │  │
│  │  /admin/*            → Blazor Admin Pages                  │  │
│  │  /cms                → CMS Home (redirects to /cms-home)   │  │
│  │  /cms-*              → CMS Content Pages                   │  │
│  │  /_framework/*       → Blazor Framework Files             │  │
│  │  /_content/*         → Blazor Content Files               │  │
│  └───────────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────────┘
```

## Request Flow

### 1. Public Access (Unauthenticated)

**Scenario**: User visits `https://mosaic-saas.azurewebsites.net/`

```
User → https://mosaic-saas.azurewebsites.net/
     → Static Files Middleware (UseStaticFiles)
     → Serves /wwwroot/index.html
     → React Portal SPA loads
     → Shows Landing Page
```

**Routes Accessible**:
- `/` - Landing page (React SPA)
- `/assets/*` - React assets (JS, CSS, images)
- Any React router path handled by SPA

### 2. Authentication Flow

**Scenario**: User clicks "Register" or "Sign In"

```
User clicks "Register/Sign In" in Portal
     → React handles form display
     → User submits credentials
     → POST to /api/auth/register or /api/auth/login
     → API Controller validates
     → Returns JWT token
     → Portal stores token in localStorage
     → Portal redirects to Dashboard
```

### 3. Authenticated Dashboard Access

**Scenario**: Authenticated user accesses dashboard

```
User → https://mosaic-saas.azurewebsites.net/ (with token)
     → React Portal loads
     → Checks localStorage for token
     → Shows authenticated Dashboard view
     → User can create/manage sites
```

### 4. CMS Admin Access

**Scenario**: User clicks "Configure Site" in Portal

```
User clicks "Configure Site" button
     → Portal navigates to /admin
     → Request goes to Blazor app
     → Blazor checks authentication
     → If authenticated: Shows admin interface
     → If not: Redirects to /admin/login
```

**Routes Accessible**:
- `/admin` - CMS Admin home
- `/admin/themes` - Theme management
- `/admin/login` - Admin login page
- `/cms-home`, `/cms-features`, etc. - CMS content pages

### 5. API Communication

**Scenario**: Portal needs data from backend

```
Portal Component needs data
     → Makes fetch() call to /api/sites
     → Includes JWT token in Authorization header
     → API Controller validates token
     → Returns JSON data
     → Portal updates UI
```

**API Endpoints**:
- `/api/auth/*` - Authentication endpoints
- `/api/sites/*` - Site management
- `/api/subscriptions/*` - Billing (future)
- `/api/media/*` - Azure Blob Storage operations

## Deployment Process

### GitHub Actions Workflow

1. **Trigger**: Push to `main` branch or manual workflow dispatch

2. **Build Frontend (Portal)**:
   ```bash
   cd frontend
   npm ci
   npm run build  # Creates /frontend/dist/
   ```

3. **Build Backend (CMS)**:
   ```bash
   dotnet restore OrkinosaiCMS.sln
   dotnet build --configuration Release
   dotnet publish src/OrkinosaiCMS.Web/OrkinosaiCMS.Web.csproj \
     --output ./publish
   ```

4. **Integration**:
   ```bash
   # Copy React build to backend's wwwroot
   cp -r ./frontend/dist/* ./publish/wwwroot/
   ```

5. **Deploy**:
   ```bash
   # Deploy to Azure using publish profile
   azure/webapps-deploy@v3
     app-name: mosaic-saas
     publish-profile: ${{ secrets.AZURE_WEBAPP_PUBLISH_PROFILE_MOSAIC }}
     package: ./publish
   ```

### Deployment Verification

After deployment completes, verify:

1. ✅ Portal Landing Page: `https://mosaic-saas.azurewebsites.net/`
   - Should show React landing page
   - No authentication required

2. ✅ CMS Admin: `https://mosaic-saas.azurewebsites.net/admin`
   - Should show Blazor admin interface
   - Requires authentication

3. ✅ API Health: `https://mosaic-saas.azurewebsites.net/api/health`
   - Should return 200 OK

4. ✅ Static Assets: `https://mosaic-saas.azurewebsites.net/assets/*`
   - React assets should load correctly

## Configuration

### Required Secrets

**GitHub Repository Secrets**:
- `AZURE_WEBAPP_PUBLISH_PROFILE_MOSAIC` - Downloaded from Azure Portal

### Azure Web App Settings

**Application Settings**:
```json
{
  "ASPNETCORE_ENVIRONMENT": "Production",
  "DatabaseProvider": "SqlServer",
  "ConnectionStrings:DefaultConnection": "<Azure SQL connection string>",
  "MultiTenancy:Enabled": "true",
  "MultiTenancy:TenantIdentificationStrategy": "Host"
}
```

**Connection Strings**:
- `DefaultConnection` - Azure SQL Database
- `AzureBlobStorage` - Azure Blob Storage for media

## Multi-Tenant Architecture

### Tenant Identification

- **Host-based**: Each tenant gets a subdomain (e.g., `tenant1.mosaic-saas.com`)
- **Header-based**: `X-Tenant-Id` header for API requests
- **Database**: Tenant-specific schema or tenant ID column

### Data Isolation

1. **Database**: Tenant ID in all tables
2. **Blob Storage**: Tenant-specific containers
3. **Authentication**: Tenant context in JWT claims

## Security Considerations

### Authentication Boundaries

- ✅ Portal (`/`): Public access for landing page
- ✅ Portal Dashboard: Requires JWT token
- ✅ CMS Admin (`/admin`): Requires Blazor authentication
- ✅ API (`/api/*`): Requires JWT token (except public endpoints)

### CORS Configuration

```csharp
// For Portal → API communication
builder.Services.AddCors(options =>
{
    options.AddPolicy("PortalPolicy", policy =>
    {
        policy.WithOrigins("https://mosaic-saas.azurewebsites.net")
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});
```

### HTTPS Enforcement

- All traffic redirected to HTTPS
- Azure Web App managed certificate
- Custom domain SSL support

## Future Enhancements

### Payment Integration (Stripe)

**Architecture Ready**:
- API endpoints: `/api/subscriptions/*`
- Portal UI: Billing pages prepared
- Webhook endpoint: `/api/webhooks/stripe`
- Secrets: Azure Key Vault for Stripe keys

### Scaling

**Horizontal Scaling**:
- Azure Web App can scale out (multiple instances)
- Stateless architecture (JWT-based auth)
- Azure SQL connection pooling
- Azure Blob Storage scales automatically

**Performance Optimization**:
- CDN for static assets (future)
- Response caching for API
- Database query optimization
- Blazor prerendering

## Monitoring

### Application Insights

- Request tracking
- Exception logging
- Performance metrics
- Custom events

### Health Checks

- `/health` - Application health
- `/health/db` - Database connectivity
- `/health/storage` - Blob storage connectivity

## Troubleshooting

### Issue: Portal Not Loading

**Check**:
1. Are static files in wwwroot?
2. Is index.html present?
3. Check browser console for 404s on assets

### Issue: CMS Admin Not Accessible

**Check**:
1. Is authentication configured?
2. Are Blazor routes registered?
3. Check /_framework files are present

### Issue: API Calls Failing

**Check**:
1. CORS policy configured?
2. JWT token included in request?
3. API controller routes correct?

## References

- [Azure Web Apps Documentation](https://docs.microsoft.com/azure/app-service/)
- [.NET 10 Deployment Guide](https://docs.microsoft.com/dotnet/core/deploying/)
- [React Deployment Best Practices](https://create-react-app.dev/docs/deployment/)
- [GitHub Actions for Azure](https://github.com/Azure/actions)

## Support

For deployment issues or questions:
- Check [DEPLOYMENT_CHECKLIST.md](./DEPLOYMENT_CHECKLIST.md)
- Review [Azure Deployment Guide](./AZURE_DEPLOYMENT.md)
- See [README.md](../README.md) for architecture overview
