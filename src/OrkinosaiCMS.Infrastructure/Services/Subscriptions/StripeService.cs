using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OrkinosaiCMS.Core.Entities.Subscriptions;
using OrkinosaiCMS.Core.Interfaces.Services;
using Stripe;
using Stripe.Checkout;

namespace OrkinosaiCMS.Infrastructure.Services.Subscriptions;

/// <summary>
/// Service implementation for Stripe API interactions
/// </summary>
public class StripeService : IStripeService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<StripeService> _logger;
    private readonly ICustomerService _customerService;
    private readonly ISubscriptionService _subscriptionService;
    private readonly string _secretKey;
    private readonly string _webhookSecret;

    public StripeService(
        IConfiguration configuration,
        ILogger<StripeService> logger,
        ICustomerService customerService,
        ISubscriptionService subscriptionService)
    {
        _configuration = configuration;
        _logger = logger;
        _customerService = customerService;
        _subscriptionService = subscriptionService;

        _secretKey = _configuration["Payment:Stripe:SecretKey"] ?? string.Empty;
        _webhookSecret = _configuration["Payment:Stripe:WebhookSecret"] ?? string.Empty;

        // Only configure Stripe if SecretKey is provided
        // This allows the service to be registered without breaking startup when Stripe is not configured
        if (!string.IsNullOrWhiteSpace(_secretKey))
        {
            StripeConfiguration.ApiKey = _secretKey;
            _logger.LogInformation("Stripe payment service initialized");
        }
        else
        {
            _logger.LogWarning("Stripe SecretKey not configured. Payment functionality will be disabled. Set Payment__Stripe__SecretKey environment variable to enable Stripe integration.");
        }
    }

    public async Task<string> CreateCustomerAsync(string email, string name, int userId)
    {
        if (string.IsNullOrWhiteSpace(_secretKey))
        {
            throw new InvalidOperationException("Stripe is not configured. Set Payment__Stripe__SecretKey to enable payment functionality.");
        }

        try
        {
            var options = new CustomerCreateOptions
            {
                Email = email,
                Name = name,
                Metadata = new Dictionary<string, string>
                {
                    { "user_id", userId.ToString() }
                }
            };

            var stripeCustomerService = new Stripe.CustomerService();
            var customer = await stripeCustomerService.CreateAsync(options);

            _logger.LogInformation("Created Stripe customer {CustomerId} for user {UserId}", customer.Id, userId);
            return customer.Id;
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Failed to create Stripe customer for user {UserId}", userId);
            throw;
        }
    }

    public async Task<Core.Entities.Subscriptions.Customer?> GetCustomerAsync(string stripeCustomerId)
    {
        try
        {
            return await _customerService.GetByStripeCustomerIdAsync(stripeCustomerId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get customer {CustomerId}", stripeCustomerId);
            return null;
        }
    }

    public async Task<Core.Entities.Subscriptions.Subscription> CreateSubscriptionAsync(
        string customerId, 
        string priceId, 
        SubscriptionTier tier, 
        BillingInterval interval)
    {
        if (string.IsNullOrWhiteSpace(_secretKey))
        {
            throw new InvalidOperationException("Stripe is not configured. Set Payment__Stripe__SecretKey to enable payment functionality.");
        }

        try
        {
            var options = new SubscriptionCreateOptions
            {
                Customer = customerId,
                Items = new List<SubscriptionItemOptions>
                {
                    new SubscriptionItemOptions { Price = priceId }
                },
                PaymentBehavior = "default_incomplete",
                PaymentSettings = new SubscriptionPaymentSettingsOptions
                {
                    SaveDefaultPaymentMethod = "on_subscription"
                },
                Expand = new List<string> { "latest_invoice.payment_intent" }
            };

            var stripeSubscriptionService = new Stripe.SubscriptionService();
            var subscription = await stripeSubscriptionService.CreateAsync(options);

            var dbCustomer = await _customerService.GetByStripeCustomerIdAsync(customerId);
            if (dbCustomer == null)
                throw new InvalidOperationException($"Customer not found: {customerId}");

            var dbSubscription = new Core.Entities.Subscriptions.Subscription
            {
                CustomerId = dbCustomer.Id,
                StripeSubscriptionId = subscription.Id,
                Tier = tier,
                Status = MapStripeStatus(subscription.Status),
                BillingInterval = interval,
                PriceAmount = (int)(subscription.Items.Data[0].Price.UnitAmount ?? 0),
                Currency = subscription.Items.Data[0].Price.Currency,
                CurrentPeriodStart = subscription.CurrentPeriodStart,
                CurrentPeriodEnd = subscription.CurrentPeriodEnd,
                StripePriceId = priceId,
                CancelAtPeriodEnd = subscription.CancelAtPeriodEnd
            };

            return await _subscriptionService.CreateAsync(dbSubscription);
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Failed to create subscription for customer {CustomerId}", customerId);
            throw;
        }
    }

    public async Task<Core.Entities.Subscriptions.Subscription> UpdateSubscriptionAsync(
        string subscriptionId, 
        string newPriceId, 
        SubscriptionTier newTier)
    {
        if (string.IsNullOrWhiteSpace(_secretKey))
        {
            throw new InvalidOperationException("Stripe is not configured. Set Payment__Stripe__SecretKey to enable payment functionality.");
        }

        try
        {
            var stripeSubscriptionService = new Stripe.SubscriptionService();
            var subscription = await stripeSubscriptionService.GetAsync(subscriptionId);

            var options = new SubscriptionUpdateOptions
            {
                Items = new List<SubscriptionItemOptions>
                {
                    new SubscriptionItemOptions
                    {
                        Id = subscription.Items.Data[0].Id,
                        Price = newPriceId
                    }
                },
                ProrationBehavior = "create_prorations"
            };

            var updatedSubscription = await stripeSubscriptionService.UpdateAsync(subscriptionId, options);

            var dbCustomer = await _customerService.GetByStripeCustomerIdAsync(subscription.CustomerId);
            if (dbCustomer == null)
                throw new InvalidOperationException($"Customer not found: {subscription.CustomerId}");

            var dbSubscription = await _subscriptionService.GetActiveSubscriptionByCustomerIdAsync(dbCustomer.Id);
            if (dbSubscription != null)
            {
                dbSubscription.Tier = newTier;
                dbSubscription.StripePriceId = newPriceId;
                dbSubscription.PriceAmount = (int)(updatedSubscription.Items.Data[0].Price.UnitAmount ?? 0);
                dbSubscription.Status = MapStripeStatus(updatedSubscription.Status);
                return await _subscriptionService.UpdateAsync(dbSubscription);
            }

            throw new InvalidOperationException("No active subscription found");
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Failed to update subscription {SubscriptionId}", subscriptionId);
            throw;
        }
    }

    public async Task<bool> CancelSubscriptionAsync(string subscriptionId, bool cancelAtPeriodEnd = true)
    {
        if (string.IsNullOrWhiteSpace(_secretKey))
        {
            throw new InvalidOperationException("Stripe is not configured. Set Payment__Stripe__SecretKey to enable payment functionality.");
        }

        try
        {
            var stripeSubscriptionService = new Stripe.SubscriptionService();
            
            if (cancelAtPeriodEnd)
            {
                var options = new SubscriptionUpdateOptions
                {
                    CancelAtPeriodEnd = true
                };
                await stripeSubscriptionService.UpdateAsync(subscriptionId, options);
            }
            else
            {
                await stripeSubscriptionService.CancelAsync(subscriptionId);
            }

            _logger.LogInformation("Canceled subscription {SubscriptionId}, cancelAtPeriodEnd: {CancelAtPeriodEnd}", 
                subscriptionId, cancelAtPeriodEnd);
            return true;
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Failed to cancel subscription {SubscriptionId}", subscriptionId);
            return false;
        }
    }

    public async Task<Core.Entities.Subscriptions.Subscription?> GetSubscriptionAsync(string subscriptionId)
    {
        if (string.IsNullOrWhiteSpace(_secretKey))
        {
            _logger.LogWarning("Stripe is not configured. Cannot get subscription.");
            return null;
        }

        try
        {
            var stripeSubscriptionService = new Stripe.SubscriptionService();
            var subscription = await stripeSubscriptionService.GetAsync(subscriptionId);

            var dbCustomer = await _customerService.GetByStripeCustomerIdAsync(subscription.CustomerId);
            if (dbCustomer == null)
                return null;

            return await _subscriptionService.GetActiveSubscriptionByCustomerIdAsync(dbCustomer.Id);
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Failed to get subscription {SubscriptionId}", subscriptionId);
            return null;
        }
    }

    public async Task<string> CreateCheckoutSessionAsync(
        string customerId, 
        string priceId, 
        string successUrl, 
        string cancelUrl)
    {
        if (string.IsNullOrWhiteSpace(_secretKey))
        {
            throw new InvalidOperationException("Stripe is not configured. Set Payment__Stripe__SecretKey to enable payment functionality.");
        }

        try
        {
            var options = new SessionCreateOptions
            {
                Customer = customerId,
                PaymentMethodTypes = new List<string> { "card" },
                LineItems = new List<SessionLineItemOptions>
                {
                    new SessionLineItemOptions
                    {
                        Price = priceId,
                        Quantity = 1
                    }
                },
                Mode = "subscription",
                SuccessUrl = successUrl,
                CancelUrl = cancelUrl
            };

            var service = new SessionService();
            var session = await service.CreateAsync(options);

            _logger.LogInformation("Created checkout session {SessionId} for customer {CustomerId}", 
                session.Id, customerId);
            return session.Url;
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Failed to create checkout session for customer {CustomerId}", customerId);
            throw;
        }
    }

    public async Task<string> CreateBillingPortalSessionAsync(string customerId, string returnUrl)
    {
        if (string.IsNullOrWhiteSpace(_secretKey))
        {
            throw new InvalidOperationException("Stripe is not configured. Set Payment__Stripe__SecretKey to enable payment functionality.");
        }

        try
        {
            var options = new Stripe.BillingPortal.SessionCreateOptions
            {
                Customer = customerId,
                ReturnUrl = returnUrl
            };

            var service = new Stripe.BillingPortal.SessionService();
            var session = await service.CreateAsync(options);

            _logger.LogInformation("Created billing portal session for customer {CustomerId}", customerId);
            return session.Url;
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Failed to create billing portal session for customer {CustomerId}", customerId);
            throw;
        }
    }

    public string GetPriceId(SubscriptionTier tier, BillingInterval interval)
    {
        if (string.IsNullOrWhiteSpace(_secretKey))
        {
            throw new InvalidOperationException("Stripe is not configured. Set Payment__Stripe__SecretKey to enable payment functionality.");
        }

        var key = $"Payment:Stripe:PriceIds:{tier}_{interval}";
        var priceId = _configuration[key];
        
        if (string.IsNullOrEmpty(priceId))
        {
            var message = $"Price ID not configured for {tier}_{interval}. Please configure {key} in appsettings.json or environment variables.";
            _logger.LogError(message);
            throw new InvalidOperationException(message);
        }

        return priceId;
    }

    public bool VerifyWebhookSignature(string payload, string signature)
    {
        if (string.IsNullOrWhiteSpace(_secretKey) || string.IsNullOrWhiteSpace(_webhookSecret))
        {
            _logger.LogWarning("Stripe is not configured or WebhookSecret is missing. Cannot verify webhook signature.");
            return false;
        }

        try
        {
            var stripeEvent = EventUtility.ConstructEvent(
                payload,
                signature,
                _webhookSecret
            );
            return true;
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Failed to verify webhook signature");
            return false;
        }
    }

    public async Task ProcessWebhookEventAsync(string eventType, string eventJson)
    {
        if (string.IsNullOrWhiteSpace(_secretKey))
        {
            _logger.LogWarning("Stripe is not configured. Cannot process webhook event.");
            return;
        }

        try
        {
            _logger.LogInformation("Processing webhook event: {EventType}", eventType);

            switch (eventType)
            {
                case "customer.subscription.created":
                case "customer.subscription.updated":
                    await HandleSubscriptionUpdatedAsync(eventJson);
                    break;

                case "customer.subscription.deleted":
                    await HandleSubscriptionDeletedAsync(eventJson);
                    break;

                case "invoice.paid":
                    await HandleInvoicePaidAsync(eventJson);
                    break;

                case "invoice.payment_failed":
                    await HandleInvoicePaymentFailedAsync(eventJson);
                    break;

                default:
                    _logger.LogInformation("Unhandled webhook event type: {EventType}", eventType);
                    break;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing webhook event {EventType}", eventType);
            throw;
        }
    }

    private async Task HandleSubscriptionUpdatedAsync(string eventJson)
    {
        var stripeEvent = Newtonsoft.Json.JsonConvert.DeserializeObject<Event>(eventJson);
        var subscription = stripeEvent?.Data.Object as Stripe.Subscription;

        if (subscription == null)
            return;

        var dbCustomer = await _customerService.GetByStripeCustomerIdAsync(subscription.CustomerId);
        if (dbCustomer == null)
            return;

        var dbSubscription = await _subscriptionService.GetActiveSubscriptionByCustomerIdAsync(dbCustomer.Id);
        if (dbSubscription != null)
        {
            dbSubscription.Status = MapStripeStatus(subscription.Status);
            dbSubscription.CurrentPeriodStart = subscription.CurrentPeriodStart;
            dbSubscription.CurrentPeriodEnd = subscription.CurrentPeriodEnd;
            dbSubscription.CancelAtPeriodEnd = subscription.CancelAtPeriodEnd;
            await _subscriptionService.UpdateAsync(dbSubscription);
        }
    }

    private async Task HandleSubscriptionDeletedAsync(string eventJson)
    {
        var stripeEvent = Newtonsoft.Json.JsonConvert.DeserializeObject<Event>(eventJson);
        var subscription = stripeEvent?.Data.Object as Stripe.Subscription;

        if (subscription == null)
            return;

        var dbCustomer = await _customerService.GetByStripeCustomerIdAsync(subscription.CustomerId);
        if (dbCustomer == null)
            return;

        var dbSubscription = await _subscriptionService.GetActiveSubscriptionByCustomerIdAsync(dbCustomer.Id);
        if (dbSubscription != null)
        {
            dbSubscription.Status = SubscriptionStatus.Canceled;
            dbSubscription.CanceledAt = DateTime.UtcNow;
            await _subscriptionService.UpdateAsync(dbSubscription);
        }
    }

    private async Task HandleInvoicePaidAsync(string eventJson)
    {
        var stripeEvent = Newtonsoft.Json.JsonConvert.DeserializeObject<Event>(eventJson);
        var invoice = stripeEvent?.Data.Object as Stripe.Invoice;

        if (invoice == null)
            return;

        _logger.LogInformation("Invoice {InvoiceId} paid for customer {CustomerId}", 
            invoice.Id, invoice.CustomerId);
        
        // TODO: Store invoice in database
        await Task.CompletedTask;
    }

    private async Task HandleInvoicePaymentFailedAsync(string eventJson)
    {
        var stripeEvent = Newtonsoft.Json.JsonConvert.DeserializeObject<Event>(eventJson);
        var invoice = stripeEvent?.Data.Object as Stripe.Invoice;

        if (invoice == null)
            return;

        _logger.LogWarning("Invoice payment failed for customer {CustomerId}: {InvoiceId}", 
            invoice.CustomerId, invoice.Id);
        
        // TODO: Notify user about failed payment
        await Task.CompletedTask;
    }

    private SubscriptionStatus MapStripeStatus(string stripeStatus)
    {
        return stripeStatus switch
        {
            "trialing" => SubscriptionStatus.Trialing,
            "active" => SubscriptionStatus.Active,
            "past_due" => SubscriptionStatus.PastDue,
            "canceled" => SubscriptionStatus.Canceled,
            "unpaid" => SubscriptionStatus.Unpaid,
            "incomplete" => SubscriptionStatus.Incomplete,
            "incomplete_expired" => SubscriptionStatus.IncompleteExpired,
            _ => SubscriptionStatus.Active
        };
    }
}
