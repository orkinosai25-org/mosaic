using OrkinosaiCMS.Core.Common;

namespace OrkinosaiCMS.Core.Entities.Subscriptions;

/// <summary>
/// Represents a payment method for a customer
/// </summary>
public class PaymentMethod : BaseEntity
{
    /// <summary>
    /// Customer ID
    /// </summary>
    public int CustomerId { get; set; }

    /// <summary>
    /// Stripe payment method ID
    /// </summary>
    public string StripePaymentMethodId { get; set; } = string.Empty;

    /// <summary>
    /// Payment method type (e.g., "card")
    /// </summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Card brand (e.g., "visa", "mastercard")
    /// </summary>
    public string? CardBrand { get; set; }

    /// <summary>
    /// Last 4 digits of card
    /// </summary>
    public string? CardLast4 { get; set; }

    /// <summary>
    /// Card expiration month
    /// </summary>
    public int? CardExpMonth { get; set; }

    /// <summary>
    /// Card expiration year
    /// </summary>
    public int? CardExpYear { get; set; }

    /// <summary>
    /// Whether this is the default payment method
    /// </summary>
    public bool IsDefault { get; set; }

    /// <summary>
    /// Navigation property to customer
    /// </summary>
    public Customer? Customer { get; set; }
}
