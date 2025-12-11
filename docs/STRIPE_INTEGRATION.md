# Stripe Integration Documentation

## Overview

This document provides comprehensive details on the Stripe subscription/payment integration in the MOSAIC Conversational CMS platform. The integration enables end-to-end subscription management for Free, Starter, Pro, and Business tiers.

## Table of Contents

1. [Architecture](#architecture)
2. [Subscription Tiers](#subscription-tiers)
3. [Backend Implementation](#backend-implementation)
4. [API Endpoints](#api-endpoints)
5. [Frontend Integration](#frontend-integration)
6. [Webhook Configuration](#webhook-configuration)
7. [Configuration](#configuration)
8. [Testing](#testing)
9. [Deployment](#deployment)

## Architecture

### System Components

```
┌─────────────────────────────────────────────────────────────┐
│                        Frontend (React)                      │
│  ┌──────────────────────────────────────────────────────┐  │
│  │  BillingPage Component                               │  │
│  │  - Display current subscription                      │  │
│  │  - Plan selector                                     │  │
│  │  - Upgrade/downgrade functionality                   │  │
│  │  - Billing portal access                            │  │
│  └──────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────┘
                            │
                            ▼
┌─────────────────────────────────────────────────────────────┐
│                    Backend API (.NET 10)                     │
│  ┌──────────────────────────────────────────────────────┐  │
│  │  SubscriptionController                              │  │
│  │  - GET /api/subscription/current                     │  │
│  │  - GET /api/subscription/plans                       │  │
│  │  - POST /api/subscription/checkout                   │  │
│  │  - PUT /api/subscription/update                      │  │
│  │  - DELETE /api/subscription/cancel                   │  │
│  │  - POST /api/subscription/billing-portal             │  │
│  └──────────────────────────────────────────────────────┘  │
│  ┌──────────────────────────────────────────────────────┐  │
│  │  WebhookController                                   │  │
│  │  - POST /api/webhooks/stripe                        │  │
│  └──────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────┘
                            │
                            ▼
┌─────────────────────────────────────────────────────────────┐
│                        Stripe API                            │
│  - Customer Management                                       │
│  - Subscription Management                                   │
│  - Checkout Sessions                                         │
│  - Billing Portal                                           │
│  - Webhooks                                                 │
└─────────────────────────────────────────────────────────────┘
```

### Database Schema

The integration adds the following entities:

- **Customer**: Stores Stripe customer information
- **Subscription**: Tracks active subscriptions
- **Invoice**: Records invoice history
- **PaymentMethod**: Stores payment method details

### Entity Relationships

```
User (1) ──────── (1) Customer
                        │
                        │ (1)
                        ▼
                    Subscription (N)
                        │
                        │ (1)
                        ▼
                    Invoice (N)
```

## Subscription Tiers

### Free Tier
- **Price**: $0/month
- **Websites**: 1
- **Storage**: 500 MB
- **Bandwidth**: 10 GB/month
- **Custom Domains**: 0
- **Features**: Basic themes, Platform ads, MOSAIC branding

### Starter Tier
- **Price**: $12/month or $120/year (17% savings)
- **Websites**: 3
- **Storage**: 5 GB
- **Bandwidth**: 25 GB/month
- **Custom Domains**: 1
- **Features**: 10 themes, No ads, No branding

### Pro Tier
- **Price**: $35/month or $350/year (17% savings)
- **Websites**: 10
- **Storage**: 25 GB
- **Bandwidth**: 100 GB/month
- **Custom Domains**: 10
- **Features**: 25 themes, Premium themes, Custom CSS, Analytics dashboard

### Business Tier
- **Price**: $250/month or $2,500/year (17% savings)
- **Websites**: 50
- **Storage**: 100 GB
- **Bandwidth**: 500 GB/month
- **Custom Domains**: 50
- **Features**: All themes, AI-powered features, Priority support, White-label option

## Backend Implementation

### Services

#### IStripeService
Handles all Stripe API interactions:
- `CreateCustomerAsync`: Creates a Stripe customer
- `CreateSubscriptionAsync`: Creates a new subscription
- `UpdateSubscriptionAsync`: Upgrades/downgrades subscription
- `CancelSubscriptionAsync`: Cancels a subscription
- `CreateCheckoutSessionAsync`: Creates Stripe Checkout session
- `CreateBillingPortalSessionAsync`: Creates billing portal session
- `VerifyWebhookSignature`: Verifies webhook signatures
- `ProcessWebhookEventAsync`: Processes webhook events

#### ISubscriptionService
Manages subscription logic and tier limits:
- `GetActiveSubscriptionByUserIdAsync`: Gets user's active subscription
- `CreateAsync`: Creates new subscription record
- `UpdateAsync`: Updates subscription record
- `CancelAsync`: Cancels subscription
- `HasReachedWebsiteLimitAsync`: Checks website limit
- `HasReachedStorageLimitAsync`: Checks storage limit
- `GetTierLimits`: Returns tier limits configuration

#### ICustomerService
Manages customer records:
- `GetByUserIdAsync`: Gets customer by user ID
- `GetByStripeCustomerIdAsync`: Gets customer by Stripe ID
- `CreateAsync`: Creates new customer record
- `UpdateAsync`: Updates customer record

### Entities

All subscription entities inherit from `BaseEntity` which provides:
- `Id`: Unique identifier
- `CreatedOn`: Creation timestamp
- `CreatedBy`: Creator identifier
- `ModifiedOn`: Last modification timestamp
- `ModifiedBy`: Last modifier identifier
- `IsDeleted`: Soft delete flag

## API Endpoints

### GET /api/subscription/current
Returns current subscription for a user.

**Query Parameters:**
- `userEmail` (required): User's email address

**Response:**
```json
{
  "id": 1,
  "tier": "Pro",
  "status": "Active",
  "billingInterval": "Monthly",
  "priceAmount": 35.00,
  "currency": "usd",
  "currentPeriodStart": "2024-01-01T00:00:00Z",
  "currentPeriodEnd": "2024-02-01T00:00:00Z",
  "cancelAtPeriodEnd": false,
  "limits": {
    "maxWebsites": 10,
    "maxStorageBytes": 26843545600,
    "maxBandwidthBytes": 107374182400,
    "maxCustomDomains": 10,
    "hasAds": false,
    "hasBranding": false
  }
}
```

### GET /api/subscription/plans
Returns all available subscription plans.

**Response:**
```json
[
  {
    "tier": "Free",
    "name": "Free",
    "description": "Perfect for trying out Mosaic",
    "monthlyPrice": 0,
    "yearlyPrice": 0,
    "monthlyPriceId": "",
    "yearlyPriceId": "",
    "limits": { ... }
  },
  ...
]
```

### POST /api/subscription/checkout
Creates a Stripe Checkout session for new subscription.

**Query Parameters:**
- `userEmail` (required): User's email address

**Request Body:**
```json
{
  "tier": "Pro",
  "billingInterval": "Monthly",
  "successUrl": "https://example.com/success",
  "cancelUrl": "https://example.com/cancel"
}
```

**Response:**
```json
{
  "sessionUrl": "https://checkout.stripe.com/c/pay/cs_test_..."
}
```

### PUT /api/subscription/update
Updates subscription (upgrade/downgrade).

**Query Parameters:**
- `userEmail` (required): User's email address

**Request Body:**
```json
{
  "newTier": "Business",
  "billingInterval": "Yearly"
}
```

**Response:**
Returns updated subscription object (same format as GET /current).

### DELETE /api/subscription/cancel
Cancels a subscription.

**Query Parameters:**
- `userEmail` (required): User's email address
- `cancelAtPeriodEnd` (optional, default: true): Whether to cancel at period end

**Response:**
```json
{
  "message": "Subscription canceled successfully"
}
```

### POST /api/subscription/billing-portal
Creates a Stripe Billing Portal session.

**Query Parameters:**
- `userEmail` (required): User's email address

**Request Body:**
```json
{
  "returnUrl": "https://example.com/billing"
}
```

**Response:**
```json
{
  "portalUrl": "https://billing.stripe.com/session/..."
}
```

### POST /api/webhooks/stripe
Handles Stripe webhook events.

**Headers:**
- `Stripe-Signature` (required): Webhook signature for verification

**Supported Events:**
- `customer.subscription.created`
- `customer.subscription.updated`
- `customer.subscription.deleted`
- `invoice.paid`
- `invoice.payment_failed`

## Frontend Integration

### Components

#### BillingPage
Main component for subscription management.

**Features:**
- Displays current subscription details
- Shows all available plans
- Upgrade/downgrade buttons
- Access to Stripe Billing Portal
- Real-time subscription status

**Location:** `frontend/src/pages/BillingPage.tsx`

#### subscriptionService
Service for making API calls to subscription endpoints.

**Location:** `frontend/src/services/subscriptionService.ts`

**Methods:**
- `getCurrentSubscription(userEmail)`
- `getPlans()`
- `createCheckoutSession(userEmail, tier, interval, successUrl, cancelUrl)`
- `updateSubscription(userEmail, newTier, interval)`
- `cancelSubscription(userEmail, cancelAtPeriodEnd)`
- `createBillingPortalSession(userEmail, returnUrl)`

### Usage Example

```typescript
import { subscriptionService } from '../services/subscriptionService';

// Get current subscription
const subscription = await subscriptionService.getCurrentSubscription('user@example.com');

// Upgrade to Pro plan
const sessionUrl = await subscriptionService.createCheckoutSession(
  'user@example.com',
  'Pro',
  'Monthly',
  'https://example.com/success',
  'https://example.com/cancel'
);
window.location.href = sessionUrl;
```

## Webhook Configuration

### Setting Up Webhooks in Stripe

1. Go to Stripe Dashboard → Developers → Webhooks
2. Click "Add endpoint"
3. Enter your webhook URL: `https://your-domain.com/api/webhooks/stripe`
4. Select events to listen for:
   - `customer.subscription.created`
   - `customer.subscription.updated`
   - `customer.subscription.deleted`
   - `invoice.paid`
   - `invoice.payment_failed`
5. Click "Add endpoint"
6. Copy the webhook signing secret

### Webhook Signature Verification

All webhook requests are automatically verified using the webhook signing secret. Invalid signatures are rejected with a 400 Bad Request response.

### Webhook Processing

The webhook controller processes events asynchronously:

1. **Subscription Events**: Updates local subscription status
2. **Invoice Events**: Logs invoice status and can trigger notifications
3. **Failed Payments**: Can trigger email notifications to users

## Configuration

### Environment Variables

Set these in your `.env` file or Azure App Service Configuration:

```bash
# Stripe API Keys
Payment__Stripe__PublishableKey=pk_test_...
Payment__Stripe__SecretKey=sk_test_...
Payment__Stripe__WebhookSecret=whsec_...

# Stripe Price IDs
Payment__Stripe__PriceIds__Starter_Monthly=price_...
Payment__Stripe__PriceIds__Starter_Yearly=price_...
Payment__Stripe__PriceIds__Pro_Monthly=price_...
Payment__Stripe__PriceIds__Pro_Yearly=price_...
Payment__Stripe__PriceIds__Business_Monthly=price_...
Payment__Stripe__PriceIds__Business_Yearly=price_...
```

### appsettings.json

```json
{
  "Payment": {
    "Stripe": {
      "PublishableKey": "",
      "SecretKey": "",
      "WebhookSecret": "",
      "ApiVersion": "2024-11-20.acacia",
      "Currency": "usd",
      "EnableTestMode": true,
      "PriceIds": {
        "Starter_Monthly": "",
        "Starter_Yearly": "",
        "Pro_Monthly": "",
        "Pro_Yearly": "",
        "Business_Monthly": "",
        "Business_Yearly": ""
      }
    }
  }
}
```

## Testing

### Local Testing

1. **Install Stripe CLI**: `stripe login`
2. **Forward webhooks**: `stripe listen --forward-to https://localhost:5001/api/webhooks/stripe`
3. **Trigger test events**: `stripe trigger customer.subscription.created`

### Integration Tests

Run integration tests:
```bash
cd tests/OrkinosaiCMS.Tests.Integration
dotnet test
```

### Manual Testing Checklist

- [ ] Create free tier user
- [ ] Upgrade to Starter plan (monthly)
- [ ] Upgrade to Pro plan (yearly)
- [ ] Access billing portal
- [ ] Update payment method
- [ ] Downgrade to Starter plan
- [ ] Cancel subscription
- [ ] Test webhook events
- [ ] Verify tier limits enforcement

## Deployment

### Pre-Deployment Checklist

- [ ] Create Stripe account (production)
- [ ] Configure products and prices in Stripe
- [ ] Set up webhook endpoint
- [ ] Update environment variables with production keys
- [ ] Test webhook signature verification
- [ ] Run database migrations
- [ ] Deploy to Azure Web App
- [ ] Verify all API endpoints
- [ ] Test complete subscription flow

### Azure Configuration

Set the following Application Settings in Azure Portal:

```
Payment:Stripe:PublishableKey = pk_live_...
Payment:Stripe:SecretKey = sk_live_...
Payment:Stripe:WebhookSecret = whsec_...
Payment:Stripe:PriceIds:Starter_Monthly = price_...
Payment:Stripe:PriceIds:Starter_Yearly = price_...
Payment:Stripe:PriceIds:Pro_Monthly = price_...
Payment:Stripe:PriceIds:Pro_Yearly = price_...
Payment:Stripe:PriceIds:Business_Monthly = price_...
Payment:Stripe:PriceIds:Business_Yearly = price_...
```

### Post-Deployment Verification

1. Test subscription creation
2. Verify webhook delivery
3. Check database records
4. Test billing portal access
5. Monitor application logs
6. Verify email notifications (if configured)

## Security Considerations

- API keys are never exposed to frontend
- Webhook signatures are verified for all incoming requests
- User authentication required for all subscription operations
- Stripe Customer Portal handles sensitive payment data
- PCI compliance handled by Stripe

## Support and Troubleshooting

### Common Issues

1. **Webhook signature verification fails**
   - Verify webhook secret is correct
   - Check that raw request body is used for verification

2. **Price not found**
   - Ensure price IDs are configured in appsettings.json
   - Verify price IDs exist in Stripe dashboard

3. **Subscription status not updating**
   - Check webhook endpoint is publicly accessible
   - Verify webhook events are being delivered
   - Check application logs for errors

### Logging

The integration logs important events:
- Subscription creation
- Subscription updates
- Webhook events
- Failed operations

Check logs in Azure Application Insights or local logs directory.

## Future Enhancements

- [ ] Usage-based billing
- [ ] Proration handling for mid-cycle changes
- [ ] Custom invoice generation
- [ ] Email notifications for subscription events
- [ ] Admin dashboard for subscription management
- [ ] Analytics and reporting
- [ ] Multi-currency support
- [ ] Tax calculation integration

## References

- [Stripe API Documentation](https://stripe.com/docs/api)
- [Stripe Checkout](https://stripe.com/docs/payments/checkout)
- [Stripe Billing Portal](https://stripe.com/docs/billing/subscriptions/integrating-customer-portal)
- [Stripe Webhooks](https://stripe.com/docs/webhooks)
- [Stripe .NET SDK](https://github.com/stripe/stripe-dotnet)
