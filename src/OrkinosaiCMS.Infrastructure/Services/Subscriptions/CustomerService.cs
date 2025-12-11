using Microsoft.EntityFrameworkCore;
using OrkinosaiCMS.Core.Entities.Subscriptions;
using OrkinosaiCMS.Core.Interfaces.Repositories;
using OrkinosaiCMS.Core.Interfaces.Services;
using OrkinosaiCMS.Infrastructure.Data;

namespace OrkinosaiCMS.Infrastructure.Services.Subscriptions;

/// <summary>
/// Service implementation for customer management
/// </summary>
public class CustomerService : ICustomerService
{
    private readonly IRepository<Customer> _customerRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ApplicationDbContext _context;

    public CustomerService(
        IRepository<Customer> customerRepository, 
        IUnitOfWork unitOfWork,
        ApplicationDbContext context)
    {
        _customerRepository = customerRepository;
        _unitOfWork = unitOfWork;
        _context = context;
    }

    public async Task<Customer?> GetByIdAsync(int id)
    {
        return await _customerRepository.GetByIdAsync(id);
    }

    public async Task<Customer?> GetByUserIdAsync(int userId)
    {
        return await _context.Customers
            .Include(c => c.Subscriptions)
            .FirstOrDefaultAsync(c => c.UserId == userId && !c.IsDeleted);
    }

    public async Task<Customer?> GetByStripeCustomerIdAsync(string stripeCustomerId)
    {
        return await _context.Customers
            .Include(c => c.Subscriptions)
            .FirstOrDefaultAsync(c => c.StripeCustomerId == stripeCustomerId && !c.IsDeleted);
    }

    public async Task<Customer> CreateAsync(Customer customer)
    {
        customer.CreatedOn = DateTime.UtcNow;
        customer.CreatedBy = "System";
        var result = await _customerRepository.AddAsync(customer);
        await _unitOfWork.SaveChangesAsync();
        return result;
    }

    public async Task<Customer> UpdateAsync(Customer customer)
    {
        customer.ModifiedOn = DateTime.UtcNow;
        customer.ModifiedBy = "System";
        _customerRepository.Update(customer);
        await _unitOfWork.SaveChangesAsync();
        return customer;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var customer = await _customerRepository.GetByIdAsync(id);
        if (customer == null)
            return false;

        customer.IsDeleted = true;
        customer.ModifiedOn = DateTime.UtcNow;
        customer.ModifiedBy = "System";
        _customerRepository.Update(customer);
        await _unitOfWork.SaveChangesAsync();
        return true;
    }
}
