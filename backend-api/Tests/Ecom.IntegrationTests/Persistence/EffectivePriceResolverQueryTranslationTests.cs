using Ecom.Application.Common.Interfaces;
using Ecom.Application.Common.Services;
using Ecom.Domain.Entities;
using Ecom.Domain.Interfaces.Services;
using Ecom.Infrastructure.Persistence.Database;
using Ecom.Infrastructure.Persistence.Database.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace Ecom.IntegrationTests.Persistence;

public sealed class EffectivePriceResolverQueryTranslationTests
{
    [Fact]
    public void Effective_product_price_query_can_be_composed_as_a_catalog_exists_filter()
    {
        using var context = CreateContext();
        var unitOfWork = new UnitOfWork(context, NullLogger<UnitOfWork>.Instance);
        var resolver = new EffectivePriceResolver(unitOfWork);
        var effectivePrices = resolver.QueryEffectiveProductPrices(DateTime.UtcNow);

        var catalogQuery = unitOfWork.Repository<Product>().QueryNoTracking()
            .Where(product => effectivePrices.Any(price =>
                price.ProductId == product.Id && price.Amount >= 1m));

        var sql = catalogQuery.ToQueryString();

        Assert.Contains("EXISTS", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("NOT EXISTS", sql, StringComparison.OrdinalIgnoreCase);
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
