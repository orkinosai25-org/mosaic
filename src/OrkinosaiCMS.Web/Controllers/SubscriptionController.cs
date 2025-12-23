using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrkinosaiCMS.Core.Entities.Subscriptions;
using OrkinosaiCMS.Core.Interfaces.Services;
using OrkinosaiCMS.Shared.DTOs.Subscriptions;

namespace OrkinosaiCMS.Web.Controllers;

/// <summary>
/// API Controller for subscription management
/// 
/// ENDPOINTS:
/// - GET  /api/subscription/current         - Get current subscription for a user
/// - GET  /api/subscription/plans           - Get available subscription plans
/// - POST /api/subscription/checkout        - Create checkout session for new subscription
/// - PUT  /api/subscription/update          - Update (upgrade/downgrade) subscription
/// - DELETE /api/subscription/cancel        - Cancel subscription
/// - POST /api/subscription/billing-portal  - Create billing portal session
/// 
/// AUTHENTICATION:
/// Currently uses userEmail query parameter for demonstration.
/// In production, implement proper authentication (JWT tokens or session-based auth).
/// 
/// STATUS CODES:
/// - 200 OK: Successful operation
/// - 400 Bad Request: Invalid input (e.g., empty email, invalid tier)
/// - 404 Not Found: User or subscription not found
/// - 500 Internal Server Error: Unexpected error
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class SubscriptionController : ControllerBase
{
    private readonly ISubscriptionService _subscriptionService;
    private readonly ICustomerService _customerService;
    private readonly IStripeService _stripeService;
    private readonly IUserService _userService;
    private readonly ILogger<SubscriptionController> _logger;

    public SubscriptionController(
        ISubscriptionService subscriptionService,
        ICustomerService customerService,
        IStripeService stripeService,
        IUserService userService,
        ILogger<SubscriptionController> logger)
    {
        _subscriptionService = subscriptionService;
        _customerService = customerService;
        _stripeService = stripeService;
        _userService = userService;
        _logger = logger;
    }

    /// <summary>
    /// Get current subscription for a user
    /// NOTE: This endpoint uses userEmail as a query parameter for demonstration purposes.
    /// In production, implement proper authentication using JWT tokens or session-based auth
    /// to identify the current user securely.
    /// 
    /// BEHAVIOR:
    /// - If user has an active subscription, returns that subscription with its tier and limits
    /// - If user has no subscription, returns Free tier (default tier for all users)
    /// - Returns 400 Bad Request if userEmail is empty
    /// - Returns 404 Not Found if user doesn't exist
    /// </summary>
    [HttpGet("current")]
    [ProducesResponseType(typeof(SubscriptionDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetCurrentSubscription([FromQuery] string userEmail)
    {
        try
        {
            if (string.IsNullOrEmpty(userEmail))
            {
                return BadRequest("User email is required");
            }

            var user = await _userService.GetByEmailAsync(userEmail);
            if (user == null)
            {
                return NotFound("User not found");
            }

            var subscription = await _subscriptionService.GetActiveSubscriptionByUserIdAsync(user.Id);
            if (subscription == null)
            {
                // Return free tier as default - all users without a paid subscription get Free tier
                // Free tier includes: 1 website, 500MB storage, with ads and branding
                var freeLimits = _subscriptionService.GetTierLimits(SubscriptionTier.Free);
                return Ok(new SubscriptionDto
                {
                    Id = 0,
                    Tier = "Free",
                    Status = "Active",
                    BillingInterval = "Monthly",
                    PriceAmount = 0,
                    Currency = "usd",
                    CurrentPeriodStart = DateTime.UtcNow,
                    CurrentPeriodEnd = DateTime.UtcNow.AddYears(100),
                    CancelAtPeriodEnd = false,
                    Limits = MapTierLimits(freeLimits)
                });
            }

            var limits = _subscriptionService.GetTierLimits(subscription.Tier);
            return Ok(new SubscriptionDto
            {
                Id = subscription.Id,
                Tier = subscription.Tier.ToString(),
                Status = subscription.Status.ToString(),
                BillingInterval = subscription.BillingInterval.ToString(),
                PriceAmount = subscription.PriceAmount / 100m, // Convert from cents
                Currency = subscription.Currency,
                CurrentPeriodStart = subscription.CurrentPeriodStart,
                CurrentPeriodEnd = subscription.CurrentPeriodEnd,
                CancelAtPeriodEnd = subscription.CancelAtPeriodEnd,
                CanceledAt = subscription.CanceledAt,
                Limits = MapTierLimits(limits)
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting current subscription for user {UserEmail}", userEmail);
            return StatusCode(500, "An error occurred while retrieving subscription");
        }
    }

    /// <summary>
    /// Get available subscription plans
    /// </summary>
    [HttpGet("plans")]
    [ProducesResponseType(typeof(IEnumerable<PlanDto>), 200)]
    public IActionResult GetPlans()
    {
        try
        {
            var plans = new List<PlanDto>
            {
                new PlanDto
                {
                    Tier = "Free",
                    Name = "Free",
                    Description = "Perfect for trying out Mosaic",
                    MonthlyPrice = 0,
                    YearlyPrice = 0,
                    MonthlyPriceId = "",
                    YearlyPriceId = "",
                    Limits = MapTierLimits(_subscriptionService.GetTierLimits(SubscriptionTier.Free))
                },
                new PlanDto
                {
                    Tier = "Starter",
                    Name = "Starter",
                    Description = "Great for personal projects and small businesses",
                    MonthlyPrice = 12,
                    YearlyPrice = 120,
                    MonthlyPriceId = _stripeService.GetPriceId(SubscriptionTier.Starter, BillingInterval.Monthly),
                    YearlyPriceId = _stripeService.GetPriceId(SubscriptionTier.Starter, BillingInterval.Yearly),
                    Limits = MapTierLimits(_subscriptionService.GetTierLimits(SubscriptionTier.Starter))
                },
                new PlanDto
                {
                    Tier = "Pro",
                    Name = "Pro",
                    Description = "For growing businesses and agencies",
                    MonthlyPrice = 35,
                    YearlyPrice = 350,
                    MonthlyPriceId = _stripeService.GetPriceId(SubscriptionTier.Pro, BillingInterval.Monthly),
                    YearlyPriceId = _stripeService.GetPriceId(SubscriptionTier.Pro, BillingInterval.Yearly),
                    Limits = MapTierLimits(_subscriptionService.GetTierLimits(SubscriptionTier.Pro))
                },
                new PlanDto
                {
                    Tier = "Business",
                    Name = "Business",
                    Description = "Enterprise-grade features and support",
                    MonthlyPrice = 250,
                    YearlyPrice = 2500,
                    MonthlyPriceId = _stripeService.GetPriceId(SubscriptionTier.Business, BillingInterval.Monthly),
                    YearlyPriceId = _stripeService.GetPriceId(SubscriptionTier.Business, BillingInterval.Yearly),
                    Limits = MapTierLimits(_subscriptionService.GetTierLimits(SubscriptionTier.Business))
                }
            };

            return Ok(plans);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting subscription plans");
            return StatusCode(500, "An error occurred while retrieving plans");
        }
    }

    /// <summary>
    /// Create a checkout session for new subscription
    /// </summary>
    [HttpPost("checkout")]
    [ProducesResponseType(typeof(CheckoutSessionResponseDto), 200)]
    public async Task<IActionResult> CreateCheckoutSession(
        [FromQuery] string userEmail,
        [FromBody] CreateCheckoutSessionDto request)
    {
        try
        {
            if (string.IsNullOrEmpty(userEmail))
            {
                return BadRequest("User email is required");
            }

            var user = await _userService.GetByEmailAsync(userEmail);
            if (user == null)
            {
                return NotFound("User not found");
            }

            // Get or create customer
            var customer = await _customerService.GetByUserIdAsync(user.Id);
            if (customer == null)
            {
                var stripeCustomerId = await _stripeService.CreateCustomerAsync(user.Email, user.DisplayName, user.Id);
                customer = await _customerService.CreateAsync(new Customer
                {
                    UserId = user.Id,
                    StripeCustomerId = stripeCustomerId,
                    Email = user.Email,
                    Name = user.DisplayName
                });
            }

            // Parse tier and interval
            if (!Enum.TryParse<SubscriptionTier>(request.Tier, out var tier))
            {
                return BadRequest("Invalid subscription tier");
            }

            if (!Enum.TryParse<BillingInterval>(request.BillingInterval, out var interval))
            {
                return BadRequest("Invalid billing interval");
            }

            var priceId = _stripeService.GetPriceId(tier, interval);
            if (string.IsNullOrEmpty(priceId))
            {
                return BadRequest("Price not configured for selected plan");
            }

            var sessionUrl = await _stripeService.CreateCheckoutSessionAsync(
                customer.StripeCustomerId,
                priceId,
                request.SuccessUrl,
                request.CancelUrl
            );

            return Ok(new CheckoutSessionResponseDto { SessionUrl = sessionUrl });
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("Stripe is not configured"))
        {
            _logger.LogWarning("Stripe checkout attempted but Stripe is not configured");
            return StatusCode(503, new { 
                error = "Payment system not configured", 
                message = "Stripe payment integration is not configured. Please set the Payment__Stripe__SecretKey, Payment__Stripe__PublishableKey, and Payment__Stripe__WebhookSecret environment variables or Azure App Service configuration settings.",
                configRequired = new[] { "Payment__Stripe__SecretKey", "Payment__Stripe__PublishableKey", "Payment__Stripe__WebhookSecret" }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating checkout session for user {UserEmail}", userEmail);
            return StatusCode(500, "An error occurred while creating checkout session");
        }
    }

    /// <summary>
    /// Update (upgrade/downgrade) subscription
    /// </summary>
    [HttpPut("update")]
    [ProducesResponseType(typeof(SubscriptionDto), 200)]
    public async Task<IActionResult> UpdateSubscription(
        [FromQuery] string userEmail,
        [FromBody] UpdateSubscriptionDto request)
    {
        try
        {
            if (string.IsNullOrEmpty(userEmail))
            {
                return BadRequest("User email is required");
            }

            var user = await _userService.GetByEmailAsync(userEmail);
            if (user == null)
            {
                return NotFound("User not found");
            }

            var subscription = await _subscriptionService.GetActiveSubscriptionByUserIdAsync(user.Id);
            if (subscription == null)
            {
                return NotFound("No active subscription found");
            }

            // Parse new tier
            if (!Enum.TryParse<SubscriptionTier>(request.NewTier, out var newTier))
            {
                return BadRequest("Invalid subscription tier");
            }

            if (!Enum.TryParse<BillingInterval>(request.BillingInterval, out var interval))
            {
                return BadRequest("Invalid billing interval");
            }

            var newPriceId = _stripeService.GetPriceId(newTier, interval);
            if (string.IsNullOrEmpty(newPriceId))
            {
                return BadRequest("Price not configured for selected plan");
            }

            var updatedSubscription = await _stripeService.UpdateSubscriptionAsync(
                subscription.StripeSubscriptionId,
                newPriceId,
                newTier
            );

            var limits = _subscriptionService.GetTierLimits(updatedSubscription.Tier);
            return Ok(new SubscriptionDto
            {
                Id = updatedSubscription.Id,
                Tier = updatedSubscription.Tier.ToString(),
                Status = updatedSubscription.Status.ToString(),
                BillingInterval = updatedSubscription.BillingInterval.ToString(),
                PriceAmount = updatedSubscription.PriceAmount / 100m,
                Currency = updatedSubscription.Currency,
                CurrentPeriodStart = updatedSubscription.CurrentPeriodStart,
                CurrentPeriodEnd = updatedSubscription.CurrentPeriodEnd,
                CancelAtPeriodEnd = updatedSubscription.CancelAtPeriodEnd,
                Limits = MapTierLimits(limits)
            });
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("Stripe is not configured"))
        {
            _logger.LogWarning("Stripe subscription update attempted but Stripe is not configured");
            return StatusCode(503, new { 
                error = "Payment system not configured", 
                message = "Stripe payment integration is not configured. Please set the Payment__Stripe__SecretKey, Payment__Stripe__PublishableKey, and Payment__Stripe__WebhookSecret environment variables or Azure App Service configuration settings.",
                configRequired = new[] { "Payment__Stripe__SecretKey", "Payment__Stripe__PublishableKey", "Payment__Stripe__WebhookSecret" }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating subscription for user {UserEmail}", userEmail);
            return StatusCode(500, "An error occurred while updating subscription");
        }
    }

    /// <summary>
    /// Cancel subscription
    /// </summary>
    [HttpDelete("cancel")]
    [ProducesResponseType(200)]
    public async Task<IActionResult> CancelSubscription(
        [FromQuery] string userEmail,
        [FromQuery] bool cancelAtPeriodEnd = true)
    {
        try
        {
            if (string.IsNullOrEmpty(userEmail))
            {
                return BadRequest("User email is required");
            }

            var user = await _userService.GetByEmailAsync(userEmail);
            if (user == null)
            {
                return NotFound("User not found");
            }

            var subscription = await _subscriptionService.GetActiveSubscriptionByUserIdAsync(user.Id);
            if (subscription == null)
            {
                return NotFound("No active subscription found");
            }

            var success = await _stripeService.CancelSubscriptionAsync(
                subscription.StripeSubscriptionId,
                cancelAtPeriodEnd
            );

            if (success)
            {
                await _subscriptionService.CancelAsync(subscription.Id, cancelAtPeriodEnd);
                return Ok(new { message = "Subscription canceled successfully" });
            }

            return StatusCode(500, "Failed to cancel subscription");
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("Stripe is not configured"))
        {
            _logger.LogWarning("Stripe subscription cancellation attempted but Stripe is not configured");
            return StatusCode(503, new { 
                error = "Payment system not configured", 
                message = "Stripe payment integration is not configured. Please set the Payment__Stripe__SecretKey, Payment__Stripe__PublishableKey, and Payment__Stripe__WebhookSecret environment variables or Azure App Service configuration settings.",
                configRequired = new[] { "Payment__Stripe__SecretKey", "Payment__Stripe__PublishableKey", "Payment__Stripe__WebhookSecret" }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error canceling subscription for user {UserEmail}", userEmail);
            return StatusCode(500, "An error occurred while canceling subscription");
        }
    }

    /// <summary>
    /// Create billing portal session
    /// </summary>
    [HttpPost("billing-portal")]
    [ProducesResponseType(typeof(BillingPortalResponseDto), 200)]
    public async Task<IActionResult> CreateBillingPortalSession(
        [FromQuery] string userEmail,
        [FromBody] BillingPortalSessionDto request)
    {
        try
        {
            if (string.IsNullOrEmpty(userEmail))
            {
                return BadRequest("User email is required");
            }

            var user = await _userService.GetByEmailAsync(userEmail);
            if (user == null)
            {
                return NotFound("User not found");
            }

            var customer = await _customerService.GetByUserIdAsync(user.Id);
            if (customer == null)
            {
                return NotFound("No customer record found");
            }

            var portalUrl = await _stripeService.CreateBillingPortalSessionAsync(
                customer.StripeCustomerId,
                request.ReturnUrl
            );

            return Ok(new BillingPortalResponseDto { PortalUrl = portalUrl });
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("Stripe is not configured"))
        {
            _logger.LogWarning("Stripe billing portal attempted but Stripe is not configured");
            return StatusCode(503, new { 
                error = "Payment system not configured", 
                message = "Stripe payment integration is not configured. Please set the Payment__Stripe__SecretKey, Payment__Stripe__PublishableKey, and Payment__Stripe__WebhookSecret environment variables or Azure App Service configuration settings.",
                configRequired = new[] { "Payment__Stripe__SecretKey", "Payment__Stripe__PublishableKey", "Payment__Stripe__WebhookSecret" }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating billing portal session for user {UserEmail}", userEmail);
            return StatusCode(500, "An error occurred while creating billing portal session");
        }
    }

    private TierLimitsDto MapTierLimits(TierLimits limits)
    {
        return new TierLimitsDto
        {
            MaxWebsites = limits.MaxWebsites,
            MaxStorageBytes = limits.MaxStorageBytes,
            MaxBandwidthBytes = limits.MaxBandwidthBytes,
            MaxCustomDomains = limits.MaxCustomDomains,
            HasAds = limits.HasAds,
            HasBranding = limits.HasBranding
        };
    }
}
