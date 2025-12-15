using Microsoft.AspNetCore.Identity;

namespace OrkinosaiCMS.Core.Entities.Identity;

/// <summary>
/// ApplicationUser extends IdentityUser to integrate with ASP.NET Core Identity
/// Follows Oqtane's pattern of using Identity for authentication
/// </summary>
public class ApplicationUser : IdentityUser<int>
{
    /// <summary>
    /// Display name for the user
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// Profile picture URL
    /// </summary>
    public string? AvatarUrl { get; set; }

    /// <summary>
    /// Last login date
    /// </summary>
    public DateTime? LastLoginOn { get; set; }

    /// <summary>
    /// Last IP address used for login
    /// </summary>
    public string? LastIPAddress { get; set; }

    /// <summary>
    /// Stripe customer ID (if user has made purchases)
    /// </summary>
    public string? StripeCustomerId { get; set; }

    /// <summary>
    /// Current subscription tier (stored as int for database compatibility)
    /// Maps to SubscriptionTier enum: Free = 0, Starter = 1, Pro = 2, Business = 3
    /// </summary>
    public int SubscriptionTierValue { get; set; } = 0;

    /// <summary>
    /// Whether the user account is deleted (soft delete)
    /// </summary>
    public bool IsDeleted { get; set; }

    /// <summary>
    /// Created timestamp
    /// </summary>
    public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Modified timestamp
    /// </summary>
    public DateTime? ModifiedOn { get; set; }

    /// <summary>
    /// Deleted timestamp
    /// </summary>
    public DateTime? DeletedOn { get; set; }
}
