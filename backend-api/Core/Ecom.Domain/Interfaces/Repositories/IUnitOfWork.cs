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
    /// <returns><c>true</c> when this caller opened the transaction; <c>false</c> when it joined an existing scope.</returns>
    Task<bool> BeginTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Commit the current transaction (also saves changes).
    /// </summary>
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Rollback the current transaction.
    /// </summary>
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Legacy helper for existing modules. New MediatR commands must implement
    /// ITransactionalRequest and let UnitOfWorkBehavior own the transaction.
    /// </summary>
    Task ExecuteInTransactionAsync(Func<Task> action, CancellationToken cancellationToken = default);

    /// <summary>
    /// Legacy helper for existing modules. New MediatR commands must implement
    /// ITransactionalRequest and let UnitOfWorkBehavior own the transaction.
    /// </summary>
    Task<TResult> ExecuteInTransactionAsync<TResult>(Func<Task<TResult>> func, CancellationToken cancellationToken = default);

	/// <summary>
	/// Detaches all entities currently tracked by the ChangeTracker. Use this when a retry is needed
	/// after SaveChangesAsync fails (e.g., due to a unique key violation) in order to build a fresh entity.
	/// </summary>
	void ClearChangeTracker();
}

