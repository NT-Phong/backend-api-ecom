using Ecom.Domain.Entities;
using Ecom.Domain.Enums;
using Ecom.IntegrationTests.Catalog;
using Ecom.IntegrationTests.PostgreSql;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using CommerceCart = Ecom.Domain.Entities.Cart;

namespace Ecom.IntegrationTests.Cart;

[Collection(PostgreSqlCollection.Name)]
public sealed class CartMergeApiTests(PostgreSqlFixture fixture)
{
    [PostgreSqlFact]
    public async Task Merge_guest_cart_requires_an_authenticated_user()
    {
        await fixture.ResetDatabaseAsync();
        await using var factory = new CatalogApiFactory(fixture);
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost"),
            HandleCookies = false
        });
        using var response = await SendMergeAsync(client, "unauthenticated-guest-cart");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.DoesNotContain(GetSetCookies(response), cookie => cookie.StartsWith("__Host-ecom_cart=;", StringComparison.OrdinalIgnoreCase));
    }

    [PostgreSqlFact]
    public async Task Merge_guest_cart_with_missing_csrf_returns_structured_error_without_clearing_guest_cookie()
    {
        await fixture.ResetDatabaseAsync();
        var seeded = await SeedGuestCartAsync("merge-missing-csrf", DateTime.UtcNow.AddDays(1), includeExistingUserCart: false);

        await using var factory = new CatalogApiFactory(fixture);
        using var client = CreateClient(factory, seeded.UserId);
        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/cart/merge-guest")
        {
            Content = new StringContent("{}", Encoding.UTF8, "application/json")
        };
        request.Headers.Add("Cookie", "__Host-ecom_cart=merge-missing-csrf");
        using var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.True(response.Headers.Contains("X-Trace-Id"));
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal("CSRF_INVALID", document.RootElement.GetProperty("errorCode").GetString());
        Assert.DoesNotContain(GetSetCookies(response), cookie => cookie.StartsWith("__Host-ecom_cart=;", StringComparison.OrdinalIgnoreCase));

        await using var context = fixture.CreateDbContext();
        Assert.Equal(CartStatus.Active, (await context.Carts.SingleAsync(x => x.Id == seeded.GuestCartId)).Status);
        Assert.False(await context.Carts.AnyAsync(x => x.UserId == seeded.UserId));
    }

    [PostgreSqlFact]
    public async Task Merge_missing_guest_cart_returns_404_without_creating_user_cart_or_clearing_cookie()
    {
        await fixture.ResetDatabaseAsync();
        var userId = await SeedUserAsync();

        await using var factory = new CatalogApiFactory(fixture);
        using var client = CreateClient(factory, userId);
        using var response = await SendMergeAsync(client, "missing-guest-cart");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.DoesNotContain(GetSetCookies(response), cookie => cookie.StartsWith("__Host-ecom_cart=;", StringComparison.OrdinalIgnoreCase));
        await using var context = fixture.CreateDbContext();
        Assert.False(await context.Carts.AnyAsync(x => x.UserId == userId));
    }

    [PostgreSqlFact]
    public async Task Merge_guest_cart_creates_active_user_cart_and_clears_cookie_after_success()
    {
        await fixture.ResetDatabaseAsync();
        const string guestToken = "merge-guest-create-cart";
        var seeded = await SeedGuestCartAsync(guestToken, DateTime.UtcNow.AddDays(1), includeExistingUserCart: false);

        await using var factory = new CatalogApiFactory(fixture);
        using var client = CreateClient(factory, seeded.UserId);
        using var response = await SendMergeAsync(client, guestToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True((await ReadDataAsync(response)).TryGetProperty("items", out var items));
        Assert.Equal(2, items.GetArrayLength());
        Assert.Contains(GetSetCookies(response), cookie => cookie.StartsWith("__Host-ecom_cart=;", StringComparison.OrdinalIgnoreCase));

        await using var context = fixture.CreateDbContext();
        var source = await context.Carts.SingleAsync(x => x.Id == seeded.GuestCartId);
        Assert.Equal(CartStatus.Converted, source.Status);
        var target = await context.Carts.SingleAsync(x => x.UserId == seeded.UserId && x.Status == CartStatus.Active);
        var targetItems = await context.CartItems.Where(x => x.CartId == target.Id).OrderBy(x => x.ProductVariantId).ToListAsync();
        Assert.Equal(2, targetItems.Count);
        Assert.Contains(targetItems, x => x.ProductVariantId == seeded.FirstVariantId && x.Quantity == 2);
        Assert.Contains(targetItems, x => x.ProductVariantId == seeded.SecondVariantId && x.Quantity == 1);
    }

    [PostgreSqlFact]
    public async Task Merge_guest_cart_combines_duplicate_variants_with_existing_user_cart()
    {
        await fixture.ResetDatabaseAsync();
        const string guestToken = "merge-guest-existing-cart";
        var seeded = await SeedGuestCartAsync(guestToken, DateTime.UtcNow.AddDays(1), includeExistingUserCart: true);

        await using var factory = new CatalogApiFactory(fixture);
        using var client = CreateClient(factory, seeded.UserId);
        using var response = await SendMergeAsync(client, guestToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        await using var context = fixture.CreateDbContext();
        var activeCarts = await context.Carts.Where(x => x.UserId == seeded.UserId && x.Status == CartStatus.Active).ToListAsync();
        var target = Assert.Single(activeCarts);
        var targetItems = await context.CartItems.Where(x => x.CartId == target.Id).OrderBy(x => x.ProductVariantId).ToListAsync();
        Assert.Equal(2, targetItems.Count);
        Assert.Contains(targetItems, x => x.ProductVariantId == seeded.FirstVariantId && x.Quantity == 5);
        Assert.Contains(targetItems, x => x.ProductVariantId == seeded.SecondVariantId && x.Quantity == 1);
        Assert.Equal(CartStatus.Converted, (await context.Carts.SingleAsync(x => x.Id == seeded.GuestCartId)).Status);
    }

    [PostgreSqlFact]
    public async Task Merge_guest_cart_retry_after_commit_is_successful_without_duplicate_quantities()
    {
        await fixture.ResetDatabaseAsync();
        const string guestToken = "merge-guest-retry";
        var seeded = await SeedGuestCartAsync(guestToken, DateTime.UtcNow.AddDays(1), includeExistingUserCart: false);

        await using var factory = new CatalogApiFactory(fixture);
        using var client = CreateClient(factory, seeded.UserId);
        using var first = await SendMergeAsync(client, guestToken);
        using var retry = await SendMergeAsync(client, guestToken);

        Assert.Equal(HttpStatusCode.OK, first.StatusCode);
        Assert.Equal(HttpStatusCode.OK, retry.StatusCode);

        await using var context = fixture.CreateDbContext();
        var target = await context.Carts.SingleAsync(x => x.UserId == seeded.UserId && x.Status == CartStatus.Active);
        var targetItems = await context.CartItems.Where(x => x.CartId == target.Id).ToListAsync();
        Assert.Contains(targetItems, x => x.ProductVariantId == seeded.FirstVariantId && x.Quantity == 2);
        Assert.Contains(targetItems, x => x.ProductVariantId == seeded.SecondVariantId && x.Quantity == 1);
        Assert.Equal(2, targetItems.Count);
    }

    [PostgreSqlFact]
    public async Task Concurrent_merge_requests_create_one_active_cart_and_do_not_duplicate_items()
    {
        await fixture.ResetDatabaseAsync();
        const string guestToken = "merge-guest-concurrent";
        var seeded = await SeedGuestCartAsync(guestToken, DateTime.UtcNow.AddDays(1), includeExistingUserCart: false);

        await using var factory = new CatalogApiFactory(fixture);
        using var firstClient = CreateClient(factory, seeded.UserId);
        using var secondClient = CreateClient(factory, seeded.UserId);
        var responses = await Task.WhenAll(SendMergeAsync(firstClient, guestToken), SendMergeAsync(secondClient, guestToken));
        try
        {
            Assert.All(responses, response => Assert.Equal(HttpStatusCode.OK, response.StatusCode));
        }
        finally
        {
            foreach (var response in responses)
                response.Dispose();
        }

        await using var context = fixture.CreateDbContext();
        var target = Assert.Single(await context.Carts.Where(x => x.UserId == seeded.UserId && x.Status == CartStatus.Active).ToListAsync());
        var targetItems = await context.CartItems.Where(x => x.CartId == target.Id).ToListAsync();
        Assert.Contains(targetItems, x => x.ProductVariantId == seeded.FirstVariantId && x.Quantity == 2);
        Assert.Contains(targetItems, x => x.ProductVariantId == seeded.SecondVariantId && x.Quantity == 1);
        Assert.Equal(2, targetItems.Count);
        Assert.Equal(CartStatus.Converted, (await context.Carts.SingleAsync(x => x.Id == seeded.GuestCartId)).Status);
    }

    [PostgreSqlFact]
    public async Task Merge_expired_guest_cart_returns_422_and_keeps_guest_cookie()
    {
        await fixture.ResetDatabaseAsync();
        const string guestToken = "merge-expired-guest-cart";
        var userId = await SeedExpiredGuestCartAsync(guestToken);

        await using var factory = new CatalogApiFactory(fixture);
        using var client = CreateClient(factory, userId);
        using var response = await SendMergeAsync(client, guestToken);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        Assert.DoesNotContain(GetSetCookies(response), cookie => cookie.StartsWith("__Host-ecom_cart=;", StringComparison.OrdinalIgnoreCase));
    }

    private async Task<MergeSeed> SeedGuestCartAsync(string guestToken, DateTime expiresAt, bool includeExistingUserCart)
    {
        var user = new User($"090{Random.Shared.Next(1000000, 9999999)}", null);
        var producer = Producer.Create($"MERGE-{Guid.NewGuid():N}", "Merge test producer", null, null, null);
        var product = Product.Create(producer.Id, "Merge test product", $"merge-product-{Guid.NewGuid():N}");
        var firstVariant = ProductVariant.Create(product.Id, $"MERGE-A-{Guid.NewGuid():N}", "Variant A", InventoryMode.NotTracked);
        var secondVariant = ProductVariant.Create(product.Id, $"MERGE-B-{Guid.NewGuid():N}", "Variant B", InventoryMode.NotTracked);
        var guestCart = CommerceCart.CreateForGuest(Hash(guestToken), expiresAt);
        var guestItems = new List<CartItem>();
        guestCart.AddItem(guestItems, firstVariant.Id, 2);
        guestCart.AddItem(guestItems, secondVariant.Id, 1);

        CommerceCart? userCart = null;
        var userItems = new List<CartItem>();
        if (includeExistingUserCart)
        {
            userCart = CommerceCart.CreateForUser(user.Id);
            userCart.AddItem(userItems, firstVariant.Id, 3);
        }

        await using var context = fixture.CreateDbContext();
        context.Users.Add(user);
        context.Producers.Add(producer);
        context.Products.Add(product);
        context.ProductVariants.AddRange(firstVariant, secondVariant);
        context.Carts.Add(guestCart);
        context.CartItems.AddRange(guestItems);
        if (userCart is not null)
        {
            context.Carts.Add(userCart);
            context.CartItems.AddRange(userItems);
        }
        await context.SaveChangesAsync();

        return new MergeSeed(user.Id, guestCart.Id, firstVariant.Id, secondVariant.Id);
    }

    private async Task<Guid> SeedExpiredGuestCartAsync(string guestToken)
    {
        var user = new User($"090{Random.Shared.Next(1000000, 9999999)}", null);
        var guestCart = CommerceCart.CreateForGuest(Hash(guestToken), DateTime.UtcNow.AddMinutes(-1));
        await using var context = fixture.CreateDbContext();
        context.Users.Add(user);
        context.Carts.Add(guestCart);
        await context.SaveChangesAsync();
        return user.Id;
    }

    private async Task<Guid> SeedUserAsync()
    {
        var user = new User($"090{Random.Shared.Next(1000000, 9999999)}", null);
        await using var context = fixture.CreateDbContext();
        context.Users.Add(user);
        await context.SaveChangesAsync();
        return user.Id;
    }

    private static HttpClient CreateClient(CatalogApiFactory factory, Guid userId)
    {
        var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost"),
            HandleCookies = false
        });
        client.DefaultRequestHeaders.Authorization = new("Bearer", CreateAccessToken(userId));
        return client;
    }

    private static async Task<HttpResponseMessage> SendMergeAsync(HttpClient client, string guestToken)
    {
        using var csrfResponse = await client.GetAsync("/api/v1/security/csrf");
        csrfResponse.EnsureSuccessStatusCode();
        using var csrfDocument = JsonDocument.Parse(await csrfResponse.Content.ReadAsStringAsync());
        var csrfToken = csrfDocument.RootElement.GetProperty("data").GetProperty("token").GetString();
        var antiforgeryCookie = Assert.Single(GetSetCookies(csrfResponse)).Split(';', 2)[0];

        var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/cart/merge-guest")
        {
            Content = new StringContent("{}", Encoding.UTF8, "application/json")
        };
        request.Headers.Add("X-CSRF-TOKEN", csrfToken);
        request.Headers.Add("Cookie", $"{antiforgeryCookie}; __Host-ecom_cart={guestToken}");
        return await client.SendAsync(request);
    }

    private static async Task<JsonElement> ReadDataAsync(HttpResponseMessage response)
    {
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        return document.RootElement.GetProperty("data").Clone();
    }

    private static IEnumerable<string> GetSetCookies(HttpResponseMessage response) =>
        response.Headers.TryGetValues("Set-Cookie", out var values) ? values : [];

    private static string CreateAccessToken(Guid userId)
    {
        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, userId.ToString()) };
        var credentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(CatalogApiFactory.JwtSecret)),
            SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(CatalogApiFactory.JwtIssuer, CatalogApiFactory.JwtAudience, claims,
            notBefore: DateTime.UtcNow.AddMinutes(-1), expires: DateTime.UtcNow.AddMinutes(5), signingCredentials: credentials);
        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static string Hash(string value) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value))).ToLowerInvariant();

    private sealed record MergeSeed(Guid UserId, Guid GuestCartId, Guid FirstVariantId, Guid SecondVariantId);
}
