namespace OrkinosaiCMS.Core.Entities.Subscriptions;

/// <summary>
/// Enumeration of subscription statuses
/// </summary>
public enum SubscriptionStatus
{
    /// <summary>
    /// Subscription is in trial period
    /// </summary>
    Trialing = 0,

    /// <summary>
    /// Subscription is active and in good standing
    /// </summary>
    Active = 1,

    /// <summary>
    /// Subscription payment is past due
    /// </summary>
    PastDue = 2,

    /// <summary>
    /// Subscription has been canceled
    /// </summary>
    Canceled = 3,

    /// <summary>
    /// Subscription is unpaid
    /// </summary>
    Unpaid = 4,

    /// <summary>
    /// Subscription is incomplete (payment not completed)
    /// </summary>
    Incomplete = 5,

    /// <summary>
    /// Subscription setup is incomplete and has expired
    /// </summary>
    IncompleteExpired = 6
}
