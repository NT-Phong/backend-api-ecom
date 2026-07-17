using System.Linq.Expressions;
using Ecom.Application.Common.Extensions;
using Ecom.Domain.Models;

namespace Ecom.Infrastructure.Persistence.Database.UnitOfWork;

public sealed class BaseRepository<TEntity> : IBaseRepository<TEntity> where TEntity : BaseEntity
{
    private readonly ApplicationDbContext _context;
    private readonly DbSet<TEntity> _dbSet;

    public BaseRepository(ApplicationDbContext context)
    {
        _context = context;
        _dbSet = context.Set<TEntity>();
    }

    #region Query Methods

    public async Task<List<TEntity>> FindAsync(
        Expression<Func<TEntity, bool>>[]? filters = null,
        string? orderBy = null,
        int skip = 0,
        int limit = 0,
        params Expression<Func<TEntity, object>>[]? includes)
    {
        var query = BuildQuery(filters, orderBy, skip, limit, includes);
        return await query.ToListAsync();
    }

    public async Task<List<TDto>> FindAsync<TDto>(
        Expression<Func<TEntity, bool>>[]? filters = null,
        string? orderBy = null,
        int skip = 0,
        int limit = 0,
        params Expression<Func<TEntity, object>>[]? includes)
    {
        var entities = await FindAsync(filters, orderBy, skip, limit, includes);
        return entities.ProjectTo<TEntity, TDto>();
    }

    public async Task<int> CountAsync(Expression<Func<TEntity, bool>>[]? filters = null)
    {
        var query = Query();
        query = ApplyFilters(query, filters);
        return await query.CountAsync();
    }

    public async Task<bool> AnyAsync(Expression<Func<TEntity, bool>>[]? filters = null)
    {
        var query = Query();
        query = ApplyFilters(query, filters);
        return await query.AnyAsync();
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        return await _dbSet.AnyAsync(x => x.Id == id && !x.IsDeleted);
    }

    public async Task<bool> ExistsAsync(IEnumerable<Guid> ids)
    {
        var idList = ids.ToList();
        var count = await _dbSet.CountAsync(x => !x.IsDeleted && idList.Contains(x.Id));
        return count == idList.Count;
    }
    public async Task<FindResult<TEntity>> FindResultAsync(
        Expression<Func<TEntity, bool>>[]? filters = null,
        string? orderBy = null,
        int skip = 0,
        int limit = 0,
        Expression<Func<TEntity, object>>[]? includes = null)
    {
        IQueryable<TEntity> query = _dbSet;

        if (includes != null && includes.Any())
            query = includes.Aggregate(query, (current, include) => current.Include(include));

        query = query.Where(x => !x.IsDeleted);

        if (filters != null && filters.Any())
            query = filters.Aggregate(query, (current, filter) => current.Where(filter));

        if (!string.IsNullOrEmpty(orderBy))
            query = ApplyOrderBy(query, orderBy);

        var totalCount = await query.LongCountAsync();

        if (skip > 0) query = query.Skip(skip);
        if (limit > 0) query = query.Take(limit);

        var items = await query.ToListAsync();

        return FindResult<TEntity>.Success(items, totalCount);
    }

    #endregion

    #region FindOne Methods

    public async Task<TEntity?> FindByIdAsync(Guid id, params Expression<Func<TEntity, object>>[] includes)
    {
        var query = Query();
        query = ApplyIncludes(query, includes);
        return await query.FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<TDto?> FindByIdAsync<TDto>(Guid id, params Expression<Func<TEntity, object>>[] includes)
    {
        var entity = await FindByIdAsync(id, includes);
        return entity == null ? default : entity.ProjectTo<TEntity, TDto>();
    }

    public async Task<TEntity?> FindOneAsync(
        Expression<Func<TEntity, bool>>[]? filters = null,
        string? orderBy = null,
        params Expression<Func<TEntity, object>>[]? includes)
    {
        var query = Query();
        query = ApplyFilters(query, filters);
        query = ApplyIncludes(query, includes);
        query = ApplyOrderBy(query, orderBy);
        return await query.FirstOrDefaultAsync();
    }

    public async Task<TDto?> FindOneAsync<TDto>(
        Expression<Func<TEntity, bool>>[]? filters = null,
        string? orderBy = null,
        params Expression<Func<TEntity, object>>[]? includes)
    {
        var entity = await FindOneAsync(filters, orderBy, includes);
        return entity == null ? default : entity.ProjectTo<TEntity, TDto>();
    }

    #endregion

    #region CUD Operations

    public async Task<TEntity> InsertAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        await _dbSet.AddAsync(entity, cancellationToken);
        return entity;
    }

    public async Task InsertRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
    {
        await _dbSet.AddRangeAsync(entities, cancellationToken);
    }

    public Task<TEntity> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        _context.Entry(entity).State = EntityState.Modified;
        return Task.FromResult(entity);
    }

    public Task UpdateRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
    {
        foreach (var entity in entities)
        {
            _context.Entry(entity).State = EntityState.Modified;
        }
        return Task.CompletedTask;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _dbSet.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, cancellationToken);
        if (entity == null) return false;

        return await DeleteAsync(entity, cancellationToken);
    }

    public Task<bool> DeleteAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        _dbSet.Remove(entity);
        return Task.FromResult(true);
    }

    public Task DeleteRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
    {
        _dbSet.RemoveRange(entities);
        return Task.CompletedTask;
    }

    public async Task<bool> HardDeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(x => x.Id == id)
            .ExecuteDeleteAsync(cancellationToken) > 0;
    }

    public async Task HardDeleteRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
    {
        var ids = entities.Select(entity => entity.Id).ToList();
        if (ids.Count > 0)
        {
            await _dbSet.Where(entity => ids.Contains(entity.Id)).ExecuteDeleteAsync(cancellationToken);
        }
    }

    #endregion

    #region Query Builders

    public IQueryable<TEntity> Query(bool includeDeleted = false)
    {
        return includeDeleted ? _dbSet.AsQueryable() : _dbSet.Where(x => !x.IsDeleted);
    }

    public IQueryable<TEntity> QueryNoTracking(bool includeDeleted = false)
    {
        return Query(includeDeleted).AsNoTracking();
    }

    #endregion

    #region Protected Helpers

    private IQueryable<TEntity> BuildQuery(
        Expression<Func<TEntity, bool>>[]? filters,
        string? orderBy,
        int skip,
        int limit,
        Expression<Func<TEntity, object>>[]? includes)
    {
        var query = Query();
        query = ApplyFilters(query, filters);
        query = ApplyIncludes(query, includes);
        query = ApplyOrderBy(query, orderBy);
        query = ApplyPaging(query, skip, limit);
        return query;
    }

    private static IQueryable<TEntity> ApplyFilters(IQueryable<TEntity> query, Expression<Func<TEntity, bool>>[]? filters)
    {
        if (filters == null || filters.Length == 0) return query;
        return filters.Aggregate(query, (current, filter) => current.Where(filter));
    }

    private static IQueryable<TEntity> ApplyIncludes(IQueryable<TEntity> query, Expression<Func<TEntity, object>>[]? includes)
    {
        if (includes == null || includes.Length == 0) return query;
        return includes.Aggregate(query, (current, include) => current.Include(include));
    }

    private static IQueryable<TEntity> ApplyOrderBy(IQueryable<TEntity> query, string? orderBy)
    {
        if (string.IsNullOrWhiteSpace(orderBy)) return query;

        var parts = orderBy.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var propertyName = parts[0];
        var isDescending = parts.Length > 1 && parts[1].Equals("desc", StringComparison.OrdinalIgnoreCase);

        // Convert first letter to uppercase for property matching
        propertyName = char.ToUpper(propertyName[0]) + propertyName[1..];

        return isDescending
            ? query.OrderByDescending(e => EF.Property<object>(e, propertyName))
            : query.OrderBy(e => EF.Property<object>(e, propertyName));
    }

    private static IQueryable<TEntity> ApplyPaging(IQueryable<TEntity> query, int skip, int limit)
    {
        if (skip > 0) query = query.Skip(skip);
        if (limit > 0) query = query.Take(limit);
        return query;
    }

    private static IQueryable<TEntity> ApplyDateFilters<TQuery>(IQueryable<TEntity> query, TQuery queryDto) where TQuery : BaseQueryDto
    {
        if (queryDto.CreatedAt is not null)
        {
            var dateQuery = queryDto.CreatedAt.DatetimeQuery();
            if (dateQuery != null)
            {
                query = query.Where(x => x.CreatedAt >= dateQuery.StartDateAt && x.CreatedAt <= dateQuery.EndDateAt);
            }
        }

        if (queryDto.CreateAtFrom != null && queryDto.CreateAtFrom != default)
        {
            var dateQuery = queryDto.CreateAtFrom.DatetimeQuery();
            if (dateQuery != null)
            {
                query = query.Where(x => x.CreatedAt >= dateQuery.StartDateAt);
            }
        }

        if (queryDto.CreateAtTo != null && queryDto.CreateAtTo != default)
        {
            var dateQuery = queryDto.CreateAtTo.DatetimeQuery();
            if (dateQuery != null)
            {
                query = query.Where(x => x.CreatedAt <= dateQuery.EndDateAt);
            }
        }

        return query;
    }

    #endregion
}

