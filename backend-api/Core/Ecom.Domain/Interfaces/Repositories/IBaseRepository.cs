using Ecom.Domain.Common;
using Ecom.Domain.Models;
using System.Linq.Expressions;

namespace Ecom.Domain.Interfaces.Repositories;

/// <summary>
/// Base repository interface providing common CRUD and query operations for entities.
/// Supports Include for loading related entities (navigation properties).
/// </summary>
/// <typeparam name="TEntity">The entity type that extends BaseEntity</typeparam>
public interface IBaseRepository<TEntity> where TEntity : BaseEntity
{
    #region Query Methods

    /// <summary>
    /// Find entities with optional filters, ordering, paging, and includes.
    /// </summary>
    Task<List<TEntity>> FindAsync(
        Expression<Func<TEntity, bool>>[]? filters = null,
        string? orderBy = null,
        int skip = 0,
        int limit = 0,
        Expression<Func<TEntity, object>>[]? includes = null);

    /// <summary>
    /// Find entities and project to DTO type.
    /// </summary>
    Task<List<TDto>> FindAsync<TDto>(
        Expression<Func<TEntity, bool>>[]? filters = null,
        string? orderBy = null,
        int skip = 0,
        int limit = 0,
        Expression<Func<TEntity, object>>[]? includes = null);
    /// <summary>
    /// Find entities with total count for pagination.
    /// </summary>
    Task<FindResult<TEntity>> FindResultAsync(
    Expression<Func<TEntity, bool>>[]? filters = null,
    string? orderBy = null,
    int skip = 0,
    int limit = 0,
    Expression<Func<TEntity, object>>[]? includes = null);

    /// <summary>
    /// Count entities matching the filters.
    /// </summary>
    Task<int> CountAsync(Expression<Func<TEntity, bool>>[]? filters = null);

    /// <summary>
    /// Check if any entity matches the filters.
    /// </summary>
    Task<bool> AnyAsync(Expression<Func<TEntity, bool>>[]? filters = null);

    /// <summary>
    /// Check if an entity with the given ID exists (not deleted).
    /// </summary>
    Task<bool> ExistsAsync(Guid id);

    /// <summary>
    /// Check if all entities with the given IDs exist (not deleted).
    /// </summary>
    Task<bool> ExistsAsync(IEnumerable<Guid> ids);

    #endregion

    #region FindOne Methods

    /// <summary>
    /// Find a single entity by ID with optional includes for related entities.
    /// </summary>
    Task<TEntity?> FindByIdAsync(Guid id, params Expression<Func<TEntity, object>>[] includes);

    /// <summary>
    /// Find a single entity by ID and project to DTO.
    /// </summary>
    Task<TDto?> FindByIdAsync<TDto>(Guid id, params Expression<Func<TEntity, object>>[] includes);

    /// <summary>
    /// Find the first entity matching filters with optional ordering and includes.
    /// </summary>
    Task<TEntity?> FindOneAsync(
        Expression<Func<TEntity, bool>>[]? filters = null,
        string? orderBy = null,
        Expression<Func<TEntity, object>>[]? includes = null);

    /// <summary>
    /// Find the first entity matching filters and project to DTO.
    /// </summary>
    Task<TDto?> FindOneAsync<TDto>(
        Expression<Func<TEntity, bool>>[]? filters = null,
        string? orderBy = null,
        Expression<Func<TEntity, object>>[]? includes = null);

    #endregion

    #region CUD Operations

    /// <summary>
    /// Insert a new entity.
    /// </summary>
    Task<TEntity> InsertAsync(TEntity entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Insert multiple entities.
    /// </summary>
    Task InsertRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);

    /// <summary>
    /// Update an existing entity.
    /// </summary>
    Task<TEntity> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Update multiple entities.
    /// </summary>
    Task UpdateRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);

    /// <summary>
    /// Soft delete an entity by ID.
    /// </summary>
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Soft delete an entity.
    /// </summary>
    Task<bool> DeleteAsync(TEntity entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Soft delete multiple entities.
    /// </summary>
    Task DeleteRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);

    /// <summary>
    /// Permanently delete an entity by ID.
    /// </summary>
    Task<bool> HardDeleteAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Permanently delete multiple entities.
    /// </summary>
    Task HardDeleteRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);

    #endregion

    #region Query Builders

    /// <summary>
    /// Get a queryable for the entity type. By default excludes soft-deleted entities.
    /// </summary>
    IQueryable<TEntity> Query(bool includeDeleted = false);

    /// <summary>
    /// Get a no-tracking queryable for read-only operations.
    /// </summary>
    IQueryable<TEntity> QueryNoTracking(bool includeDeleted = false);

    #endregion
}

