using Ecom.Domain.Entities;
using Ecom.Infrastructure.Persistence.Database.UnitOfWork;
using Microsoft.EntityFrameworkCore;

namespace Ecom.IntegrationTests.PostgreSql;

[Collection(PostgreSqlCollection.Name)]
public sealed class BaseRepositoryPostgreSqlTests(PostgreSqlFixture fixture)
{
    [PostgreSqlFact]
    public async Task Exists_many_normalizes_duplicate_ids_and_rejects_empty_or_missing_sets()
    {
        await fixture.ResetDatabaseAsync();
        var role = CreateRole("EXISTS");

        await using (var context = fixture.CreateDbContext())
        {
            await context.Roles.AddAsync(role);
            await context.SaveChangesAsync();
        }

        await using var verificationContext = fixture.CreateDbContext();
        var repository = new BaseRepository<Role>(verificationContext);

        Assert.True(await repository.ExistsAsync([role.Id, role.Id]));
        Assert.False(await repository.ExistsAsync([]));
        Assert.False(await repository.ExistsAsync([role.Id, Guid.NewGuid()]));
    }

    [PostgreSqlFact]
    public async Task Soft_delete_is_hidden_by_default_and_visible_when_deleted_rows_are_included()
    {
        await fixture.ResetDatabaseAsync();
        var role = CreateRole("SOFT_DELETE");

        await using (var context = fixture.CreateDbContext())
        {
            context.Roles.Add(role);
            await context.SaveChangesAsync();

            var repository = new BaseRepository<Role>(context);
            Assert.True(await repository.DeleteAsync(role.Id));
            await context.SaveChangesAsync();
        }

        await using var verificationContext = fixture.CreateDbContext();
        var verificationRepository = new BaseRepository<Role>(verificationContext);

        Assert.Null(await verificationRepository.QueryNoTracking().SingleOrDefaultAsync(x => x.Id == role.Id));
        var deleted = await verificationRepository.QueryNoTracking(includeDeleted: true)
            .SingleAsync(x => x.Id == role.Id);
        Assert.True(deleted.IsDeleted);
        Assert.NotNull(deleted.DeletedAt);
    }

    [PostgreSqlFact]
    public async Task Hard_delete_removes_an_already_soft_deleted_row()
    {
        await fixture.ResetDatabaseAsync();
        var role = CreateRole("HARD_DELETE");

        await using (var context = fixture.CreateDbContext())
        {
            context.Roles.Add(role);
            await context.SaveChangesAsync();

            var repository = new BaseRepository<Role>(context);
            Assert.True(await repository.DeleteAsync(role.Id));
            await context.SaveChangesAsync();
        }

        await using (var deleteContext = fixture.CreateDbContext())
        {
            var repository = new BaseRepository<Role>(deleteContext);
            Assert.True(await repository.HardDeleteAsync(role.Id));
        }

        await using var verificationContext = fixture.CreateDbContext();
        Assert.False(await verificationContext.Roles.IgnoreQueryFilters().AnyAsync(x => x.Id == role.Id));
    }

    [PostgreSqlFact]
    public async Task PostgreSql_unique_constraint_rejects_duplicate_active_role_codes()
    {
        await fixture.ResetDatabaseAsync();

        await using var context = fixture.CreateDbContext();
        context.Roles.AddRange(CreateRole("DUPLICATE"), CreateRole("DUPLICATE"));

        await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync());
    }

    private static Role CreateRole(string code) => new()
    {
        Code = code,
        Name = $"Role {code}",
        IsActive = true
    };
}
