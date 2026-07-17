namespace Ecom.Infrastructure.Persistence.Database;

public static class DbContextExtensions
{
    /// <summary>
    /// Configure DbContext for read operations with optimizations
    /// </summary>
    public static IQueryable<T> AsReadOnly<T>(this DbSet<T> dbSet) where T : class
    {
        return dbSet
            .AsNoTracking()
            .AsSplitQuery(); // Better performance for complex queries with includes
    }
    
    /// <summary>
    /// Configure DbContext for read operations with specific tracking behavior
    /// </summary>
    public static IQueryable<T> AsReadOnlyQuery<T>(this DbSet<T> dbSet, bool splitQuery = true) where T : class
    {
        var query = dbSet.AsNoTracking();
        
        if (splitQuery)
        {
            query = query.AsSplitQuery();
        }
        
        return query;
    }
}
