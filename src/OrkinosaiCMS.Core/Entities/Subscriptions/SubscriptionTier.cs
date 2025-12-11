namespace OrkinosaiCMS.Core.Entities.Subscriptions;

/// <summary>
/// Enumeration of subscription tiers
/// </summary>
public enum SubscriptionTier
{
    /// <summary>
    /// Free tier - 1 website, 500MB storage, 10GB bandwidth
    /// </summary>
    Free = 0,

    /// <summary>
    /// Starter tier - 3 websites, 5GB storage, 25GB bandwidth - $12/month
    /// </summary>
    Starter = 1,

    /// <summary>
    /// Pro tier - 10 websites, 25GB storage, 100GB bandwidth - $35/month
    /// </summary>
    Pro = 2,

    /// <summary>
    /// Business tier - 50 websites, 100GB storage, 500GB bandwidth - $250/month
    /// </summary>
    Business = 3
}
