using System.Linq.Expressions;
using OrkinosaiCMS.Core.Common;

namespace OrkinosaiCMS.Core.Interfaces.Repositories;

/// <summary>
/// Generic repository interface for common data operations
/// </summary>
/// <typeparam name="T">Entity type derived from BaseEntity</typeparam>
public interface IRepository<T> where T : BaseEntity
{
    /// <summary>
    /// Get entity by ID
    /// </summary>
    Task<T?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all entities
    /// </summary>
    Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Find entities matching a predicate
    /// </summary>
    Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get first entity matching a predicate
    /// </summary>
    Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Add a new entity
    /// </summary>
    Task<T> AddAsync(T entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Add multiple entities
    /// </summary>
    Task AddRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default);

    /// <summary>
    /// Update an entity
    /// </summary>
    void Update(T entity);

    /// <summary>
    /// Update multiple entities
    /// </summary>
    void UpdateRange(IEnumerable<T> entities);

    /// <summary>
    /// Remove an entity (soft delete)
    /// </summary>
    void Remove(T entity);

    /// <summary>
    /// Remove multiple entities (soft delete)
    /// </summary>
    void RemoveRange(IEnumerable<T> entities);

    /// <summary>
    /// Hard delete an entity from database
    /// </summary>
    void HardDelete(T entity);

    /// <summary>
    /// Count entities matching a predicate
    /// </summary>
    Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if any entity exists matching a predicate
    /// </summary>
    Task<bool> AnyAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
}
