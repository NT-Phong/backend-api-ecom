using System.Collections.Concurrent;
using Ecom.Domain.Common;
using Ecom.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;

namespace Ecom.Infrastructure.Persistence.Database.UnitOfWork;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<UnitOfWork> _logger;
    private IDbContextTransaction? _transaction;

    // Cache repositories to avoid creating multiple instances for the same entity type
    private readonly ConcurrentDictionary<Type, object> _repositories = new();

    public UnitOfWork(ApplicationDbContext context, ILogger<UnitOfWork> logger)
    {
        _context = context;
        _logger = logger;
    }

    #region Repository Access

    /// <summary>
    /// Get or create a repository for the specified entity type.
    /// Repositories are cached per entity type for the lifetime of this UnitOfWork.
    /// </summary>
    public IBaseRepository<TEntity> Repository<TEntity>() where TEntity : BaseEntity
    {
        var entityType = typeof(TEntity);

        if (_repositories.TryGetValue(entityType, out var existingRepo))
        {
            return (IBaseRepository<TEntity>)existingRepo;
        }

        var repository = new BaseRepository<TEntity>(_context);
        _repositories.TryAdd(entityType, repository);
        return repository;
    }

    #endregion

    #region Transaction Management

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction != null) return false;
        _transaction =
            await _context.Database.BeginTransactionAsync(System.Data.IsolationLevel.ReadCommitted, cancellationToken);
        _logger.LogDebug("Transaction started");
        return true;
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction is null)
            throw new InvalidOperationException("Cannot commit because no transaction is active.");

        await SaveChangesAsync(cancellationToken);
        try
        {
            await _transaction.CommitAsync(cancellationToken);
            _logger.LogDebug("Transaction committed");
        }
        finally
        {
            if (_transaction != null)
            {
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }
    }

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await (_transaction?.RollbackAsync(cancellationToken) ?? Task.CompletedTask);
            _logger.LogDebug("Transaction rolled back");
        }
        finally
        {
            if (_transaction != null)
            {
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }
    }

    /// <summary>
    /// Execute an action within a transaction scope.
    /// Automatically commits on success or rolls back on exception.
    /// </summary>
    public async Task ExecuteInTransactionAsync(Func<Task> action, CancellationToken cancellationToken = default)
    {
        var ownsTransaction = await BeginTransactionAsync(cancellationToken);
        try
        {
            await action();
            if (ownsTransaction) await CommitTransactionAsync(cancellationToken);
        }
        catch
        {
            if (ownsTransaction) await RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }

    /// <summary>
    /// Execute a function within a transaction scope and return the result.
    /// Automatically commits on success or rolls back on exception.
    /// </summary>
    public async Task<TResult> ExecuteInTransactionAsync<TResult>(Func<Task<TResult>> func, CancellationToken cancellationToken = default)
    {
        var ownsTransaction = await BeginTransactionAsync(cancellationToken);
        try
        {
            var result = await func();
            if (ownsTransaction) await CommitTransactionAsync(cancellationToken);
            return result;
        }
        catch
        {
            if (ownsTransaction) await RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }

    #endregion

    public void Dispose()
    {
        _transaction?.Dispose();
        _repositories.Clear();
        GC.SuppressFinalize(this);
    }

    public void ClearChangeTracker()
    {
        _context.ChangeTracker.Clear();
    }
}

