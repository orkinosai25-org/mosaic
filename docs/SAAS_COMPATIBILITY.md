# SaaS Compatibility Adjustments

This document outlines the necessary adjustments to transform OrkinosaiCMS into the MOSAIC SaaS platform.

## Overview

OrkinosaiCMS provides a solid foundation as a single-tenant CMS. To transform it into MOSAIC, a multi-tenant SaaS platform, several architectural and configuration changes are required.

## 🔧 Required Adjustments

### 1. Database & Multi-Tenancy

#### Current State (OrkinosaiCMS)
- Single database per deployment
- No tenant isolation
- LocalDB/SQL Server configuration

#### Required Changes (MOSAIC)
- **Tenant Identification Strategy**: Implement host-based or subdomain-based tenant resolution
- **Data Isolation**: Choose between:
  - **Database per tenant**: Full isolation, easier backup/restore
  - **Schema per tenant**: Balance between isolation and management
  - **Shared database with TenantId**: Cost-effective, requires careful query filtering
- **Connection String Management**: Dynamic connection string resolution per tenant
- **Tenant Provisioning**: Automated tenant creation workflow

#### Implementation Files
- `src/OrkinosaiCMS.Infrastructure/Data/ApplicationDbContext.cs` - Add tenant filtering
- `src/OrkinosaiCMS.Web/Middleware/TenantResolutionMiddleware.cs` - NEW
- `src/OrkinosaiCMS.Core/Interfaces/ITenantService.cs` - NEW

### 2. Authentication & Authorization

#### Current State (OrkinosaiCMS)
- Basic authentication with roles
- Single-tenant user management

#### Required Changes (MOSAIC)
- **Identity Provider Integration**: Azure AD B2C or Auth0
- **OAuth 2.0 Providers**: Google, Microsoft, GitHub
- **Tenant-Scoped Users**: Users belong to specific tenants
- **Cross-Tenant Access**: Support for users managing multiple tenants
- **API Keys**: For programmatic access per tenant

#### Implementation Files
- `src/OrkinosaiCMS.Web/Services/AuthenticationService.cs` - Enhance for OAuth
- `src/OrkinosaiCMS.Infrastructure/Services/UserService.cs` - Add tenant scoping
- `appsettings.json` - Add OAuth configuration sections

### 3. Branding & Theming

#### Current State (OrkinosaiCMS)
- Generic theme system
- 6 pre-built themes (Dashboard, Marketing, Minimal, Orkinosai, SharePoint Portal, Top Navigation)

#### Required Changes (MOSAIC)
- **Professional Themes**: Create 6+ themes based on modern design principles
  - Modern Classic Theme (clean geometric design)
  - Contemporary Theme (professional styling)
  - Executive Theme (premium appearance)
  - Creative Theme (bold and modern)
  - Minimal Theme (clean and simple)
  - Business Theme (corporate professional)
- **Color Palettes**: Implement professional color schemes
  - Cobalt blues: `#1e3a8a`, `#2563eb`
  - Turquoise accents: `#06b6d4`, `#22d3ee`
  - Gold highlights: `#fbbf24`, `#f59e0b`
- **Pattern Library**: Modern geometric patterns and design elements
- **Tenant-Specific Branding**: Custom logos, colors per tenant

#### Implementation Files
- `src/OrkinosaiCMS.Web/wwwroot/css/themes/` - Create new Ottoman themes
- `src/OrkinosaiCMS.Infrastructure/Services/ThemeService.cs` - Enhance for tenant branding
- `logo/` - MOSAIC brand assets (already present)

### 4. Configuration Management

#### Current State (OrkinosaiCMS)
- Static appsettings.json
- LocalDB connection string

#### Required Changes (MOSAIC)
- **Azure Key Vault Integration**: Store secrets securely
  - Database connection strings
  - API keys (Stripe, SendGrid, etc.)
  - OAuth client secrets
- **Environment-Specific Configs**: Dev, Staging, Production
- **Feature Flags**: Enable/disable features per tenant or globally
- **Tenant-Specific Settings**: Allow customization per tenant

#### Configuration Sections to Add

```json
{
  "MultiTenancy": {
    "Enabled": true,
    "TenantIdentificationStrategy": "Host", // Host, Subdomain, or Header
    "DefaultTenantId": "system"
  },
  "Payment": {
    "Provider": "Stripe",
    "StripePublishableKey": "[Azure Key Vault]",
    "StripeSecretKey": "[Azure Key Vault]",
    "WebhookSecret": "[Azure Key Vault]"
  },
  "Email": {
    "Provider": "SendGrid",
    "ApiKey": "[Azure Key Vault]",
    "FromAddress": "noreply@mosaic.orkinosai.com",
    "FromName": "MOSAIC Platform"
  },
  "OAuth": {
    "Google": {
      "ClientId": "[Azure Key Vault]",
      "ClientSecret": "[Azure Key Vault]"
    },
    "Microsoft": {
      "ClientId": "[Azure Key Vault]",
      "ClientSecret": "[Azure Key Vault]"
    }
  },
  "MicrosoftFounderHub": {
    "Enabled": true,
    "BenefitsEndpoint": "[URL]"
  }
}
```

### 5. Payment Integration

#### Required Implementation (New Feature)
- **Stripe Integration**:
  - Payment method collection
  - Subscription management
  - Usage tracking and metering
  - Invoice generation
  - Webhook handling for events
- **Subscription Tiers**:
  - Free: 1 site, basic features
  - Pro: 5 sites, advanced features
  - Business: 25 sites, team collaboration
  - Enterprise: Unlimited sites, custom features
- **Microsoft Founder Hub Support**: Credit application and tracking

#### Implementation Files (New)
- `src/OrkinosaiCMS.Web/Controllers/PaymentController.cs`
- `src/OrkinosaiCMS.Infrastructure/Services/SubscriptionService.cs`
- `src/OrkinosaiCMS.Core/Entities/Subscription.cs`
- `src/OrkinosaiCMS.Core/Entities/PaymentMethod.cs`

### 6. User Onboarding

#### Current State (OrkinosaiCMS)
- Manual setup required
- No guided wizard

#### Required Changes (MOSAIC)
- **Sign-Up Flow**: Streamlined registration with email verification
- **Onboarding Wizard**: 5-step guided setup
  1. Account creation
  2. Tenant setup (site name, subdomain)
  3. Theme selection (Ottoman-inspired themes)
  4. Payment method (if not free tier)
  5. First site creation
- **Email Verification**: Automated email with verification link
- **Welcome Tour**: Interactive guide for new users

#### Implementation Files (New)
- `src/OrkinosaiCMS.Web/Components/Pages/Onboarding/`
  - `SignUp.razor`
  - `VerifyEmail.razor`
  - `TenantSetup.razor`
  - `ThemeSelection.razor`
  - `PaymentSetup.razor`
  - `WelcomeTour.razor`

### 7. API & Webhooks

#### Current State (OrkinosaiCMS)
- Internal API for CMS operations
- No public API

#### Required Changes (MOSAIC)
- **Public REST API**: Versioned, documented API
- **API Authentication**: JWT tokens, API keys per tenant
- **Rate Limiting**: Fair usage policies
- **Webhooks**: Event notifications for:
  - Site created/updated/deleted
  - User joined/left
  - Payment received/failed
  - Content published
- **API Documentation**: Swagger/OpenAPI with examples

#### Implementation Files
- `src/OrkinosaiCMS.Web/Controllers/Api/V1/` - API controllers
- `src/OrkinosaiCMS.Web/Middleware/RateLimitingMiddleware.cs` - NEW
- `src/OrkinosaiCMS.Infrastructure/Services/WebhookService.cs` - NEW

### 8. Analytics & Monitoring

#### Required Implementation (New Feature)
- **Application Insights**: Azure Application Insights integration
- **Custom Metrics**: Track usage, performance, errors
- **Tenant-Specific Dashboards**: Per-tenant analytics
- **Alerting**: Automated alerts for critical issues
- **Usage Tracking**: For billing and optimization

#### Implementation Files (New)
- `src/OrkinosaiCMS.Web/Services/AnalyticsService.cs`
- `src/OrkinosaiCMS.Web/Middleware/TelemetryMiddleware.cs`

### 9. Domain Management

#### Required Implementation (New Feature)
- **Custom Domains**: Allow tenants to use their own domains
- **SSL Certificates**: Automatic provisioning via Let's Encrypt
- **DNS Configuration**: Guidance and validation
- **Domain Verification**: Prove ownership before activation

#### Implementation Files (New)
- `src/OrkinosaiCMS.Infrastructure/Services/DomainService.cs`
- `src/OrkinosaiCMS.Web/Controllers/DomainController.cs`

### 10. AI Agent Enhancement

#### Current State (OrkinosaiCMS)
- Zoota Admin Agent (basic implementation)
- Chat interface in place

#### Required Changes (MOSAIC)
- **MOSAIC Public Agent**: Customer-facing AI assistant
  - Content creation help
  - SEO recommendations
  - Design guidance
  - Accessibility checks
- **Enhanced Zoota Admin Agent**: 
  - Tenant management automation
  - Usage analytics and insights
  - Proactive issue detection
  - Resource optimization recommendations

#### Implementation Files
- `src/OrkinosaiCMS.Web/Components/Shared/ChatAgent.razor` - Enhance
- `src/OrkinosaiCMS.Web/Controllers/ZootaCmsController.cs` - Add MOSAIC agent
- `src/OrkinosaiCMS.Web/PythonBackend/` - Enhance AI capabilities

## 🔐 Security Considerations

### Secrets Management
- ❌ **Do NOT**: Store secrets in appsettings.json
- ✅ **Do**: Use Azure Key Vault for all production secrets
- ✅ **Do**: Use User Secrets for local development

### Example: Using Azure Key Vault

```csharp
// In Program.cs
if (builder.Environment.IsProduction())
{
    var keyVaultEndpoint = new Uri(builder.Configuration["KeyVaultEndpoint"]);
    builder.Configuration.AddAzureKeyVault(
        keyVaultEndpoint,
        new DefaultAzureCredential());
}
```

### Data Isolation
- All queries must include tenant filtering
- Use global query filters in Entity Framework
- Validate tenant access on every API call

### Example: Global Query Filter

```csharp
// In ApplicationDbContext.OnModelCreating
modelBuilder.Entity<Site>()
    .HasQueryFilter(s => s.TenantId == _currentTenant.Id);
```

## 📦 Infrastructure Requirements

### Azure Resources
1. **Azure App Service**: Host the web application
2. **Azure SQL Database**: Multi-tenant database
3. **Azure Key Vault**: Secrets management
4. **Azure Storage**: File storage for user uploads
5. **Azure CDN**: Static asset delivery
6. **Application Insights**: Monitoring and analytics
7. **Azure Front Door**: Global load balancing (optional)

### Third-Party Services
1. **Stripe**: Payment processing
2. **SendGrid**: Transactional emails
3. **Cloudflare**: DNS and CDN (optional)
4. **Let's Encrypt**: SSL certificates

## 📋 Migration Checklist

### Phase 1: Foundation
- [ ] Implement tenant resolution middleware
- [ ] Add TenantId to all entities
- [ ] Create tenant provisioning workflow
- [ ] Set up Azure Key Vault integration
- [ ] Configure environment-specific settings

### Phase 2: Authentication
- [ ] Integrate Azure AD B2C or Auth0
- [ ] Add OAuth providers (Google, Microsoft, GitHub)
- [ ] Implement tenant-scoped user management
- [ ] Create API key generation system

### Phase 3: Payment
- [ ] Integrate Stripe SDK
- [ ] Implement subscription management
- [ ] Create webhook handlers
- [ ] Build billing dashboard
- [ ] Add Microsoft Founder Hub integration

### Phase 4: Branding
- [ ] Design 6+ Ottoman-inspired themes
- [ ] Create Iznik pattern library
- [ ] Build theme customization UI
- [ ] Implement tenant branding system

### Phase 5: Features
- [ ] Build onboarding wizard
- [ ] Create public REST API
- [ ] Implement webhooks
- [ ] Add custom domain support
- [ ] Deploy MOSAIC Public Agent

### Phase 6: Polish
- [ ] Set up Application Insights
- [ ] Create admin analytics dashboard
- [ ] Write comprehensive documentation
- [ ] Conduct security audit
- [ ] Perform load testing

## 🔄 Backward Compatibility

To maintain compatibility with OrkinosaiCMS while building MOSAIC:

1. **Feature Flags**: Use feature flags to enable/disable SaaS features
2. **Configuration**: Support both single-tenant and multi-tenant modes
3. **Database**: Allow migration from single-tenant to multi-tenant
4. **Documentation**: Maintain separate docs for CMS vs. SaaS usage

## 📚 Additional Resources

- [Multi-Tenancy Patterns in .NET](https://learn.microsoft.com/en-us/azure/architecture/patterns/multi-tenancy)
- [Azure Key Vault Best Practices](https://learn.microsoft.com/en-us/azure/key-vault/general/best-practices)
- [Stripe Integration Guide](https://stripe.com/docs/payments)
- [OAuth 2.0 Specification](https://oauth.net/2/)

## 🤝 Questions or Issues?

If you have questions about SaaS compatibility adjustments:
- Review the [Architecture Guide](ARCHITECTURE.md)
- Check the [Setup Guide](SETUP.md)
- Open a discussion on GitHub

---

**Last Updated**: December 9, 2025  
**Document Version**: 1.0
