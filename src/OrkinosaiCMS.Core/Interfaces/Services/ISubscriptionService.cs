using OrkinosaiCMS.Core.Entities.Subscriptions;

namespace OrkinosaiCMS.Core.Interfaces.Services;

/// <summary>
/// Service interface for subscription management
/// </summary>
public interface ISubscriptionService
{
    /// <summary>
    /// Get subscription by ID
    /// </summary>
    Task<Subscription?> GetByIdAsync(int id);

    /// <summary>
    /// Get active subscription for a customer
    /// </summary>
    Task<Subscription?> GetActiveSubscriptionByCustomerIdAsync(int customerId);

    /// <summary>
    /// Get active subscription for a user
    /// </summary>
    Task<Subscription?> GetActiveSubscriptionByUserIdAsync(int userId);

    /// <summary>
    /// Create a new subscription
    /// </summary>
    Task<Subscription> CreateAsync(Subscription subscription);

    /// <summary>
    /// Update an existing subscription
    /// </summary>
    Task<Subscription> UpdateAsync(Subscription subscription);

    /// <summary>
    /// Cancel a subscription
    /// </summary>
    Task<bool> CancelAsync(int subscriptionId, bool cancelAtPeriodEnd = true);

    /// <summary>
    /// Check if user has reached tier limits for websites
    /// </summary>
    Task<bool> HasReachedWebsiteLimitAsync(int userId);

    /// <summary>
    /// Check if user has reached tier limits for storage
    /// </summary>
    Task<bool> HasReachedStorageLimitAsync(int userId, long currentStorageBytes);

    /// <summary>
    /// Get tier limits for a subscription tier
    /// </summary>
    TierLimits GetTierLimits(SubscriptionTier tier);
}

/// <summary>
/// Tier limits for a subscription plan
/// </summary>
public record TierLimits
{
    public int MaxWebsites { get; init; }
    public long MaxStorageBytes { get; init; }
    public long MaxBandwidthBytes { get; init; }
    public int MaxCustomDomains { get; init; }
    public bool HasAds { get; init; }
    public bool HasBranding { get; init; }
}
