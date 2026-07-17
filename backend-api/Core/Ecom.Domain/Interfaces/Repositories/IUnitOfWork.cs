using Ecom.Domain.Common;

namespace Ecom.Domain.Interfaces.Repositories;

public interface IUnitOfWork : IDisposable
{
    /// <summary>
    /// Get or create a repository for the specified entity type.
    /// </summary>
    IBaseRepository<TEntity> Repository<TEntity>() where TEntity : BaseEntity;

    /// <summary>
    /// Save all pending changes to the database.
    /// </summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Begin a new database transaction.
    /// </summary>
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Commit the current transaction (also saves changes).
    /// </summary>
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Rollback the current transaction.
    /// </summary>
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Execute an action within a transaction scope.
    /// Automatically commits on success or rolls back on exception.
    /// </summary>
    Task ExecuteInTransactionAsync(Func<Task> action, CancellationToken cancellationToken = default);

    /// <summary>
    /// Execute a function within a transaction scope and return the result.
    /// Automatically commits on success or rolls back on exception.
    /// </summary>
    Task<TResult> ExecuteInTransactionAsync<TResult>(Func<Task<TResult>> func, CancellationToken cancellationToken = default);

	/// <summary>
	/// Detaches all entities currently tracked by the ChangeTracker. Use this when a retry is needed
	/// after SaveChangesAsync fails (e.g., due to a unique key violation) in order to build a fresh entity.
	/// </summary>
	void ClearChangeTracker();
}

