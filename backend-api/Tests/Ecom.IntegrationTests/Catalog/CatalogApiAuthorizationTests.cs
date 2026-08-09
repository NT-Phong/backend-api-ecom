using Ecom.Domain.Constants;
using Ecom.Domain.Entities;
using Ecom.Domain.Enums;
using Ecom.IntegrationTests.PostgreSql;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace Ecom.IntegrationTests.Catalog;

[Collection(PostgreSqlCollection.Name)]
public sealed class CatalogApiAuthorizationTests(PostgreSqlFixture fixture)
{
    [PostgreSqlFact]
    public async Task Public_catalog_endpoints_return_the_standard_success_envelope()
    {
        await fixture.ResetDatabaseAsync();
        await using var factory = new CatalogApiFactory(fixture);
        using var client = factory.CreateClient();

        using var products = await client.GetAsync("/api/v1/products");
        using var categories = await client.GetAsync("/api/v1/categories");

        Assert.Equal(HttpStatusCode.OK, products.StatusCode);
        Assert.Equal(HttpStatusCode.OK, categories.StatusCode);
        await AssertSuccessEnvelopeAsync(products);
        await AssertSuccessEnvelopeAsync(categories);
    }

    [PostgreSqlFact]
    public async Task Public_product_list_returns_only_a_product_with_published_catalog_facts_and_an_effective_price()
    {
        await fixture.ResetDatabaseAsync();
        var slug = await SeedPublishedProductAsync();
        await using var factory = new CatalogApiFactory(fixture);
        using var client = factory.CreateClient();

        using var response = await client.GetAsync("/api/v1/products");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var items = document.RootElement.GetProperty("data").GetProperty("items").EnumerateArray();
        Assert.Contains(items, item => string.Equals(item.GetProperty("slug").GetString(), slug, StringComparison.Ordinal));
    }

    [PostgreSqlFact]
    public async Task Public_category_list_hides_a_published_child_when_an_ancestor_is_not_published()
    {
        await fixture.ResetDatabaseAsync();
        var childSlug = await SeedCategoryWithPausedAncestorAsync();
        await using var factory = new CatalogApiFactory(fixture);
        using var client = factory.CreateClient();

        using var response = await client.GetAsync("/api/v1/categories");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var categories = document.RootElement.GetProperty("data").EnumerateArray();
        Assert.DoesNotContain(categories, category =>
            string.Equals(category.GetProperty("slug").GetString(), childSlug, StringComparison.Ordinal));
    }

    [PostgreSqlFact]
    public async Task Backoffice_catalog_endpoint_returns_401_without_credentials()
    {
        await fixture.ResetDatabaseAsync();
        await using var factory = new CatalogApiFactory(fixture);
        using var client = factory.CreateClient();

        using var response = await client.GetAsync("/api/v1/catalog/products");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [PostgreSqlFact]
    public async Task Backoffice_catalog_endpoint_returns_403_without_the_required_permission()
    {
        await fixture.ResetDatabaseAsync();
        await using var factory = new CatalogApiFactory(fixture);
        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new("Bearer", CreateAccessToken(Permissions.CatalogCategories.Read));

        using var response = await client.GetAsync("/api/v1/catalog/products");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [PostgreSqlFact]
    public async Task Backoffice_catalog_endpoint_allows_the_required_permission_and_preserves_the_success_envelope()
    {
        await fixture.ResetDatabaseAsync();
        await using var factory = new CatalogApiFactory(fixture);
        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new("Bearer", CreateAccessToken(Permissions.CatalogProducts.Read));

        using var response = await client.GetAsync("/api/v1/catalog/products");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        await AssertSuccessEnvelopeAsync(response);
    }

    [PostgreSqlFact]
    public async Task Producer_picker_requires_product_create_permission_and_returns_only_eligible_producers()
    {
        await fixture.ResetDatabaseAsync();
        var now = DateTime.UtcNow;
        await using (var context = fixture.CreateDbContext())
        {
            await context.Database.ExecuteSqlInterpolatedAsync($"""
                INSERT INTO "Tbl_Producer" ("Id", "Code", "Name", "PublicStatus", "IsVerified", "CreatedAt", "IsDeleted", "ConcurrencyStamp")
                VALUES
                    ({Guid.NewGuid()}, {"PICKER_OK"}, {"Eligible Producer"}, {"Published"}, {true}, {now}, {false}, {Guid.NewGuid()}),
                    ({Guid.NewGuid()}, {"PICKER_DRAFT"}, {"Draft Producer"}, {"Draft"}, {true}, {now}, {false}, {Guid.NewGuid()}),
                    ({Guid.NewGuid()}, {"PICKER_UNVERIFIED"}, {"Unverified Producer"}, {"Published"}, {false}, {now}, {false}, {Guid.NewGuid()});
                """);
        }

        await using var factory = new CatalogApiFactory(fixture);
        using var client = factory.CreateClient();

        using var unauthorized = await client.GetAsync("/api/v1/catalog/producers");
        Assert.Equal(HttpStatusCode.Unauthorized, unauthorized.StatusCode);

        client.DefaultRequestHeaders.Authorization = new("Bearer", CreateAccessToken(Permissions.CatalogProducts.Read));
        using var forbidden = await client.GetAsync("/api/v1/catalog/producers");
        Assert.Equal(HttpStatusCode.Forbidden, forbidden.StatusCode);

        client.DefaultRequestHeaders.Authorization = new("Bearer", CreateAccessToken(Permissions.CatalogProducts.Create));
        using var response = await client.GetAsync("/api/v1/catalog/producers?q=producer");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var items = document.RootElement.GetProperty("data").GetProperty("items").EnumerateArray().ToList();
        Assert.Single(items);
        Assert.Equal("PICKER_OK", items[0].GetProperty("code").GetString());
    }

    [PostgreSqlFact]
    public async Task Producer_picker_supports_paging_and_hides_ineligible_detail_records()
    {
        await fixture.ResetDatabaseAsync();
        var now = DateTime.UtcNow;
        var firstId = Guid.NewGuid();
        var secondId = Guid.NewGuid();
        var draftId = Guid.NewGuid();
        await using (var context = fixture.CreateDbContext())
        {
            await context.Database.ExecuteSqlInterpolatedAsync($"""
                INSERT INTO "Tbl_Producer" ("Id", "Code", "Name", "PublicStatus", "IsVerified", "CreatedAt", "IsDeleted", "ConcurrencyStamp")
                VALUES
                    ({firstId}, {"ELIGIBLE_A"}, {"Eligible Alpha"}, {"Published"}, {true}, {now}, {false}, {Guid.NewGuid()}),
                    ({secondId}, {"ELIGIBLE_B"}, {"Eligible Beta"}, {"Published"}, {true}, {now}, {false}, {Guid.NewGuid()}),
                    ({draftId}, {"ELIGIBLE_DRAFT"}, {"Eligible Draft"}, {"Draft"}, {true}, {now}, {false}, {Guid.NewGuid()});
                """);
        }

        await using var factory = new CatalogApiFactory(fixture);
        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new("Bearer", CreateAccessToken(Permissions.CatalogProducts.Create));

        using var paged = await client.GetAsync("/api/v1/catalog/producers?q=eligible&page=2&pageSize=1");
        Assert.Equal(HttpStatusCode.OK, paged.StatusCode);
        using (var document = JsonDocument.Parse(await paged.Content.ReadAsStringAsync()))
        {
            var data = document.RootElement.GetProperty("data");
            Assert.Equal(2, data.GetProperty("totalCount").GetInt32());
            Assert.Equal(2, data.GetProperty("pageNumber").GetInt32());
            var item = Assert.Single(data.GetProperty("items").EnumerateArray());
            Assert.Equal(secondId, item.GetProperty("id").GetGuid());
        }

        using var detail = await client.GetAsync($"/api/v1/catalog/producers/{firstId}");
        Assert.Equal(HttpStatusCode.OK, detail.StatusCode);
        using (var document = JsonDocument.Parse(await detail.Content.ReadAsStringAsync()))
        {
            var data = document.RootElement.GetProperty("data");
            Assert.Equal(firstId, data.GetProperty("id").GetGuid());
            Assert.Equal("ELIGIBLE_A", data.GetProperty("code").GetString());
            Assert.True(data.GetProperty("isVerified").GetBoolean());
            Assert.Equal("Published", data.GetProperty("publicStatus").GetString());
        }

        using var hiddenDetail = await client.GetAsync($"/api/v1/catalog/producers/{draftId}");
        Assert.Equal(HttpStatusCode.NotFound, hiddenDetail.StatusCode);

        using var invalidPage = await client.GetAsync("/api/v1/catalog/producers?page=0&pageSize=101");
        Assert.Equal(HttpStatusCode.BadRequest, invalidPage.StatusCode);
    }

    private static string CreateAccessToken(params string[] permissions)
    {
        var claims = permissions.Select(permission => new Claim("policy", permission)).Append(new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()));
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

    private static async Task AssertSuccessEnvelopeAsync(HttpResponseMessage response)
    {
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.True(document.RootElement.GetProperty("success").GetBoolean());
        Assert.True(document.RootElement.TryGetProperty("data", out _));
    }

    private async Task<string> SeedPublishedProductAsync()
    {
        var producerId = Guid.NewGuid();
        var now = DateTime.UtcNow;
        var slug = $"public-product-{Guid.NewGuid():N}";
        await using var context = fixture.CreateDbContext();
        await context.Database.ExecuteSqlInterpolatedAsync($"""
            INSERT INTO "Tbl_Producer"
                ("Id", "Code", "Name", "PublicStatus", "IsVerified", "CreatedAt", "IsDeleted", "ConcurrencyStamp")
            VALUES
                ({producerId}, {$"PUBLIC_{Guid.NewGuid():N}"}, {"Public Test Producer"}, {"Published"}, {true}, {now}, {false}, {Guid.NewGuid()});
            """);

        var category = Category.Create(null, "Public Test Category", $"public-category-{Guid.NewGuid():N}", null, 0);
        category.Publish();
        var product = Product.Create(producerId, "Public Test Product", slug);
        var productCategory = product.ReplaceCategories([], [new ProductCategoryAssignment(category.Id, true)]).Single();
        product.SubmitForReview();
        product.Publish(now, hasPrimaryCategory: true, hasPrimaryMedia: true, hasActiveVariant: true, hasEffectivePrice: true);
        var variant = ProductVariant.Create(product.Id, $"PUBLIC-SKU-{Guid.NewGuid():N}", "Default", InventoryMode.NotTracked);
        var price = VariantPrice.Create(variant.Id, 123_000m, PriceType.Public, now.AddMinutes(-1), now.AddDays(1));

        context.Categories.Add(category);
        context.Products.Add(product);
        context.ProductCategories.Add(productCategory);
        context.ProductVariants.Add(variant);
        context.VariantPrices.Add(price);
        await context.SaveChangesAsync();
        return slug;
    }

    private async Task<string> SeedCategoryWithPausedAncestorAsync()
    {
        var parent = Category.Create(null, "Paused Parent", $"paused-parent-{Guid.NewGuid():N}", null, 0);
        parent.Publish();
        var childSlug = $"hidden-child-{Guid.NewGuid():N}";
        var child = Category.Create(parent.Id, "Published Child", childSlug, null, 0);
        child.Publish();
        parent.Pause();

        await using var context = fixture.CreateDbContext();
        context.Categories.AddRange(parent, child);
        await context.SaveChangesAsync();
        return childSlug;
    }
}
