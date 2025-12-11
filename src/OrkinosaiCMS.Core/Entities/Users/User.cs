using OrkinosaiCMS.Core.Common;

namespace OrkinosaiCMS.Core.Entities.Sites;

/// <summary>
/// Represents a user in the CMS
/// </summary>
public class User : BaseEntity
{
    /// <summary>
    /// Username for login
    /// </summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// Email address
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Display name
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// Hashed password
    /// </summary>
    public string PasswordHash { get; set; } = string.Empty;

    /// <summary>
    /// Whether the user is active
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Whether email is verified
    /// </summary>
    public bool EmailConfirmed { get; set; }

    /// <summary>
    /// Last login date
    /// </summary>
    public DateTime? LastLoginOn { get; set; }

    /// <summary>
    /// Profile picture URL
    /// </summary>
    public string? AvatarUrl { get; set; }

    /// <summary>
    /// User roles/permissions
    /// </summary>
    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();

    /// <summary>
    /// Stripe customer ID (if user has made purchases)
    /// </summary>
    public string? StripeCustomerId { get; set; }

    /// <summary>
    /// Current subscription tier (stored as int for database compatibility)
    /// Maps to SubscriptionTier enum: Free = 0, Starter = 1, Pro = 2, Business = 3
    /// </summary>
    public int SubscriptionTierValue { get; set; } = 0; // Default to Free tier
}
