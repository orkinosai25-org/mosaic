using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using OrkinosaiCMS.Core.Entities.Sites;
using OrkinosaiCMS.Core.Entities.Subscriptions;
using OrkinosaiCMS.Core.Interfaces.Services;
using OrkinosaiCMS.Infrastructure.Data;
using OrkinosaiCMS.Shared.DTOs.Subscriptions;
using OrkinosaiCMS.Tests.Integration.Fixtures;

namespace OrkinosaiCMS.Tests.Integration.Api;

/// <summary>
/// Integration tests for Subscription API endpoints
/// Tests the critical subscription management functionality added in PR #49
/// </summary>
public class SubscriptionTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;

    public SubscriptionTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    /// <summary>
    /// Helper method to create a unique test user to avoid test interference
    /// Each test that creates subscriptions should use its own user to prevent
    /// tests from affecting each other when they run in parallel or random order
    /// </summary>
    private async Task<User> CreateUniqueTestUser(ApplicationDbContext context, string prefix)
    {
        var user = new User
        {
            Username = prefix,
            Email = $"{prefix}@test.com",
            DisplayName = $"{prefix} User",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("TestPassword123!"),
            IsActive = true,
            CreatedOn = DateTime.UtcNow
        };
        context.Users.Add(user);
        await context.SaveChangesAsync();
        return user;
    }

    [Fact]
    public async Task GetCurrentSubscription_WithoutSubscription_ShouldReturnFreeTier()
    {
        // Arrange
        var userEmail = "admin@test.com"; // User seeded in factory

        // Act
        var response = await _client.GetAsync($"/api/subscription/current?userEmail={userEmail}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var subscription = await response.Content.ReadFromJsonAsync<SubscriptionDto>();
        subscription.Should().NotBeNull();
        subscription!.Tier.Should().Be("Free");
        subscription.Status.Should().Be("Active");
        subscription.Limits.Should().NotBeNull();
        subscription.Limits!.MaxWebsites.Should().Be(1);
    }

    [Fact]
    public async Task GetPlans_ShouldReturnAllAvailablePlans()
    {
        // Act
        var response = await _client.GetAsync("/api/subscription/plans");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var plans = await response.Content.ReadFromJsonAsync<List<PlanDto>>();
        plans.Should().NotBeNull();
        plans!.Should().HaveCount(4); // Free, Starter, Pro, Business
        
        var freePlan = plans.FirstOrDefault(p => p.Tier == "Free");
        freePlan.Should().NotBeNull();
        freePlan!.MonthlyPrice.Should().Be(0);
        
        var starterPlan = plans.FirstOrDefault(p => p.Tier == "Starter");
        starterPlan.Should().NotBeNull();
        starterPlan!.MonthlyPrice.Should().Be(12);
    }

    [Fact]
    public async Task GetCurrentSubscription_WithNonExistentUser_ShouldReturnNotFound()
    {
        // Arrange
        var userEmail = "nonexistent@test.com";

        // Act
        var response = await _client.GetAsync($"/api/subscription/current?userEmail={userEmail}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetCurrentSubscription_WithEmptyEmail_ShouldReturnBadRequest()
    {
        // Act
        var response = await _client.GetAsync("/api/subscription/current?userEmail=");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task SubscriptionService_GetTierLimits_ShouldReturnCorrectLimitsForEachTier()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var subscriptionService = scope.ServiceProvider.GetRequiredService<ISubscriptionService>();

        // Act & Assert - Free Tier
        var freeLimits = subscriptionService.GetTierLimits(SubscriptionTier.Free);
        freeLimits.MaxWebsites.Should().Be(1);
        freeLimits.MaxStorageBytes.Should().Be(500L * 1024 * 1024); // 500 MB
        freeLimits.HasAds.Should().BeTrue();
        freeLimits.HasBranding.Should().BeTrue();

        // Act & Assert - Starter Tier
        var starterLimits = subscriptionService.GetTierLimits(SubscriptionTier.Starter);
        starterLimits.MaxWebsites.Should().Be(3);
        starterLimits.MaxStorageBytes.Should().Be(5L * 1024 * 1024 * 1024); // 5 GB
        starterLimits.HasAds.Should().BeFalse();
        starterLimits.HasBranding.Should().BeFalse();

        // Act & Assert - Pro Tier
        var proLimits = subscriptionService.GetTierLimits(SubscriptionTier.Pro);
        proLimits.MaxWebsites.Should().Be(10);
        proLimits.MaxStorageBytes.Should().Be(25L * 1024 * 1024 * 1024); // 25 GB

        // Act & Assert - Business Tier
        var businessLimits = subscriptionService.GetTierLimits(SubscriptionTier.Business);
        businessLimits.MaxWebsites.Should().Be(50);
        businessLimits.MaxStorageBytes.Should().Be(100L * 1024 * 1024 * 1024); // 100 GB
    }

    [Fact]
    public async Task SubscriptionService_CreateSubscription_ShouldPersistToDatabase()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var subscriptionService = scope.ServiceProvider.GetRequiredService<ISubscriptionService>();
        var customerService = scope.ServiceProvider.GetRequiredService<ICustomerService>();

        // Create a unique test user for this test to avoid interference with other tests
        var testUser = await CreateUniqueTestUser(context, "createsubtest");

        // Create customer
        var customer = await customerService.CreateAsync(new Customer
        {
            UserId = testUser.Id,
            StripeCustomerId = "cus_test_123",
            Email = testUser.Email,
            Name = testUser.DisplayName
        });

        var subscription = new Subscription
        {
            CustomerId = customer.Id,
            StripeSubscriptionId = "sub_test_123",
            Tier = SubscriptionTier.Starter,
            Status = SubscriptionStatus.Active,
            BillingInterval = BillingInterval.Monthly,
            PriceAmount = 1200, // $12 in cents
            Currency = "usd",
            CurrentPeriodStart = DateTime.UtcNow,
            CurrentPeriodEnd = DateTime.UtcNow.AddMonths(1)
        };

        // Act
        var result = await subscriptionService.CreateAsync(subscription);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().BeGreaterThan(0);
        result.CustomerId.Should().Be(customer.Id);
        result.Tier.Should().Be(SubscriptionTier.Starter);
        result.Status.Should().Be(SubscriptionStatus.Active);

        // Verify it's in the database
        var retrieved = await subscriptionService.GetByIdAsync(result.Id);
        retrieved.Should().NotBeNull();
        retrieved!.StripeSubscriptionId.Should().Be("sub_test_123");
    }

    [Fact]
    public async Task SubscriptionService_GetActiveSubscriptionByUserId_ShouldReturnActiveSubscription()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var subscriptionService = scope.ServiceProvider.GetRequiredService<ISubscriptionService>();
        var customerService = scope.ServiceProvider.GetRequiredService<ICustomerService>();

        // Create a unique test user for this test to avoid interference with other tests
        var testUser = await CreateUniqueTestUser(context, "activesubtest");

        var customer = await customerService.CreateAsync(new Customer
        {
            UserId = testUser.Id,
            StripeCustomerId = "cus_active_test",
            Email = testUser.Email,
            Name = testUser.DisplayName
        });

        await subscriptionService.CreateAsync(new Subscription
        {
            CustomerId = customer.Id,
            StripeSubscriptionId = "sub_active_test",
            Tier = SubscriptionTier.Pro,
            Status = SubscriptionStatus.Active,
            BillingInterval = BillingInterval.Yearly,
            PriceAmount = 35000,
            Currency = "usd",
            CurrentPeriodStart = DateTime.UtcNow,
            CurrentPeriodEnd = DateTime.UtcNow.AddYears(1)
        });

        // Act
        var result = await subscriptionService.GetActiveSubscriptionByUserIdAsync(testUser.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Tier.Should().Be(SubscriptionTier.Pro);
        result.Status.Should().Be(SubscriptionStatus.Active);
        result.StripeSubscriptionId.Should().Be("sub_active_test");
    }

    [Fact]
    public async Task SubscriptionService_CancelSubscription_ShouldUpdateStatusAndCanceledDate()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var subscriptionService = scope.ServiceProvider.GetRequiredService<ISubscriptionService>();
        var customerService = scope.ServiceProvider.GetRequiredService<ICustomerService>();

        // Create a unique test user for this test to avoid interference with other tests
        var testUser = await CreateUniqueTestUser(context, "cancelsubtest");

        var customer = await customerService.CreateAsync(new Customer
        {
            UserId = testUser.Id,
            StripeCustomerId = "cus_cancel_test",
            Email = testUser.Email,
            Name = testUser.DisplayName
        });

        var subscription = await subscriptionService.CreateAsync(new Subscription
        {
            CustomerId = customer.Id,
            StripeSubscriptionId = "sub_cancel_test",
            Tier = SubscriptionTier.Starter,
            Status = SubscriptionStatus.Active,
            BillingInterval = BillingInterval.Monthly,
            PriceAmount = 1200,
            Currency = "usd",
            CurrentPeriodStart = DateTime.UtcNow,
            CurrentPeriodEnd = DateTime.UtcNow.AddMonths(1)
        });

        // Act
        var result = await subscriptionService.CancelAsync(subscription.Id, cancelAtPeriodEnd: false);

        // Assert
        result.Should().BeTrue();
        var canceled = await subscriptionService.GetByIdAsync(subscription.Id);
        canceled.Should().NotBeNull();
        canceled!.Status.Should().Be(SubscriptionStatus.Canceled);
        canceled.CanceledAt.Should().NotBeNull();
        canceled.CanceledAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task SubscriptionService_HasReachedWebsiteLimit_ShouldEnforceTierLimits()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var subscriptionService = scope.ServiceProvider.GetRequiredService<ISubscriptionService>();
        var customerService = scope.ServiceProvider.GetRequiredService<ICustomerService>();

        // Create a unique test user for this test to avoid interference with other tests
        var testUser = await CreateUniqueTestUser(context, "limitsubtest");

        var customer = await customerService.CreateAsync(new Customer
        {
            UserId = testUser.Id,
            StripeCustomerId = "cus_limit_test",
            Email = testUser.Email,
            Name = testUser.DisplayName
        });

        // Create a Free tier subscription (limit: 1 website)
        await subscriptionService.CreateAsync(new Subscription
        {
            CustomerId = customer.Id,
            StripeSubscriptionId = "sub_limit_test",
            Tier = SubscriptionTier.Free,
            Status = SubscriptionStatus.Active,
            BillingInterval = BillingInterval.Monthly,
            PriceAmount = 0,
            Currency = "usd",
            CurrentPeriodStart = DateTime.UtcNow,
            CurrentPeriodEnd = DateTime.UtcNow.AddMonths(1)
        });

        // Create a site for this user
        context.Sites.Add(new Site
        {
            Name = "Test Site",
            Url = "/testlimit",
            AdminEmail = testUser.Email,
            IsActive = true,
            CreatedBy = testUser.Id.ToString(),
            CreatedOn = DateTime.UtcNow
        });
        await context.SaveChangesAsync();

        // Act
        var hasReachedLimit = await subscriptionService.HasReachedWebsiteLimitAsync(testUser.Id);

        // Assert
        hasReachedLimit.Should().BeTrue("user on Free tier has already created 1 website (the limit)");
    }
}
