using OrkinosaiCMS.Core.Entities.Subscriptions;

namespace OrkinosaiCMS.Core.Interfaces.Services;

/// <summary>
/// Service interface for Stripe API interactions
/// </summary>
public interface IStripeService
{
    /// <summary>
    /// Create a Stripe customer
    /// </summary>
    Task<string> CreateCustomerAsync(string email, string name, int userId);

    /// <summary>
    /// Get a Stripe customer
    /// </summary>
    Task<Customer?> GetCustomerAsync(string stripeCustomerId);

    /// <summary>
    /// Create a subscription in Stripe
    /// </summary>
    Task<Subscription> CreateSubscriptionAsync(string customerId, string priceId, SubscriptionTier tier, BillingInterval interval);

    /// <summary>
    /// Update a subscription in Stripe
    /// </summary>
    Task<Subscription> UpdateSubscriptionAsync(string subscriptionId, string newPriceId, SubscriptionTier newTier);

    /// <summary>
    /// Cancel a subscription in Stripe
    /// </summary>
    Task<bool> CancelSubscriptionAsync(string subscriptionId, bool cancelAtPeriodEnd = true);

    /// <summary>
    /// Get subscription from Stripe
    /// </summary>
    Task<Subscription?> GetSubscriptionAsync(string subscriptionId);

    /// <summary>
    /// Create a checkout session for new subscription
    /// </summary>
    Task<string> CreateCheckoutSessionAsync(string customerId, string priceId, string successUrl, string cancelUrl);

    /// <summary>
    /// Create a billing portal session for managing subscription
    /// </summary>
    Task<string> CreateBillingPortalSessionAsync(string customerId, string returnUrl);

    /// <summary>
    /// Get price ID for a tier and billing interval
    /// </summary>
    string GetPriceId(SubscriptionTier tier, BillingInterval interval);

    /// <summary>
    /// Verify webhook signature
    /// </summary>
    bool VerifyWebhookSignature(string payload, string signature);

    /// <summary>
    /// Process webhook event
    /// </summary>
    Task ProcessWebhookEventAsync(string eventType, string eventJson);
}
