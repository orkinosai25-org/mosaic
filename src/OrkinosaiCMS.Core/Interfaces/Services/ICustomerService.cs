using OrkinosaiCMS.Core.Entities.Subscriptions;

namespace OrkinosaiCMS.Core.Interfaces.Services;

/// <summary>
/// Service interface for customer management
/// </summary>
public interface ICustomerService
{
    /// <summary>
    /// Get customer by ID
    /// </summary>
    Task<Customer?> GetByIdAsync(int id);

    /// <summary>
    /// Get customer by user ID
    /// </summary>
    Task<Customer?> GetByUserIdAsync(int userId);

    /// <summary>
    /// Get customer by Stripe customer ID
    /// </summary>
    Task<Customer?> GetByStripeCustomerIdAsync(string stripeCustomerId);

    /// <summary>
    /// Create a new customer
    /// </summary>
    Task<Customer> CreateAsync(Customer customer);

    /// <summary>
    /// Update an existing customer
    /// </summary>
    Task<Customer> UpdateAsync(Customer customer);

    /// <summary>
    /// Delete a customer
    /// </summary>
    Task<bool> DeleteAsync(int id);
}
