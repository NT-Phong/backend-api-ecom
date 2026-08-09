using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Ecom.Domain.Constants;
using Ecom.Domain.Entities;
using Ecom.Domain.Enums;
using Ecom.IntegrationTests.PostgreSql;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace Ecom.IntegrationTests.Catalog;

[Collection(PostgreSqlCollection.Name)]
public sealed class MediaCatalogApiTests(PostgreSqlFixture fixture)
{
    [PostgreSqlFact]
    public async Task Retry_scan_enforces_access_and_preserves_its_state_contract()
    {
        await fixture.ResetDatabaseAsync();
        var ownerId = Guid.NewGuid();
        var otherOwnerId = Guid.NewGuid();
        var now = DateTime.UtcNow;
        var failed = CreateAsset("failed.jpg", MediaVisibility.Restricted);
        failed.CreatedBy = ownerId;
        failed.MarkScanFailed(MediaScanFailureCodes.ScannerUnavailable, "Scanner unavailable", now);
        var pending = CreateAsset("pending.jpg", MediaVisibility.Restricted);
        pending.CreatedBy = ownerId;
        pending.ScheduleScanRetry(now.AddMinutes(2));
        var clean = CreateAsset("clean.jpg", MediaVisibility.Public);
        clean.CreatedBy = ownerId;
        clean.MarkClean(now);
        var rejected = CreateAsset("rejected.jpg", MediaVisibility.Restricted);
        rejected.CreatedBy = ownerId;
        rejected.Reject("Rejected", now);
        var foreign = CreateAsset("foreign.jpg", MediaVisibility.Restricted);
        foreign.CreatedBy = otherOwnerId;
        foreign.MarkScanFailed(MediaScanFailureCodes.ScannerUnavailable, "Scanner unavailable", now);
        await using (var context = fixture.CreateDbContext())
        {
            context.MediaAssets.AddRange(failed, pending, clean, rejected, foreign);
            await context.SaveChangesAsync();
        }

        await using var factory = new CatalogApiFactory(fixture);
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost"),
            HandleCookies = true
        });

        using var anonymous = await client.PostAsync($"/api/v1/media/{failed.Id}/retry-scan", null);
        Assert.Equal(HttpStatusCode.Unauthorized, anonymous.StatusCode);

        client.DefaultRequestHeaders.Authorization = new("Bearer", CreateAccessToken(ownerId, Permissions.CatalogProducts.Read));
        using var forbidden = await client.PostAsync($"/api/v1/media/{failed.Id}/retry-scan", null);
        Assert.Equal(HttpStatusCode.Forbidden, forbidden.StatusCode);

        client.DefaultRequestHeaders.Authorization = new("Bearer", CreateAccessToken(ownerId, Permissions.Media.Read));
        using var csrfMissing = await client.PostAsync($"/api/v1/media/{failed.Id}/retry-scan", null);
        Assert.Equal(HttpStatusCode.BadRequest, csrfMissing.StatusCode);

        var csrf = await GetCsrfTokenAsync(client);
        client.DefaultRequestHeaders.Add("X-CSRF-TOKEN", csrf);

        using var retry = await client.PostAsync($"/api/v1/media/{failed.Id}/retry-scan", null);
        Assert.Equal(HttpStatusCode.OK, retry.StatusCode);
        using (var document = JsonDocument.Parse(await retry.Content.ReadAsStringAsync()))
        {
            var data = document.RootElement.GetProperty("data");
            Assert.Equal("Pending", data.GetProperty("scanStatus").GetString());
            Assert.False(data.GetProperty("canRetryScan").GetBoolean());
            Assert.Equal(JsonValueKind.Null, data.GetProperty("scanFailureCode").ValueKind);
            Assert.Equal(JsonValueKind.Null, data.GetProperty("nextScanAttemptAt").ValueKind);
        }

        using var pendingRetry = await client.PostAsync($"/api/v1/media/{pending.Id}/retry-scan", null);
        Assert.Equal(HttpStatusCode.OK, pendingRetry.StatusCode);
        using (var document = JsonDocument.Parse(await pendingRetry.Content.ReadAsStringAsync()))
        {
            var data = document.RootElement.GetProperty("data");
            Assert.Equal("Pending", data.GetProperty("scanStatus").GetString());
            Assert.NotEqual(JsonValueKind.Null, data.GetProperty("nextScanAttemptAt").ValueKind);
        }

        using var cleanRetry = await client.PostAsync($"/api/v1/media/{clean.Id}/retry-scan", null);
        Assert.Equal(HttpStatusCode.BadRequest, cleanRetry.StatusCode);
        await AssertErrorAsync(cleanRetry, "MEDIA_SCAN_RETRY_INVALID");

        using var rejectedRetry = await client.PostAsync($"/api/v1/media/{rejected.Id}/retry-scan", null);
        Assert.Equal(HttpStatusCode.BadRequest, rejectedRetry.StatusCode);
        await AssertErrorAsync(rejectedRetry, "MEDIA_SCAN_RETRY_INVALID");

        using var foreignRetry = await client.PostAsync($"/api/v1/media/{foreign.Id}/retry-scan", null);
        Assert.Equal(HttpStatusCode.Forbidden, foreignRetry.StatusCode);

        client.DefaultRequestHeaders.Authorization = new("Bearer",
            CreateAccessToken(ownerId, Permissions.Media.Read, Permissions.Media.Manage));
        using var managerRetry = await client.PostAsync($"/api/v1/media/{foreign.Id}/retry-scan", null);
        Assert.Equal(HttpStatusCode.OK, managerRetry.StatusCode);
    }

    [PostgreSqlFact]
    public async Task Attach_media_checks_ownership_before_readiness_and_renews_the_product_stamp()
    {
        await fixture.ResetDatabaseAsync();
        var ownerId = Guid.NewGuid();
        var otherOwnerId = Guid.NewGuid();
        var now = DateTime.UtcNow;
        var producerId = Guid.NewGuid();
        var product = Product.Create(producerId, "Attach Product", $"attach-product-{Guid.NewGuid():N}");
        var pendingForeign = CreateAsset("foreign-pending.jpg", MediaVisibility.Restricted);
        pendingForeign.CreatedBy = otherOwnerId;
        var pendingOwned = CreateAsset("owned-pending.jpg", MediaVisibility.Restricted);
        pendingOwned.CreatedBy = ownerId;
        var failedOwned = CreateAsset("owned-failed.jpg", MediaVisibility.Restricted);
        failedOwned.CreatedBy = ownerId;
        failedOwned.MarkScanFailed(MediaScanFailureCodes.ScannerUnavailable, "Scanner unavailable", now);
        var rejectedOwned = CreateAsset("owned-rejected.jpg", MediaVisibility.Restricted);
        rejectedOwned.CreatedBy = ownerId;
        rejectedOwned.Reject("Rejected", now);
        var cleanRestrictedOwned = CreateAsset("owned-clean-restricted.jpg", MediaVisibility.Restricted);
        cleanRestrictedOwned.CreatedBy = ownerId;
        cleanRestrictedOwned.MarkClean(now);
        var cleanOwned = CreateAsset("owned-clean.jpg", MediaVisibility.Public);
        cleanOwned.CreatedBy = ownerId;
        cleanOwned.MarkClean(now);
        await using (var context = fixture.CreateDbContext())
        {
            await context.Database.ExecuteSqlInterpolatedAsync($"""
                INSERT INTO "Tbl_Producer" ("Id", "Code", "Name", "PublicStatus", "IsVerified", "CreatedAt", "IsDeleted", "ConcurrencyStamp")
                VALUES ({producerId}, {"ATTACH_TEST"}, {"Attach Test Producer"}, {"Draft"}, {false}, {now}, {false}, {Guid.NewGuid()});
                """);
            context.Products.Add(product);
            context.MediaAssets.AddRange(pendingForeign, pendingOwned, failedOwned, rejectedOwned,
                cleanRestrictedOwned, cleanOwned);
            await context.SaveChangesAsync();
        }

        await using var factory = new CatalogApiFactory(fixture);
        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new("Bearer", CreateAccessToken(ownerId, Permissions.CatalogProducts.Update));

        using var foreign = await AttachAsync(client, product.Id, product.ConcurrencyStamp, pendingForeign.Id);
        Assert.Equal(HttpStatusCode.Forbidden, foreign.StatusCode);

        foreach (var notReady in new[] { pendingOwned, failedOwned, rejectedOwned, cleanRestrictedOwned })
        {
            using var response = await AttachAsync(client, product.Id, product.ConcurrencyStamp, notReady.Id);
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            await AssertErrorAsync(response, "MEDIA_NOT_READY");
        }

        using var attached = await AttachAsync(client, product.Id, product.ConcurrencyStamp, cleanOwned.Id);
        Assert.Equal(HttpStatusCode.OK, attached.StatusCode);
        Guid renewedStamp;
        using (var document = JsonDocument.Parse(await attached.Content.ReadAsStringAsync()))
        {
            renewedStamp = document.RootElement.GetProperty("data").GetProperty("concurrencyStamp").GetGuid();
        }
        Assert.NotEqual(product.ConcurrencyStamp, renewedStamp);

        using var stale = await AttachAsync(client, product.Id, product.ConcurrencyStamp, cleanOwned.Id);
        Assert.Equal(HttpStatusCode.Conflict, stale.StatusCode);
    }

    private static MediaAsset CreateAsset(string fileName, MediaVisibility visibility) =>
        MediaAsset.CreatePending($"quarantine/{fileName}", fileName, "image/jpeg", 128, MediaType.Image, visibility);

    private static async Task<HttpResponseMessage> AttachAsync(HttpClient client, Guid productId, Guid stamp, Guid mediaAssetId) =>
        await client.PostAsJsonAsync($"/api/v1/catalog/products/{productId}/media", new
        {
            concurrencyStamp = stamp,
            mediaAssetId,
            displayOrder = 0,
            makePrimary = true,
            caption = "Product image"
        });

    private static async Task<string> GetCsrfTokenAsync(HttpClient client)
    {
        using var response = await client.GetAsync("/api/v1/security/csrf");
        response.EnsureSuccessStatusCode();
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        return document.RootElement.GetProperty("data").GetProperty("token").GetString()!;
    }

    private static async Task AssertErrorAsync(HttpResponseMessage response, string message)
    {
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal(message, document.RootElement.GetProperty("message").GetString());
    }

    private static string CreateAccessToken(Guid userId, params string[] permissions)
    {
        var claims = permissions.Select(permission => new Claim("policy", permission))
            .Append(new Claim(ClaimTypes.NameIdentifier, userId.ToString()));
        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(CatalogApiFactory.JwtSecret)),
            SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            CatalogApiFactory.JwtIssuer,
            CatalogApiFactory.JwtAudience,
            claims,
            notBefore: DateTime.UtcNow.AddMinutes(-1),
            expires: DateTime.UtcNow.AddMinutes(5),
            signingCredentials: credentials);
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
