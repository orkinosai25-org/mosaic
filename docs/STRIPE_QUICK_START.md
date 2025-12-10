# Stripe Integration Quick Start

Get started with Stripe payment integration in 5 minutes!

## 🚀 Quick Setup for Local Development

### 1. Get Your Stripe Keys

Visit [Stripe Dashboard](https://dashboard.stripe.com/apikeys) and copy:
- **Publishable Key** (starts with `pk_test_`)
- **Secret Key** (starts with `sk_test_`)

### 2. Configure Environment Variables

```bash
# Copy the example file
cp .env.example .env

# Edit .env and add your keys
STRIPE_PUBLISHABLE_KEY=pk_test_YOUR_KEY_HERE
STRIPE_SECRET_KEY=sk_test_YOUR_KEY_HERE
```

### 3. Start Development

```bash
# Backend (Terminal 1)
cd src/OrkinosaiCMS.Web
dotnet run

# Frontend (Terminal 2)
cd frontend
npm run dev
```

## 🔐 Key Security Rules

✅ **Safe to expose:**
- `STRIPE_PUBLISHABLE_KEY` (frontend/client-side)

❌ **Keep secret:**
- `STRIPE_SECRET_KEY` (backend/server-side only)
- `STRIPE_WEBHOOK_SECRET` (backend/server-side only)

## 🧪 Test Cards

Use these cards for testing:

| Card Number | Result |
|-------------|--------|
| `4242 4242 4242 4242` | Success |
| `4000 0025 0000 3155` | Requires 3D Secure |
| `4000 0000 0000 9995` | Declined |

- Any future expiration date (e.g., `12/34`)
- Any 3-digit CVC
- Any billing postal code

## 📦 For Production Deployment

### GitHub Secrets

Add to **Settings** → **Secrets and variables** → **Actions**:
- `STRIPE_PUBLISHABLE_KEY`
- `STRIPE_SECRET_KEY`
- `STRIPE_WEBHOOK_SECRET`

### Azure Web App

Add to **Configuration** → **Application settings**:
- `Payment__Stripe__PublishableKey`
- `Payment__Stripe__SecretKey`
- `Payment__Stripe__WebhookSecret`

### Vercel (Frontend Only)

Add to **Settings** → **Environment Variables**:
- `VITE_STRIPE_PUBLISHABLE_KEY` (publishable key only!)

## 📚 Detailed Documentation

For complete instructions, see:
- [Stripe Setup Guide](./STRIPE_SETUP.md) - Complete setup for all environments
- [GitHub Secrets Setup](./GITHUB_SECRETS_SETUP.md) - CI/CD configuration
- [Application Settings](./appsettings.md) - Configuration reference

## 🆘 Common Issues

**"Invalid API Key"**
- Check you're using test keys (not live keys)
- Verify keys are copied correctly (no spaces)

**"Keys not loading"**
- Restart application after setting `.env`
- Check `.env` is in the repository root

**"Webhook signature failed"**
- Verify webhook secret is correct
- Check webhook URL in Stripe Dashboard

## 💡 Pro Tips

1. **Use test mode** during development
   - Test keys start with `pk_test_` and `sk_test_`
   - No real money involved

2. **Never commit secrets** to Git
   - `.env` is already in `.gitignore`
   - Use `.env.example` as template

3. **Different keys per environment**
   - Development: Test keys
   - Production: Live keys

4. **Webhook testing with Stripe CLI**
   ```bash
   stripe listen --forward-to localhost:5000/api/webhooks/stripe
   ```

## 🔗 Quick Links

- [Stripe Dashboard](https://dashboard.stripe.com/)
- [Stripe API Docs](https://stripe.com/docs/api)
- [Stripe Testing Guide](https://stripe.com/docs/testing)
- [Stripe Webhooks](https://stripe.com/docs/webhooks)

---

**Need help?** See the [full Stripe Setup Guide](./STRIPE_SETUP.md)
