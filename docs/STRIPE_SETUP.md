# Stripe API Keys Setup Guide

This guide explains how to configure Stripe API keys for the MOSAIC SaaS platform in local development, CI/CD pipelines, and production deployment environments.

## 🔑 Required Stripe API Keys

You need three keys from Stripe:

1. **STRIPE_PUBLISHABLE_KEY** - Safe to expose to frontend/client-side code
   - Format: `pk_test_*` (test) or `pk_live_*` (production)
   - Used for: Client-side Stripe.js library initialization

2. **STRIPE_SECRET_KEY** - MUST remain secret, backend/server-side only
   - Format: `sk_test_*` (test) or `sk_live_*` (production)
   - Used for: Server-side API calls to Stripe

3. **STRIPE_WEBHOOK_SECRET** - Used to verify webhook signatures
   - Format: `whsec_*`
   - Used for: Verifying that webhook events are from Stripe

## 📋 Getting Your Stripe API Keys

### Step 1: Create/Login to Stripe Account

1. Visit [Stripe Dashboard](https://dashboard.stripe.com/)
2. Sign up for a new account or log in to existing account
3. Complete account setup if prompted

### Step 2: Get API Keys

1. Navigate to **Developers** → **API keys** in the Stripe Dashboard
2. You'll see two types of keys:
   - **Test mode keys** (for development)
   - **Live mode keys** (for production)

3. Copy your keys:
   - **Publishable key**: Visible on the page (starts with `pk_test_` or `pk_live_`)
   - **Secret key**: Click "Reveal test key" or "Reveal live key" to view (starts with `sk_test_` or `sk_live_`)

### Step 3: Get Webhook Secret (Optional for Development)

1. Navigate to **Developers** → **Webhooks** in the Stripe Dashboard
2. Click **Add endpoint**
3. Enter your webhook URL (e.g., `https://mosaic-saas.azurewebsites.net/api/webhooks/stripe`)
4. Select events to listen for (e.g., `checkout.session.completed`, `customer.subscription.updated`)
5. Click **Add endpoint**
6. Copy the **Signing secret** (starts with `whsec_`)

## 🏠 Local Development Setup

### Option 1: Using .env File (Recommended)

1. Copy the example environment file:
   ```bash
   cp .env.example .env
   ```

2. Edit `.env` and add your Stripe test keys:
   ```bash
   STRIPE_PUBLISHABLE_KEY=pk_test_51AbCdEf...
   STRIPE_SECRET_KEY=sk_test_51AbCdEf...
   STRIPE_WEBHOOK_SECRET=whsec_123456...
   ```

3. **Important**: The `.env` file is already in `.gitignore` and will NOT be committed to Git.

### Option 2: Using .NET User Secrets (Alternative)

For backend configuration, you can use .NET user secrets:

```bash
cd src/OrkinosaiCMS.Web

# Initialize user secrets (if not already done)
dotnet user-secrets init

# Add Stripe keys
dotnet user-secrets set "Payment:Stripe:PublishableKey" "pk_test_51AbCdEf..."
dotnet user-secrets set "Payment:Stripe:SecretKey" "sk_test_51AbCdEf..."
dotnet user-secrets set "Payment:Stripe:WebhookSecret" "whsec_123456..."

# List all secrets
dotnet user-secrets list
```

### Option 3: Using Environment Variables

Set environment variables directly in your shell:

**Windows (PowerShell):**
```powershell
$env:STRIPE_PUBLISHABLE_KEY="pk_test_51AbCdEf..."
$env:STRIPE_SECRET_KEY="sk_test_51AbCdEf..."
$env:STRIPE_WEBHOOK_SECRET="whsec_123456..."
```

**Linux/macOS (Bash):**
```bash
export STRIPE_PUBLISHABLE_KEY="pk_test_51AbCdEf..."
export STRIPE_SECRET_KEY="sk_test_51AbCdEf..."
export STRIPE_WEBHOOK_SECRET="whsec_123456..."
```

## 🔐 Backend Configuration (appsettings.json)

The backend reads Stripe configuration from `appsettings.json`. The structure is:

```json
{
  "Payment": {
    "Stripe": {
      "PublishableKey": "",
      "SecretKey": "USE_AZURE_KEY_VAULT",
      "WebhookSecret": "USE_AZURE_KEY_VAULT",
      "ApiVersion": "2024-11-20.acacia",
      "Currency": "usd",
      "EnableTestMode": true
    }
  }
}
```

**Important Notes:**
- DO NOT put actual secret keys in `appsettings.json`
- Use environment variables, user secrets, or Azure Key Vault
- The configuration system will automatically override values from environment variables

### Reading Configuration in Code

```csharp
// In Startup/Program.cs
builder.Services.Configure<StripeSettings>(
    builder.Configuration.GetSection("Payment:Stripe"));

// In a controller or service
public class PaymentService
{
    private readonly StripeSettings _stripeSettings;
    
    public PaymentService(IOptions<StripeSettings> stripeSettings)
    {
        _stripeSettings = stripeSettings.Value;
    }
    
    public void InitializeStripe()
    {
        // Use secret key server-side only
        StripeConfiguration.ApiKey = _stripeSettings.SecretKey;
    }
    
    public string GetPublishableKey()
    {
        // Safe to return to frontend
        return _stripeSettings.PublishableKey;
    }
}
```

## 🌐 Frontend Configuration

The frontend needs access to the **publishable key only** (NOT the secret key).

### Option 1: Environment Variables in Vite

1. Create `.env.local` in the `frontend/` directory:
   ```bash
   VITE_STRIPE_PUBLISHABLE_KEY=pk_test_51AbCdEf...
   ```

2. Access in React code:
   ```typescript
   const stripePublishableKey = import.meta.env.VITE_STRIPE_PUBLISHABLE_KEY;
   ```

### Option 2: API Endpoint (Recommended for Production)

Fetch the publishable key from a backend API endpoint:

```typescript
// Fetch Stripe config from backend
const response = await fetch('/api/stripe/config');
const { publishableKey } = await response.json();

// Initialize Stripe
const stripe = await loadStripe(publishableKey);
```

This approach is more secure as it keeps configuration centralized.

## 🚀 GitHub Actions / CI/CD Setup

### Step 1: Add GitHub Secrets

1. Go to your GitHub repository
2. Navigate to **Settings** → **Secrets and variables** → **Actions**
3. Click **New repository secret**
4. Add the following secrets:

   - Name: `STRIPE_PUBLISHABLE_KEY`
     - Value: `pk_test_51AbCdEf...` (or `pk_live_*` for production)
   
   - Name: `STRIPE_SECRET_KEY`
     - Value: `sk_test_51AbCdEf...` (or `sk_live_*` for production)
   
   - Name: `STRIPE_WEBHOOK_SECRET`
     - Value: `whsec_123456...`

### Step 2: Update GitHub Workflow

Modify `.github/workflows/deploy.yml` to pass secrets as environment variables:

```yaml
deploy:
  name: Deploy to Azure
  runs-on: ubuntu-latest
  needs: build
  environment:
    name: 'Production'
  
  steps:
    - name: Deploy to Azure Web App
      uses: azure/webapps-deploy@v3
      with:
        app-name: mosaic-saas
        publish-profile: ${{ secrets.AZURE_WEBAPP_PUBLISH_PROFILE_MOSAIC }}
        package: .
      env:
        STRIPE_PUBLISHABLE_KEY: ${{ secrets.STRIPE_PUBLISHABLE_KEY }}
        STRIPE_SECRET_KEY: ${{ secrets.STRIPE_SECRET_KEY }}
        STRIPE_WEBHOOK_SECRET: ${{ secrets.STRIPE_WEBHOOK_SECRET }}
```

## ☁️ Azure Web App Configuration

### Option 1: Application Settings in Azure Portal

1. Go to [Azure Portal](https://portal.azure.com/)
2. Navigate to your Web App (e.g., `mosaic-saas`)
3. Go to **Settings** → **Configuration** → **Application settings**
4. Click **New application setting** and add:

   - Name: `Payment__Stripe__PublishableKey`
     - Value: `pk_test_51AbCdEf...` (or `pk_live_*`)
   
   - Name: `Payment__Stripe__SecretKey`
     - Value: `sk_test_51AbCdEf...` (or `sk_live_*`)
   
   - Name: `Payment__Stripe__WebhookSecret`
     - Value: `whsec_123456...`

5. Click **Save** and restart the app

**Note**: In Azure App Settings, use double underscores `__` instead of colons `:` for nested configuration.

### Option 2: Azure Key Vault (Recommended for Production)

For production environments, use Azure Key Vault for enhanced security:

#### 1. Create Azure Key Vault

```bash
az keyvault create \
  --name mosaic-keyvault \
  --resource-group orkinosai_group \
  --location uksouth
```

#### 2. Add Secrets to Key Vault

```bash
# Stripe Publishable Key
az keyvault secret set \
  --vault-name mosaic-keyvault \
  --name StripePublishableKey \
  --value "pk_live_51AbCdEf..."

# Stripe Secret Key
az keyvault secret set \
  --vault-name mosaic-keyvault \
  --name StripeSecretKey \
  --value "sk_live_51AbCdEf..."

# Stripe Webhook Secret
az keyvault secret set \
  --vault-name mosaic-keyvault \
  --name StripeWebhookSecret \
  --value "whsec_123456..."
```

#### 3. Configure Managed Identity

```bash
# Enable system-assigned managed identity
az webapp identity assign \
  --name mosaic-saas \
  --resource-group orkinosai_group

# Get principal ID
PRINCIPAL_ID=$(az webapp identity show \
  --name mosaic-saas \
  --resource-group orkinosai_group \
  --query principalId -o tsv)

# Grant access to Key Vault
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

Update `Program.cs`:

```csharp
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

## 🔒 Vercel Deployment (Alternative to Azure)

If deploying the frontend separately to Vercel:

### Step 1: Add Environment Variables

1. Go to [Vercel Dashboard](https://vercel.com/dashboard)
2. Select your project
3. Go to **Settings** → **Environment Variables**
4. Add:

   - Name: `VITE_STRIPE_PUBLISHABLE_KEY`
     - Value: `pk_test_51AbCdEf...`
     - Environment: Production

### Step 2: Backend API URL

Also configure the backend API URL:

```
VITE_API_BASE_URL=https://mosaic-saas.azurewebsites.net/api
```

## ⚠️ Security Best Practices

### ✅ DO

1. **Use test keys during development**
   - Test keys start with `pk_test_` and `sk_test_`
   - They won't process real payments

2. **Keep secret keys secret**
   - Never commit secret keys to Git
   - Never log secret keys
   - Never expose in frontend code

3. **Use environment-specific keys**
   - Development: Test keys
   - Staging: Test keys
   - Production: Live keys

4. **Rotate keys regularly**
   - Stripe allows creating multiple secret keys
   - Rotate keys if compromised

5. **Use webhook secrets**
   - Verify webhook signatures to ensure authenticity

6. **Restrict key permissions**
   - Use restricted API keys if possible
   - Limit scope to necessary operations

### ❌ DON'T

1. **Never hardcode keys in source code**
   ```typescript
   // ❌ BAD - Never do this!
   const stripe = loadStripe('pk_live_HARDCODED_KEY');
   ```

2. **Never commit .env files**
   - Ensure `.env` is in `.gitignore`

3. **Never expose secret keys to frontend**
   ```typescript
   // ❌ BAD - Secret key in frontend!
   const secretKey = 'sk_test_SECRET';
   ```

4. **Never log sensitive data**
   ```csharp
   // ❌ BAD
   _logger.LogInformation($"Stripe key: {secretKey}");
   ```

5. **Never share keys via email/chat**
   - Use secure secret management tools

## 🧪 Testing Stripe Integration

### Test with Stripe CLI

1. Install Stripe CLI:
   ```bash
   # macOS
   brew install stripe/stripe-cli/stripe
   
   # Windows (via Scoop)
   scoop bucket add stripe https://github.com/stripe/scoop-stripe-cli.git
   scoop install stripe
   ```

2. Login to Stripe:
   ```bash
   stripe login
   ```

3. Forward webhooks to local development:
   ```bash
   stripe listen --forward-to localhost:5000/api/webhooks/stripe
   ```

4. Trigger test webhook:
   ```bash
   stripe trigger checkout.session.completed
   ```

### Test Cards

Use these test card numbers:

| Card Number         | Description                    |
|---------------------|--------------------------------|
| 4242 4242 4242 4242 | Successful payment             |
| 4000 0025 0000 3155 | Requires authentication (3DS)  |
| 4000 0000 0000 9995 | Declined (insufficient funds)  |
| 4000 0000 0000 0002 | Declined (generic decline)     |

- Use any future expiration date (e.g., `12/34`)
- Use any 3-digit CVC
- Use any billing postal code

## 📚 Additional Resources

- [Stripe API Documentation](https://stripe.com/docs/api)
- [Stripe Testing Guide](https://stripe.com/docs/testing)
- [Stripe Webhooks Guide](https://stripe.com/docs/webhooks)
- [Stripe Security Best Practices](https://stripe.com/docs/security)
- [MOSAIC Application Settings Guide](./appsettings.md)
- [Azure Key Vault Documentation](https://docs.microsoft.com/azure/key-vault/)

## 🆘 Troubleshooting

### Issue: "Invalid API Key"

**Solution**: Check that:
1. You're using the correct key format (test vs. live)
2. The key hasn't been revoked in Stripe Dashboard
3. Environment variables are set correctly

### Issue: "Webhook signature verification failed"

**Solution**: 
1. Verify webhook secret is correct
2. Check that payload hasn't been modified
3. Ensure webhook endpoint URL is correct

### Issue: "Keys not loading in application"

**Solution**:
1. Restart the application after setting environment variables
2. Check configuration precedence (environment vars override appsettings.json)
3. Verify Key Vault access if using Azure Key Vault

---

**Last Updated:** December 2024  
**Version:** 1.0  
**Maintained by:** Orkinosai Team
