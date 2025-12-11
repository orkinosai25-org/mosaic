using OrkinosaiCMS.Core.Common;

namespace OrkinosaiCMS.Core.Entities.Subscriptions;

/// <summary>
/// Represents a Stripe customer record
/// </summary>
public class Customer : BaseEntity
{
    /// <summary>
    /// User ID associated with this customer
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// Stripe customer ID
    /// </summary>
    public string StripeCustomerId { get; set; } = string.Empty;

    /// <summary>
    /// Customer email address
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Customer name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Customer phone number
    /// </summary>
    public string? Phone { get; set; }

    /// <summary>
    /// Default payment method ID
    /// </summary>
    public string? DefaultPaymentMethodId { get; set; }

    /// <summary>
    /// Customer's currency
    /// </summary>
    public string Currency { get; set; } = "usd";

    /// <summary>
    /// Customer's address line 1
    /// </summary>
    public string? AddressLine1 { get; set; }

    /// <summary>
    /// Customer's address line 2
    /// </summary>
    public string? AddressLine2 { get; set; }

    /// <summary>
    /// Customer's city
    /// </summary>
    public string? City { get; set; }

    /// <summary>
    /// Customer's state/province
    /// </summary>
    public string? State { get; set; }

    /// <summary>
    /// Customer's postal code
    /// </summary>
    public string? PostalCode { get; set; }

    /// <summary>
    /// Customer's country
    /// </summary>
    public string? Country { get; set; }

    /// <summary>
    /// Navigation property to subscriptions
    /// </summary>
    public ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();
}
