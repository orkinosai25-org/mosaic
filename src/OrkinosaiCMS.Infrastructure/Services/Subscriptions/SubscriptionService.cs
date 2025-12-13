using Microsoft.EntityFrameworkCore;
using OrkinosaiCMS.Core.Entities.Subscriptions;
using OrkinosaiCMS.Core.Interfaces.Repositories;
using OrkinosaiCMS.Core.Interfaces.Services;
using OrkinosaiCMS.Infrastructure.Data;

namespace OrkinosaiCMS.Infrastructure.Services.Subscriptions;

/// <summary>
/// Service implementation for subscription management
/// </summary>
public class SubscriptionService : ISubscriptionService
{
    private readonly IRepository<Subscription> _subscriptionRepository;
    private readonly IRepository<Customer> _customerRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ApplicationDbContext _context;

    public SubscriptionService(
        IRepository<Subscription> subscriptionRepository,
        IRepository<Customer> customerRepository,
        IUnitOfWork unitOfWork,
        ApplicationDbContext context)
    {
        _subscriptionRepository = subscriptionRepository;
        _customerRepository = customerRepository;
        _unitOfWork = unitOfWork;
        _context = context;
    }

    public async Task<Subscription?> GetByIdAsync(int id)
    {
        return await _subscriptionRepository.GetByIdAsync(id);
    }

    public async Task<Subscription?> GetActiveSubscriptionByCustomerIdAsync(int customerId)
    {
        return await _context.Subscriptions
            .Include(s => s.Customer)
            .Where(s => s.CustomerId == customerId && 
                       (s.Status == SubscriptionStatus.Active || s.Status == SubscriptionStatus.Trialing) &&
                       !s.IsDeleted)
            .OrderByDescending(s => s.CreatedOn)
            .FirstOrDefaultAsync();
    }

    public async Task<Subscription?> GetActiveSubscriptionByUserIdAsync(int userId)
    {
        // First, try to get the most recent customer for this user
        var mostRecentCustomer = await _context.Customers
            .Where(c => c.UserId == userId && !c.IsDeleted)
            .OrderByDescending(c => c.CreatedOn)
            .FirstOrDefaultAsync();

        if (mostRecentCustomer == null)
            return null;

        // Check if the most recent customer has an active subscription
        var subscription = await GetActiveSubscriptionByCustomerIdAsync(mostRecentCustomer.Id);
        if (subscription != null)
            return subscription;

        // If not, check other customers for this user (fallback for edge cases)
        var otherCustomerIds = await _context.Customers
            .Where(c => c.UserId == userId && c.Id != mostRecentCustomer.Id && !c.IsDeleted)
            .Select(c => c.Id)
            .ToListAsync();

        if (!otherCustomerIds.Any())
            return null;

        // Get the most recent active subscription across other customers
        return await _context.Subscriptions
            .Include(s => s.Customer)
            .Where(s => otherCustomerIds.Contains(s.CustomerId) &&
                       (s.Status == SubscriptionStatus.Active || s.Status == SubscriptionStatus.Trialing) &&
                       !s.IsDeleted)
            .OrderByDescending(s => s.CreatedOn)
            .FirstOrDefaultAsync();
    }

    public async Task<Subscription> CreateAsync(Subscription subscription)
    {
        subscription.CreatedOn = DateTime.UtcNow;
        subscription.CreatedBy = "System";
        var result = await _subscriptionRepository.AddAsync(subscription);
        await _unitOfWork.SaveChangesAsync();
        return result;
    }

    public async Task<Subscription> UpdateAsync(Subscription subscription)
    {
        subscription.ModifiedOn = DateTime.UtcNow;
        subscription.ModifiedBy = "System";
        _subscriptionRepository.Update(subscription);
        await _unitOfWork.SaveChangesAsync();
        return subscription;
    }

    public async Task<bool> CancelAsync(int subscriptionId, bool cancelAtPeriodEnd = true)
    {
        var subscription = await _subscriptionRepository.GetByIdAsync(subscriptionId);
        if (subscription == null)
            return false;

        subscription.CancelAtPeriodEnd = cancelAtPeriodEnd;
        subscription.Status = cancelAtPeriodEnd ? subscription.Status : SubscriptionStatus.Canceled;
        subscription.CanceledAt = DateTime.UtcNow;
        subscription.ModifiedOn = DateTime.UtcNow;
        subscription.ModifiedBy = "System";

        _subscriptionRepository.Update(subscription);
        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    public async Task<bool> HasReachedWebsiteLimitAsync(int userId)
    {
        var subscription = await GetActiveSubscriptionByUserIdAsync(userId);
        if (subscription == null)
            return false;

        var limits = GetTierLimits(subscription.Tier);
        var websiteCount = await _context.Sites
            .CountAsync(s => s.CreatedBy == userId.ToString() && !s.IsDeleted);

        return websiteCount >= limits.MaxWebsites;
    }

    public async Task<bool> HasReachedStorageLimitAsync(int userId, long currentStorageBytes)
    {
        var subscription = await GetActiveSubscriptionByUserIdAsync(userId);
        if (subscription == null)
            return false;

        var limits = GetTierLimits(subscription.Tier);
        return currentStorageBytes >= limits.MaxStorageBytes;
    }

    public TierLimits GetTierLimits(SubscriptionTier tier)
    {
        return tier switch
        {
            SubscriptionTier.Free => new TierLimits
            {
                MaxWebsites = 1,
                MaxStorageBytes = 500L * 1024 * 1024, // 500 MB
                MaxBandwidthBytes = 10L * 1024 * 1024 * 1024, // 10 GB
                MaxCustomDomains = 0,
                HasAds = true,
                HasBranding = true
            },
            SubscriptionTier.Starter => new TierLimits
            {
                MaxWebsites = 3,
                MaxStorageBytes = 5L * 1024 * 1024 * 1024, // 5 GB
                MaxBandwidthBytes = 25L * 1024 * 1024 * 1024, // 25 GB
                MaxCustomDomains = 1,
                HasAds = false,
                HasBranding = false
            },
            SubscriptionTier.Pro => new TierLimits
            {
                MaxWebsites = 10,
                MaxStorageBytes = 25L * 1024 * 1024 * 1024, // 25 GB
                MaxBandwidthBytes = 100L * 1024 * 1024 * 1024, // 100 GB
                MaxCustomDomains = 10,
                HasAds = false,
                HasBranding = false
            },
            SubscriptionTier.Business => new TierLimits
            {
                MaxWebsites = 50,
                MaxStorageBytes = 100L * 1024 * 1024 * 1024, // 100 GB
                MaxBandwidthBytes = 500L * 1024 * 1024 * 1024, // 500 GB
                MaxCustomDomains = 50,
                HasAds = false,
                HasBranding = false
            },
            _ => throw new ArgumentException($"Unknown tier: {tier}")
        };
    }
}
