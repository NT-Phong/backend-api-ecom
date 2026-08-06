using Ecom.Application.Common.Interfaces;
using Ecom.Domain.Entities;
using Ecom.Domain.Interfaces.Services;
using Ecom.Infrastructure.Persistence.Database;
using Ecom.Infrastructure.Persistence.Database.UnitOfWork;
using Microsoft.EntityFrameworkCore;

namespace Ecom.IntegrationTests.Persistence;

public sealed class BaseRepositoryQueryTests
{
    [Fact]
    public void Include_deleted_query_ignores_the_global_soft_delete_filter()
    {
        using var context = CreateContext();
        var repository = new BaseRepository<Role>(context);

        var activeOnlySql = repository.Query().ToQueryString();
        var includeDeletedSql = repository.Query(includeDeleted: true).ToQueryString();

        Assert.Contains("WHERE", activeOnlySql, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("WHERE", includeDeletedSql, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Include_deleted_no_tracking_query_ignores_the_global_soft_delete_filter()
    {
        using var context = CreateContext();
        var repository = new BaseRepository<Role>(context);

        var includeDeletedSql = repository.QueryNoTracking(includeDeleted: true).ToQueryString();

        Assert.DoesNotContain("WHERE", includeDeletedSql, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(QueryTrackingBehavior.TrackAll, context.ChangeTracker.QueryTrackingBehavior);
    }

    private static ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql("Host=localhost;Database=ecom_query_generation;Username=test;Password=test")
            .Options;

        return new ApplicationDbContext(options, new TestCurrentUser(), new TestDateTimeService(),
            new TestConnectionService());
    }

    private sealed class TestConnectionService : IConnectionService
    {
        private const string ConnectionString =
            "Host=localhost;Database=ecom_query_generation;Username=test;Password=test";

        public string GetReadConnectionString() => ConnectionString;
        public string GetWriteConnectionString() => ConnectionString;
    }

    private sealed class TestDateTimeService : IDateTimeService
    {
        public DateTimeOffset UtcNow => DateTimeOffset.UnixEpoch;
        public DateTimeOffset Now => DateTimeOffset.UnixEpoch;
        public DateOnly Today => DateOnly.FromDateTime(DateTime.UnixEpoch);
        public TimeOnly TimeNow => TimeOnly.MinValue;
    }

    private sealed class TestCurrentUser : ICurrentUser
    {
        public Guid UserId => Guid.Empty;
        public string? UserIdString => null;
        public string? PhoneNumber => null;
        public string? Email => null;
        public bool IsAuthenticated => false;
        public string? Role => null;
        public IEnumerable<string> Roles => [];
        public IEnumerable<string> Policies => [];
        public Guid SessionId => Guid.Empty;
        public string? SecurityStamp => null;
        public bool HasRole(string role) => false;
        public bool HasPolicy(string policy) => false;
    }
}
