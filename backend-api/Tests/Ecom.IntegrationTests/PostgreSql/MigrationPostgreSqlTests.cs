using Ecom.Domain.Entities;
using Ecom.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Ecom.IntegrationTests.PostgreSql;

[Collection(PostgreSqlCollection.Name)]
public sealed class MigrationPostgreSqlTests(PostgreSqlFixture fixture)
{
    [PostgreSqlFact]
    public async Task Empty_schema_is_migrated_to_the_current_model()
    {
        await using var context = fixture.CreateDbContext();

        var migrations = context.Database.GetMigrations();
        var appliedMigrations = await context.Database.GetAppliedMigrationsAsync();

        Assert.Equal(migrations, appliedMigrations);

        await using var connection = new NpgsqlConnection(fixture.ConnectionString);
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText =
            "SELECT column_name FROM information_schema.columns "
            + "WHERE table_schema = @schema AND table_name = 'Tbl_MediaAsset' "
            + "AND column_name = ANY(@columns);";
        command.Parameters.AddWithValue("schema", fixture.SchemaName);
        command.Parameters.AddWithValue("columns", new[]
        {
            "NextScanAttemptAt",
            "ScanAttemptCount",
            "ScanLeaseExpiresAt",
            "ScanFailureCode",
            "Sha256",
            "ThumbnailStorageKey"
        });

        var columns = new List<string>();
        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync()) columns.Add(reader.GetString(0));

        Assert.Equal(6, columns.Count);
    }

    [PostgreSqlFact]
    public async Task Variant_price_exclusion_constraint_rejects_overlapping_active_periods()
    {
        await fixture.ResetDatabaseAsync();
        var producerId = Guid.NewGuid();
        var now = DateTime.UtcNow;

        await using var context = fixture.CreateDbContext();
        await context.Database.ExecuteSqlInterpolatedAsync($"""
            INSERT INTO "Tbl_Producer"
                ("Id", "Code", "Name", "PublicStatus", "IsVerified", "CreatedAt", "IsDeleted", "ConcurrencyStamp")
            VALUES
                ({producerId}, {"TEST_PRODUCER"}, {"Test Producer"}, {"Draft"}, {false}, {now}, {false}, {Guid.NewGuid()});
            """);

        var product = Product.Create(producerId, "Test Product", $"test-product-{Guid.NewGuid():N}");
        var variant = ProductVariant.Create(product.Id, $"SKU-{Guid.NewGuid():N}", "Default",
            InventoryMode.NotTracked);
        context.Products.Add(product);
        context.ProductVariants.Add(variant);
        await context.SaveChangesAsync();

        context.VariantPrices.Add(VariantPrice.Create(
            variant.Id,
            100_000m,
            PriceType.Public,
            now,
            now.AddDays(10)));
        await context.SaveChangesAsync();

        context.VariantPrices.Add(VariantPrice.Create(
            variant.Id,
            90_000m,
            PriceType.Public,
            now.AddDays(5),
            now.AddDays(15)));

        await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync());
    }

    [PostgreSqlFact]
    public async Task Product_media_constraint_rejects_two_active_primary_assets_for_one_product()
    {
        await fixture.ResetDatabaseAsync();
        var producerId = Guid.NewGuid();
        var now = DateTime.UtcNow;

        await using var context = fixture.CreateDbContext();
        await context.Database.ExecuteSqlInterpolatedAsync($"""
            INSERT INTO "Tbl_Producer"
                ("Id", "Code", "Name", "PublicStatus", "IsVerified", "CreatedAt", "IsDeleted", "ConcurrencyStamp")
            VALUES
                ({producerId}, {"MEDIA_TEST_PRODUCER"}, {"Media Test Producer"}, {"Draft"}, {false}, {now}, {false}, {Guid.NewGuid()});
            """);

        var product = Product.Create(producerId, "Media Constraint Product", $"media-constraint-{Guid.NewGuid():N}");
        var firstAsset = MediaAsset.CreatePending("quarantine/first.jpg", "first.jpg", "image/jpeg", 128,
            MediaType.Image, MediaVisibility.Public);
        var secondAsset = MediaAsset.CreatePending("quarantine/second.jpg", "second.jpg", "image/jpeg", 128,
            MediaType.Image, MediaVisibility.Public);
        firstAsset.MarkClean(now);
        secondAsset.MarkClean(now);

        var firstPrimary = product.AttachMedia([], firstAsset.Id, 0, true, firstAsset.IsPubliclyUsable);
        var secondPrimary = product.AttachMedia([], secondAsset.Id, 1, true, secondAsset.IsPubliclyUsable);

        context.Products.Add(product);
        context.MediaAssets.AddRange(firstAsset, secondAsset);
        context.ProductMedia.AddRange(firstPrimary, secondPrimary);

        await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync());
    }
}
