# MOSAIC Application Settings Guide

This document provides comprehensive guidance on configuring the MOSAIC SaaS platform through `appsettings.json`, environment variables, Azure Key Vault, and user secrets.

## 📋 Configuration Overview

MOSAIC uses the standard ASP.NET Core configuration system with multiple layers:

```
Priority (lowest to highest):
1. appsettings.json (base configuration)
2. appsettings.{Environment}.json (environment-specific)
3. User Secrets (development only)
4. Environment Variables
5. Azure Key Vault (production)
6. Command-line arguments
```

## 🔧 Complete Configuration Reference

### Base Configuration (`appsettings.json`)

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Microsoft.EntityFrameworkCore": "Information"
    }
  },
  "AllowedHosts": "*",
  
  "ApplicationSettings": {
    "ApplicationName": "MOSAIC SaaS Platform",
    "Version": "1.0.0",
    "Environment": "Production",
    "DefaultTheme": "Modern",
    "DefaultLanguage": "en-US",
    "SupportEmail": "support@mosaic.orkinosai.com",
    "MaxUploadSizeInMB": 10
  },

  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=MosaicCMS;Trusted_Connection=True;",
    "AzureSqlConnection": "",
    "RedisConnection": ""
  },

  "Authentication": {
    "JwtSettings": {
      "SecretKey": "USE_AZURE_KEY_VAULT_IN_PRODUCTION",
      "Issuer": "https://mosaic.orkinosai.com",
      "Audience": "https://mosaic.orkinosai.com",
      "ExpirationInMinutes": 60,
      "RefreshTokenExpirationInDays": 7
    },
    "OAuth": {
      "Google": {
        "ClientId": "",
        "ClientSecret": "USE_AZURE_KEY_VAULT",
        "CallbackPath": "/signin-google"
      },
      "GitHub": {
        "ClientId": "",
        "ClientSecret": "USE_AZURE_KEY_VAULT",
        "CallbackPath": "/signin-github"
      },
      "Microsoft": {
        "ClientId": "",
        "ClientSecret": "USE_AZURE_KEY_VAULT",
        "CallbackPath": "/signin-microsoft"
      }
    },
    "AzureAdB2C": {
      "Instance": "https://login.microsoftonline.com/",
      "Domain": "mosaicsaas.onmicrosoft.com",
      "TenantId": "",
      "ClientId": "",
      "ClientSecret": "USE_AZURE_KEY_VAULT",
      "SignUpSignInPolicyId": "B2C_1_signup_signin",
      "ResetPasswordPolicyId": "B2C_1_password_reset",
      "EditProfilePolicyId": "B2C_1_profile_edit"
    },
    "CookieSettings": {
      "CookieName": "MOSAIC.Auth",
      "ExpireTimeSpan": "01:00:00",
      "SlidingExpiration": true,
      "HttpOnly": true,
      "SecurePolicy": "Always",
      "SameSite": "Lax"
    }
  },

  "AzureBlobStorage": {
    "AccountName": "mosaicsaas",
    "PrimaryEndpoint": "https://mosaicsaas.blob.core.windows.net/",
    "Location": "uksouth",
    "SKU": "Standard_RAGRS",
    "ResourceId": "/subscriptions/0142b600-b263-48d1-83fe-3ead960e1781/resourceGroups/orkinosai_group/providers/Microsoft.Storage/storageAccounts/mosaicsaas",
    "Endpoints": {
      "Blob": "https://mosaicsaas.blob.core.windows.net/",
      "File": "https://mosaicsaas.file.core.windows.net/",
      "Queue": "https://mosaicsaas.queue.core.windows.net/",
      "Table": "https://mosaicsaas.table.core.windows.net/",
      "Dfs": "https://mosaicsaas.dfs.core.windows.net/"
    },
    "Security": {
      "PublicAccess": false,
      "EncryptionEnabled": true,
      "FileSharesEnabled": true,
      "MinimumTlsVersion": "TLS1_2",
      "AllowBlobPublicAccess": false,
      "SupportsHttpsTrafficOnly": true
    },
    "Containers": {
      "MediaAssets": "media-assets",
      "UserUploads": "user-uploads",
      "Documents": "documents",
      "Backups": "backups",
      "Images": "images"
    },
    "ConnectionStringKey": "AzureBlobStorageConnectionString",
    "UseManagedIdentity": true,
    "SasTokenExpiryMinutes": 60
  },

  "DatabaseSettings": {
    "Provider": "SqlServer",
    "TierConfig": {
      "Free": {
        "Provider": "SQLite",
        "DatabasePath": "./data/tenants",
        "MaxDatabaseSizeInMB": 100,
        "BackupEnabled": false
      },
      "Paid": {
        "Provider": "AzureSQL",
        "SchemaIsolation": true,
        "MaxConnections": 100,
        "ConnectionTimeout": 30,
        "CommandTimeout": 30,
        "EnableRetryOnFailure": true,
        "MaxRetryCount": 3,
        "BackupRetentionDays": 7
      },
      "Enterprise": {
        "Provider": "AzureSQL",
        "DedicatedDatabase": true,
        "MaxConnections": 500,
        "ConnectionTimeout": 30,
        "CommandTimeout": 60,
        "EnableRetryOnFailure": true,
        "MaxRetryCount": 5,
        "BackupRetentionDays": 35,
        "PointInTimeRestore": true
      }
    }
  },

  "MultiTenant": {
    "TenantResolutionStrategy": "DomainBased",
    "DefaultTenantId": "default",
    "RequireTenantId": true,
    "CacheTenantInfo": true,
    "CacheDurationMinutes": 60,
    "TenantHeaderName": "X-Tenant-Id"
  },

  "Payment": {
    "Stripe": {
      "PublishableKey": "",
      "SecretKey": "USE_AZURE_KEY_VAULT",
      "WebhookSecret": "USE_AZURE_KEY_VAULT",
      "ApiVersion": "2024-11-20.acacia",
      "Currency": "usd",
      "EnableTestMode": false
    },
    "Plans": {
      "Free": {
        "Name": "Free",
        "PriceMonthly": 0,
        "PriceYearly": 0,
        "StripePriceIdMonthly": "",
        "StripePriceIdYearly": "",
        "MaxSites": 1,
        "MaxBandwidthGB": 10,
        "MaxStorageGB": 1,
        "Features": [
          "Basic themes",
          "Community support",
          "MOSAIC subdomain",
          "MOSAIC branding required"
        ]
      },
      "Pro": {
        "Name": "Pro",
        "PriceMonthly": 29,
        "PriceYearly": 290,
        "StripePriceIdMonthly": "price_pro_monthly",
        "StripePriceIdYearly": "price_pro_yearly",
        "MaxSites": 5,
        "MaxBandwidthGB": 100,
        "MaxStorageGB": 100,
        "Features": [
          "All themes",
          "Custom domain",
          "Email support",
          "Basic analytics",
          "Remove branding",
          "API access (10k req/month)"
        ]
      },
      "Business": {
        "Name": "Business",
        "PriceMonthly": 99,
        "PriceYearly": 990,
        "StripePriceIdMonthly": "price_business_monthly",
        "StripePriceIdYearly": "price_business_yearly",
        "MaxSites": 20,
        "MaxBandwidthGB": 500,
        "MaxStorageGB": 500,
        "Features": [
          "Premium themes",
          "Custom CSS",
          "Advanced analytics",
          "Priority support",
          "MOSAIC Public AI agent",
          "API access (100k req/month)",
          "Team collaboration (5 users)"
        ]
      },
      "Enterprise": {
        "Name": "Enterprise",
        "PriceMonthly": 0,
        "PriceYearly": 0,
        "ContactSales": true,
        "MaxSites": -1,
        "MaxBandwidthGB": -1,
        "MaxStorageGB": -1,
        "Features": [
          "All features",
          "Unlimited sites",
          "White-label",
          "24/7 phone support",
          "Zoota Admin AI agent",
          "Unlimited API access",
          "Custom SLA",
          "Dedicated account manager"
        ]
      }
    }
  },

  "EmailSettings": {
    "Provider": "SendGrid",
    "SendGrid": {
      "ApiKey": "USE_AZURE_KEY_VAULT",
      "FromEmail": "noreply@mosaic.orkinosai.com",
      "FromName": "MOSAIC Platform"
    },
    "SMTP": {
      "Host": "smtp.sendgrid.net",
      "Port": 587,
      "Username": "apikey",
      "Password": "USE_AZURE_KEY_VAULT",
      "EnableSsl": true
    },
    "Templates": {
      "EmailVerification": "d-verification-template-id",
      "PasswordReset": "d-password-reset-template-id",
      "Welcome": "d-welcome-template-id",
      "Invoice": "d-invoice-template-id"
    }
  },

  "CacheSettings": {
    "Provider": "Redis",
    "Redis": {
      "Configuration": "",
      "InstanceName": "MOSAIC:",
      "AbsoluteExpirationMinutes": 60,
      "SlidingExpirationMinutes": 20
    },
    "InMemory": {
      "SizeLimit": 100,
      "CompactionPercentage": 0.25
    },
    "EnableDistributedCache": true,
    "CacheProfiles": {
      "Default": {
        "Duration": 60,
        "VaryByQueryKeys": ["*"]
      },
      "Static": {
        "Duration": 3600,
        "Location": "Any"
      },
      "NoCache": {
        "Duration": 0,
        "NoStore": true
      }
    }
  },

  "AzureOpenAI": {
    "Endpoint": "https://your-resource.openai.azure.com/",
    "ApiKey": "USE_AZURE_KEY_VAULT",
    "DeploymentName": "gpt-4",
    "ApiVersion": "2024-02-15-preview",
    "MaxTokens": 2000,
    "Temperature": 0.7,
    "Agents": {
      "MosaicPublic": {
        "SystemPrompt": "You are MOSAIC Public Agent, an AI assistant helping users create and manage their websites using conversational interface. Be helpful, friendly, and knowledgeable about web design and modern best practices.",
        "MaxConversationHistory": 10
      },
      "ZootaAdmin": {
        "SystemPrompt": "You are Zoota Admin Agent, an AI assistant helping administrators manage the MOSAIC platform. Provide technical insights, system health information, and administrative guidance.",
        "MaxConversationHistory": 20
      }
    }
  },

  "ApplicationInsights": {
    "ConnectionString": "",
    "EnableAdaptiveSampling": true,
    "EnablePerformanceCounterCollection": true,
    "EnableDependencyTracking": true,
    "EnableEventCounterCollection": true,
    "InstrumentationKey": ""
  },

  "Security": {
    "Cors": {
      "AllowedOrigins": [
        "https://mosaic.orkinosai.com",
        "https://*.mosaic.app"
      ],
      "AllowedMethods": ["GET", "POST", "PUT", "DELETE", "OPTIONS"],
      "AllowedHeaders": ["*"],
      "AllowCredentials": true,
      "MaxAge": 3600
    },
    "RateLimiting": {
      "EnableRateLimiting": true,
      "PermitLimit": 100,
      "WindowInSeconds": 60,
      "QueueLimit": 10
    },
    "ContentSecurityPolicy": {
      "Enabled": true,
      "DefaultSrc": ["'self'"],
      "ScriptSrc": ["'self'", "'unsafe-inline'", "https://cdn.mosaic.app"],
      "StyleSrc": ["'self'", "'unsafe-inline'", "https://fonts.googleapis.com"],
      "ImgSrc": ["'self'", "data:", "https:"],
      "FontSrc": ["'self'", "https://fonts.gstatic.com"],
      "ConnectSrc": ["'self'", "https://api.mosaic.app"]
    },
    "PasswordPolicy": {
      "RequireDigit": true,
      "RequireLowercase": true,
      "RequireUppercase": true,
      "RequireNonAlphanumeric": true,
      "RequiredLength": 12,
      "RequiredUniqueChars": 4
    }
  },

  "Features": {
    "EnableUserRegistration": true,
    "EnableOAuthLogin": true,
    "EnableEmailVerification": true,
    "EnableTwoFactorAuth": false,
    "EnableApiAccess": true,
    "EnableAIAgents": true,
    "EnableAnalytics": true,
    "EnableMigrationTools": true,
    "MaintenanceMode": false
  },

  "UI": {
    "Theme": {
      "DefaultTheme": "Modern",
      "AllowCustomThemes": true,
      "DefaultColorScheme": "Light"
    },
    "Branding": {
      "PlatformName": "MOSAIC",
      "ShowPoweredBy": true,
      "AllowCustomBranding": true
    },
    "Portal": {
      "HeaderLogo": "/assets/logo.svg",
      "FaviconPath": "/favicon.ico",
      "DefaultDashboard": "Sites",
      "EnableSidebar": true,
      "EnableBreadcrumbs": true
    }
  }
}
```

## 🔒 Secrets Management

### Development: User Secrets

Initialize user secrets for local development:

```bash
# Navigate to project directory
cd src/MosaicCMS

# Initialize user secrets
dotnet user-secrets init

# Add sensitive configuration
dotnet user-secrets set "ConnectionStrings:AzureBlobStorageConnectionString" "DefaultEndpointsProtocol=https;AccountName=mosaicsaas;AccountKey=YOUR_KEY;EndpointSuffix=core.windows.net"

dotnet user-secrets set "Authentication:JwtSettings:SecretKey" "your-super-secret-key-min-32-chars"

dotnet user-secrets set "Payment:Stripe:SecretKey" "sk_test_YOUR_STRIPE_KEY"

dotnet user-secrets set "Payment:Stripe:WebhookSecret" "whsec_YOUR_WEBHOOK_SECRET"

dotnet user-secrets set "AzureOpenAI:ApiKey" "YOUR_AZURE_OPENAI_KEY"

dotnet user-secrets set "EmailSettings:SendGrid:ApiKey" "SG.YOUR_SENDGRID_KEY"

# List all secrets
dotnet user-secrets list
```

### Production: Azure Key Vault

#### 1. Create Key Vault

```bash
# Create resource group (if not exists)
az group create --name orkinosai_group --location uksouth

# Create Key Vault
az keyvault create \
  --name mosaic-keyvault \
  --resource-group orkinosai_group \
  --location uksouth \
  --enable-soft-delete true \
  --enable-purge-protection true
```

#### 2. Add Secrets to Key Vault

```bash
# Azure Blob Storage Connection String
az keyvault secret set \
  --vault-name mosaic-keyvault \
  --name AzureBlobStorageConnectionString \
  --value "DefaultEndpointsProtocol=https;AccountName=mosaicsaas;AccountKey=YOUR_KEY;EndpointSuffix=core.windows.net"

# JWT Secret Key
az keyvault secret set \
  --vault-name mosaic-keyvault \
  --name JwtSecretKey \
  --value "your-production-secret-key-min-32-chars"

# Stripe Secret Key
az keyvault secret set \
  --vault-name mosaic-keyvault \
  --name StripeSecretKey \
  --value "sk_live_YOUR_STRIPE_KEY"

# Stripe Webhook Secret
az keyvault secret set \
  --vault-name mosaic-keyvault \
  --name StripeWebhookSecret \
  --value "whsec_YOUR_WEBHOOK_SECRET"

# Azure OpenAI API Key
az keyvault secret set \
  --vault-name mosaic-keyvault \
  --name AzureOpenAIApiKey \
  --value "YOUR_AZURE_OPENAI_KEY"

# SendGrid API Key
az keyvault secret set \
  --vault-name mosaic-keyvault \
  --name SendGridApiKey \
  --value "SG.YOUR_SENDGRID_KEY"

# Azure SQL Connection String (DefaultConnection)
az keyvault secret set \
  --vault-name mosaic-keyvault \
  --name DefaultConnection \
  --value "Server=tcp:orkinosai.database.windows.net,1433;Initial Catalog=mosaic-saas;Persist Security Info=False;User ID=sqladmin;Password=YOUR_PRODUCTION_PASSWORD;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30"

# Alternative Azure SQL Connection String
az keyvault secret set \
  --vault-name mosaic-keyvault \
  --name AzureSqlConnection \
  --value "Server=tcp:mosaic-sql.database.windows.net,1433;Database=MosaicCMS;..."

# Redis Connection String
az keyvault secret set \
  --vault-name mosaic-keyvault \
  --name RedisConnection \
  --value "mosaic-redis.redis.cache.windows.net:6380,password=YOUR_REDIS_KEY,ssl=True"
```

#### 3. Configure Managed Identity

```bash
# Enable system-assigned managed identity on App Service
az webapp identity assign \
  --name mosaic-app \
  --resource-group orkinosai_group

# Get the managed identity principal ID (will be output from previous command)
PRINCIPAL_ID=$(az webapp identity show \
  --name mosaic-app \
  --resource-group orkinosai_group \
  --query principalId -o tsv)

# Grant the App Service access to Key Vault
az keyvault set-policy \
  --name mosaic-keyvault \
  --object-id $PRINCIPAL_ID \
  --secret-permissions get list
```

#### 4. Update Application Configuration

Add to `appsettings.Production.json`:

```json
{
  "KeyVault": {
    "VaultUri": "https://mosaic-keyvault.vault.azure.net/"
  }
}
```

Update `Program.cs` to load Key Vault:

```csharp
// In Program.cs
var builder = WebApplication.CreateBuilder(args);

// Add Azure Key Vault
if (builder.Environment.IsProduction())
{
    var keyVaultUri = builder.Configuration["KeyVault:VaultUri"];
    if (!string.IsNullOrEmpty(keyVaultUri))
    {
        builder.Configuration.AddAzureKeyVault(
            new Uri(keyVaultUri),
            new DefaultAzureCredential());
    }
}
```

### Environment Variables Override

For cloud deployments (Azure, AWS, Docker), you can override any configuration setting using environment variables. This is particularly useful for connection strings and secrets.

#### Syntax

ASP.NET Core uses double underscores (`__`) to represent nested configuration sections:

```bash
# Format: Section__SubSection__Key=Value
ConnectionStrings__DefaultConnection="Server=...;Database=...;"
Payment__Stripe__SecretKey="sk_live_YOUR_KEY"
AzureOpenAI__ApiKey="YOUR_KEY"
```

#### Azure App Service Configuration

**Method 1: Application Settings**
1. Navigate to Azure Portal → Your App Service
2. Go to **Configuration** → **Application settings**
3. Click **+ New application setting**
4. Add setting:
   - Name: `ConnectionStrings__DefaultConnection`
   - Value: `Server=tcp:orkinosai.database.windows.net,1433;Initial Catalog=mosaic-saas;...`
5. Click **Save** and **Restart** the app

**Method 2: Connection Strings (Recommended for DB connections)**
1. Navigate to Azure Portal → Your App Service
2. Go to **Configuration** → **Connection strings**
3. Click **+ New connection string**
4. Add connection string:
   - Name: `DefaultConnection`
   - Value: `Server=tcp:orkinosai.database.windows.net,1433;Initial Catalog=mosaic-saas;Persist Security Info=False;User ID=sqladmin;Password=YOUR_PRODUCTION_PASSWORD;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30`
   - Type: `SQLServer`
5. Click **OK**, then **Save** and **Restart** the app

**Note:** Connection strings added in Azure Portal automatically override `appsettings.json` values without needing the `ConnectionStrings__` prefix.

#### Docker/Docker Compose

```yaml
# docker-compose.yml
version: '3.8'
services:
  mosaic:
    image: mosaic-app
    environment:
      - ConnectionStrings__DefaultConnection=Server=sql-server;Database=mosaic-saas;User ID=sa;Password=YourPassword123!
      - Payment__Stripe__SecretKey=sk_live_YOUR_KEY
      - ASPNETCORE_ENVIRONMENT=Production
```

Or using environment file:

```bash
# .env.production (Never commit this file!)
ConnectionStrings__DefaultConnection=Server=tcp:orkinosai.database.windows.net,1433;Initial Catalog=mosaic-saas;Persist Security Info=False;User ID=sqladmin;Password=YOUR_PASSWORD;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30
Payment__Stripe__SecretKey=sk_live_YOUR_STRIPE_KEY
Payment__Stripe__WebhookSecret=whsec_YOUR_WEBHOOK_SECRET
AzureOpenAI__ApiKey=YOUR_AZURE_OPENAI_KEY
```

```bash
# Run with environment file
docker run --env-file .env.production mosaic-app
```

#### AWS Elastic Beanstalk

1. Navigate to Elastic Beanstalk Console → Your Environment
2. Go to **Configuration** → **Software**
3. Under **Environment properties**, add:
   - `ConnectionStrings__DefaultConnection`: `Server=...`
   - `Payment__Stripe__SecretKey`: `sk_live_...`
4. Click **Apply**

#### Local Development (.NET User Secrets - Recommended)

```bash
# Navigate to project directory
cd src/OrkinosaiCMS.Web

# Set connection string
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=tcp:orkinosai.database.windows.net,1433;Initial Catalog=mosaic-saas;Persist Security Info=False;User ID=sqladmin;Password=YOUR_DEV_PASSWORD;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30"

# Verify
dotnet user-secrets list
```

#### Configuration Priority (Highest to Lowest)

1. **Command-line arguments** (highest priority)
2. **Environment variables**
3. **Azure Key Vault** (production)
4. **User Secrets** (development only)
5. **appsettings.{Environment}.json** (e.g., appsettings.Production.json)
6. **appsettings.json** (lowest priority)

This means environment variables will always override values in `appsettings.json`, making them safe for production deployments.

## 🌍 Environment-Specific Configuration

### Development (`appsettings.Development.json`)

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Microsoft.AspNetCore": "Information",
      "Microsoft.EntityFrameworkCore": "Information"
    }
  },
  "ApplicationSettings": {
    "Environment": "Development"
  },
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=MosaicCMS_Dev;Trusted_Connection=True;"
  },
  "Payment": {
    "Stripe": {
      "EnableTestMode": true
    }
  },
  "Features": {
    "EnableTwoFactorAuth": false
  },
  "AzureBlobStorage": {
    "UseManagedIdentity": false
  }
}
```

### Staging (`appsettings.Staging.json`)

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "ApplicationSettings": {
    "Environment": "Staging"
  },
  "ConnectionStrings": {
    "DefaultConnection": "Server=mosaic-sql-staging.database.windows.net;Database=MosaicCMS_Staging;..."
  },
  "Payment": {
    "Stripe": {
      "EnableTestMode": true
    }
  },
  "Features": {
    "MaintenanceMode": false
  }
}
```

### Production (`appsettings.Production.json`)

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Warning",
      "Microsoft.AspNetCore": "Error"
    }
  },
  "ApplicationSettings": {
    "Environment": "Production"
  },
  "KeyVault": {
    "VaultUri": "https://mosaic-keyvault.vault.azure.net/"
  },
  "ApplicationInsights": {
    "ConnectionString": "InstrumentationKey=YOUR_KEY;IngestionEndpoint=..."
  },
  "Security": {
    "Cors": {
      "AllowedOrigins": [
        "https://mosaic.orkinosai.com",
        "https://app.mosaic.orkinosai.com"
      ]
    }
  },
  "Features": {
    "EnableTwoFactorAuth": true
  }
}
```

## 🔧 Configuration by Tier

### Free Tier Configuration

```json
{
  "TierSettings": {
    "Tier": "Free",
    "DatabaseProvider": "SQLite",
    "MaxSites": 1,
    "MaxBandwidthGB": 10,
    "MaxStorageGB": 1,
    "EnableCustomDomain": false,
    "EnableAPIAccess": false,
    "EnableAIAgents": false,
    "SupportLevel": "Community"
  }
}
```

### Paid Tier Configuration

```json
{
  "TierSettings": {
    "Tier": "Pro",
    "DatabaseProvider": "AzureSQL",
    "MaxSites": 5,
    "MaxBandwidthGB": 100,
    "MaxStorageGB": 100,
    "EnableCustomDomain": true,
    "EnableAPIAccess": true,
    "APIRequestsPerMonth": 10000,
    "EnableAIAgents": false,
    "SupportLevel": "Email"
  }
}
```

### Enterprise Tier Configuration

```json
{
  "TierSettings": {
    "Tier": "Enterprise",
    "DatabaseProvider": "AzureSQL",
    "DedicatedInfrastructure": true,
    "MaxSites": -1,
    "MaxBandwidthGB": -1,
    "MaxStorageGB": -1,
    "EnableCustomDomain": true,
    "EnableAPIAccess": true,
    "APIRequestsPerMonth": -1,
    "EnableAIAgents": true,
    "SupportLevel": "24x7Phone",
    "CustomSLA": true,
    "WhiteLabelEnabled": true
  }
}
```

## 📝 Configuration Usage in Code

### Accessing Configuration

```csharp
// In Startup/Program.cs
builder.Services.Configure<ApplicationSettings>(
    builder.Configuration.GetSection("ApplicationSettings"));

builder.Services.Configure<StripeSettings>(
    builder.Configuration.GetSection("Payment:Stripe"));

// In a controller or service
public class MyService
{
    private readonly ApplicationSettings _appSettings;
    private readonly StripeSettings _stripeSettings;
    
    public MyService(
        IOptions<ApplicationSettings> appSettings,
        IOptions<StripeSettings> stripeSettings)
    {
        _appSettings = appSettings.Value;
        _stripeSettings = stripeSettings.Value;
    }
    
    public void DoSomething()
    {
        var appName = _appSettings.ApplicationName;
        var stripeKey = _stripeSettings.SecretKey;
        // Use settings...
    }
}
```

### Configuration Models

```csharp
public class ApplicationSettings
{
    public string ApplicationName { get; set; }
    public string Version { get; set; }
    public string Environment { get; set; }
    public string DefaultTheme { get; set; }
    public string SupportEmail { get; set; }
    public int MaxUploadSizeInMB { get; set; }
}

public class StripeSettings
{
    public string PublishableKey { get; set; }
    public string SecretKey { get; set; }
    public string WebhookSecret { get; set; }
    public string ApiVersion { get; set; }
    public string Currency { get; set; }
    public bool EnableTestMode { get; set; }
}

public class JwtSettings
{
    public string SecretKey { get; set; }
    public string Issuer { get; set; }
    public string Audience { get; set; }
    public int ExpirationInMinutes { get; set; }
    public int RefreshTokenExpirationInDays { get; set; }
}
```

## 🔐 Security Best Practices

### ✅ DO

- Store all secrets in Azure Key Vault (production)
- Use user secrets for local development
- Rotate keys and secrets regularly
- Use managed identities when possible
- Enable Key Vault soft delete and purge protection
- Audit Key Vault access logs
- Use separate Key Vaults for dev/staging/prod
- Validate all configuration values at startup

### ❌ DON'T

- Commit secrets to source control
- Log configuration values containing secrets
- Share production secrets via email/chat
- Use production secrets in development
- Hardcode connection strings
- Store API keys in plain text
- Use weak JWT secret keys (< 32 characters)

## 🧪 Testing Configuration

### Unit Tests

```csharp
[Fact]
public void Configuration_LoadsSuccessfully()
{
    var configuration = new ConfigurationBuilder()
        .AddJsonFile("appsettings.json")
        .AddJsonFile("appsettings.Development.json")
        .Build();

    var appSettings = configuration
        .GetSection("ApplicationSettings")
        .Get<ApplicationSettings>();

    Assert.NotNull(appSettings);
    Assert.Equal("MOSAIC SaaS Platform", appSettings.ApplicationName);
}
```

### Configuration Validation

```csharp
// In Program.cs
public static void ValidateConfiguration(IConfiguration configuration)
{
    var errors = new List<string>();

    // Validate required settings
    if (string.IsNullOrEmpty(configuration["ApplicationSettings:ApplicationName"]))
        errors.Add("ApplicationName is required");

    if (string.IsNullOrEmpty(configuration["AzureBlobStorage:AccountName"]))
        errors.Add("Azure Blob Storage AccountName is required");

    if (errors.Any())
    {
        throw new InvalidOperationException(
            $"Configuration validation failed:\n{string.Join("\n", errors)}");
    }
}
```

## 📚 Additional Resources

- [ASP.NET Core Configuration](https://docs.microsoft.com/aspnet/core/fundamentals/configuration/)
- [Azure Key Vault Configuration Provider](https://docs.microsoft.com/aspnet/core/security/key-vault-configuration)
- [User Secrets in ASP.NET Core](https://docs.microsoft.com/aspnet/core/security/app-secrets)
- [Environment Variables](https://docs.microsoft.com/aspnet/core/fundamentals/configuration/#environment-variables)
- [MOSAIC Architecture](./architecture.md)
- [Azure Blob Storage Integration](./AZURE_BLOB_STORAGE.md)

---

**Last Updated:** December 2024  
**Version:** 1.0  
**Maintained by:** Orkinosai Team
