using OrkinosaiCMS.Core.Common;

namespace OrkinosaiCMS.Core.Entities.Subscriptions;

/// <summary>
/// Represents a subscription for a customer
/// </summary>
public class Subscription : BaseEntity
{
    /// <summary>
    /// Customer ID
    /// </summary>
    public int CustomerId { get; set; }

    /// <summary>
    /// Stripe subscription ID
    /// </summary>
    public string StripeSubscriptionId { get; set; } = string.Empty;

    /// <summary>
    /// Subscription tier
    /// </summary>
    public SubscriptionTier Tier { get; set; } = SubscriptionTier.Free;

    /// <summary>
    /// Subscription status
    /// </summary>
    public SubscriptionStatus Status { get; set; } = SubscriptionStatus.Active;

    /// <summary>
    /// Billing interval
    /// </summary>
    public BillingInterval BillingInterval { get; set; } = BillingInterval.Monthly;

    /// <summary>
    /// Price amount in cents
    /// </summary>
    public int PriceAmount { get; set; }

    /// <summary>
    /// Currency code (e.g., "usd")
    /// </summary>
    public string Currency { get; set; } = "usd";

    /// <summary>
    /// Current period start date
    /// </summary>
    public DateTime CurrentPeriodStart { get; set; }

    /// <summary>
    /// Current period end date
    /// </summary>
    public DateTime CurrentPeriodEnd { get; set; }

    /// <summary>
    /// Whether subscription will cancel at period end
    /// </summary>
    public bool CancelAtPeriodEnd { get; set; }

    /// <summary>
    /// Date when subscription was canceled (if applicable)
    /// </summary>
    public DateTime? CanceledAt { get; set; }

    /// <summary>
    /// Date when subscription trial ends (if applicable)
    /// </summary>
    public DateTime? TrialEnd { get; set; }

    /// <summary>
    /// Stripe price ID for this subscription
    /// </summary>
    public string? StripePriceId { get; set; }

    /// <summary>
    /// Navigation property to customer
    /// </summary>
    public Customer? Customer { get; set; }

    /// <summary>
    /// Navigation property to invoices
    /// </summary>
    public ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();
}
