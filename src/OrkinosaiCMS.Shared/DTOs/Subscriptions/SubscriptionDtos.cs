namespace OrkinosaiCMS.Shared.DTOs.Subscriptions;

/// <summary>
/// DTO for subscription information
/// </summary>
public class SubscriptionDto
{
    public int Id { get; set; }
    public string Tier { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string BillingInterval { get; set; } = string.Empty;
    public decimal PriceAmount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public DateTime CurrentPeriodStart { get; set; }
    public DateTime CurrentPeriodEnd { get; set; }
    public bool CancelAtPeriodEnd { get; set; }
    public DateTime? CanceledAt { get; set; }
    public TierLimitsDto Limits { get; set; } = new();
}

/// <summary>
/// DTO for tier limits
/// </summary>
public class TierLimitsDto
{
    public int MaxWebsites { get; set; }
    public long MaxStorageBytes { get; set; }
    public long MaxBandwidthBytes { get; set; }
    public int MaxCustomDomains { get; set; }
    public bool HasAds { get; set; }
    public bool HasBranding { get; set; }
}

/// <summary>
/// DTO for subscription plan information
/// </summary>
public class PlanDto
{
    public string Tier { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal MonthlyPrice { get; set; }
    public decimal YearlyPrice { get; set; }
    public string MonthlyPriceId { get; set; } = string.Empty;
    public string YearlyPriceId { get; set; } = string.Empty;
    public TierLimitsDto Limits { get; set; } = new();
}

/// <summary>
/// DTO for creating a subscription checkout session
/// </summary>
public class CreateCheckoutSessionDto
{
    public string Tier { get; set; } = string.Empty;
    public string BillingInterval { get; set; } = "Monthly";
    public string SuccessUrl { get; set; } = string.Empty;
    public string CancelUrl { get; set; } = string.Empty;
}

/// <summary>
/// DTO for checkout session response
/// </summary>
public class CheckoutSessionResponseDto
{
    public string SessionUrl { get; set; } = string.Empty;
}

/// <summary>
/// DTO for upgrading/downgrading subscription
/// </summary>
public class UpdateSubscriptionDto
{
    public string NewTier { get; set; } = string.Empty;
    public string BillingInterval { get; set; } = "Monthly";
}

/// <summary>
/// DTO for billing portal session
/// </summary>
public class BillingPortalSessionDto
{
    public string ReturnUrl { get; set; } = string.Empty;
}

/// <summary>
/// DTO for billing portal response
/// </summary>
public class BillingPortalResponseDto
{
    public string PortalUrl { get; set; } = string.Empty;
}
