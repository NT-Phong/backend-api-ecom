using Ecom.Domain.Entities;
using Ecom.Infrastructure.Persistence.Database.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace Ecom.IntegrationTests.PostgreSql;

[Collection(PostgreSqlCollection.Name)]
public sealed class UnitOfWorkPostgreSqlTests(PostgreSqlFixture fixture)
{
    [PostgreSqlFact]
    public async Task Commit_persists_all_writes_in_one_transaction()
    {
        await fixture.ResetDatabaseAsync();

        await using (var context = fixture.CreateDbContext())
        using (var unitOfWork = CreateUnitOfWork(context))
        {
            Assert.True(await unitOfWork.BeginTransactionAsync());
            await unitOfWork.Repository<Role>().InsertRangeAsync([
                CreateRole("COMMIT_A"),
                CreateRole("COMMIT_B")
            ]);
            await unitOfWork.CommitTransactionAsync();
        }

        await using var verificationContext = fixture.CreateDbContext();
        Assert.Equal(2, await verificationContext.Roles.CountAsync());
    }

    [PostgreSqlFact]
    public async Task Rollback_removes_multiple_writes_already_flushed_to_PostgreSql()
    {
        await fixture.ResetDatabaseAsync();

        await using (var context = fixture.CreateDbContext())
        using (var unitOfWork = CreateUnitOfWork(context))
        {
            Assert.True(await unitOfWork.BeginTransactionAsync());
            await unitOfWork.Repository<Role>().InsertRangeAsync([
                CreateRole("ROLLBACK_A"),
                CreateRole("ROLLBACK_B")
            ]);
            await unitOfWork.SaveChangesAsync();
            await unitOfWork.RollbackTransactionAsync();
            unitOfWork.ClearChangeTracker();
        }

        await using var verificationContext = fixture.CreateDbContext();
        Assert.Empty(await verificationContext.Roles.ToListAsync());
    }

    [PostgreSqlFact]
    public async Task Stale_concurrency_stamp_is_rejected_by_PostgreSql()
    {
        await fixture.ResetDatabaseAsync();
        var role = CreateRole("CONCURRENCY");

        await using (var seedContext = fixture.CreateDbContext())
        {
            seedContext.Roles.Add(role);
            await seedContext.SaveChangesAsync();
        }

        await using var firstContext = fixture.CreateDbContext();
        await using var staleContext = fixture.CreateDbContext();
        var first = await firstContext.Roles.SingleAsync(x => x.Id == role.Id);
        var stale = await staleContext.Roles.SingleAsync(x => x.Id == role.Id);

        first.Name = "First writer";
        await firstContext.SaveChangesAsync();

        stale.Name = "Stale writer";
        await Assert.ThrowsAsync<DbUpdateConcurrencyException>(() => staleContext.SaveChangesAsync());
    }

    private static UnitOfWork CreateUnitOfWork(Ecom.Infrastructure.Persistence.Database.ApplicationDbContext context) =>
        new(context, NullLogger<UnitOfWork>.Instance);

    private static Role CreateRole(string code) => new()
    {
        Code = code,
        Name = $"Role {code}",
        IsActive = true
    };
}
