namespace OrkinosaiCMS.Core.Entities.Subscriptions;

/// <summary>
/// Enumeration of billing intervals
/// </summary>
public enum BillingInterval
{
    /// <summary>
    /// Monthly billing
    /// </summary>
    Monthly = 0,

    /// <summary>
    /// Yearly billing (with ~17% discount)
    /// </summary>
    Yearly = 1
}
