using OrkinosaiCMS.Core.Common;

namespace OrkinosaiCMS.Core.Entities.Subscriptions;

/// <summary>
/// Represents an invoice for a subscription
/// </summary>
public class Invoice : BaseEntity
{
    /// <summary>
    /// Subscription ID
    /// </summary>
    public int SubscriptionId { get; set; }

    /// <summary>
    /// Stripe invoice ID
    /// </summary>
    public string StripeInvoiceId { get; set; } = string.Empty;

    /// <summary>
    /// Invoice number
    /// </summary>
    public string InvoiceNumber { get; set; } = string.Empty;

    /// <summary>
    /// Invoice status
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Invoice amount in cents
    /// </summary>
    public int AmountDue { get; set; }

    /// <summary>
    /// Amount paid in cents
    /// </summary>
    public int AmountPaid { get; set; }

    /// <summary>
    /// Currency code
    /// </summary>
    public string Currency { get; set; } = "usd";

    /// <summary>
    /// Invoice PDF URL
    /// </summary>
    public string? InvoicePdfUrl { get; set; }

    /// <summary>
    /// Hosted invoice URL
    /// </summary>
    public string? HostedInvoiceUrl { get; set; }

    /// <summary>
    /// Date invoice was finalized
    /// </summary>
    public DateTime? FinalizedAt { get; set; }

    /// <summary>
    /// Date invoice was paid
    /// </summary>
    public DateTime? PaidAt { get; set; }

    /// <summary>
    /// Due date for invoice
    /// </summary>
    public DateTime? DueDate { get; set; }

    /// <summary>
    /// Navigation property to subscription
    /// </summary>
    public Subscription? Subscription { get; set; }
}
