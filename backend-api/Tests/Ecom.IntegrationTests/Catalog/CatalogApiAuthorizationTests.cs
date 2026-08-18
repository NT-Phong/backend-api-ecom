using Ecom.Domain.Constants;
using Ecom.Domain.Entities;
using Ecom.Domain.Enums;
using Ecom.IntegrationTests.PostgreSql;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Json;
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
    public async Task Public_product_list_and_detail_return_a_published_product_with_an_effective_price()
    {
        await fixture.ResetDatabaseAsync();
        var product = await SeedPublishedProductAsync();
        await using var factory = new CatalogApiFactory(fixture);
        using var client = factory.CreateClient();

        using var response = await client.GetAsync("/api/v1/products");
        using var detail = await client.GetAsync($"/api/v1/products/{product.Slug}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(HttpStatusCode.OK, detail.StatusCode);
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var items = document.RootElement.GetProperty("data").GetProperty("items").EnumerateArray();
        var item = Assert.Single(items, item => string.Equals(item.GetProperty("slug").GetString(), product.Slug, StringComparison.Ordinal));
        Assert.True(item.GetProperty("hasEffectivePrice").GetBoolean());
        Assert.Equal(123_000m, item.GetProperty("fromPrice").GetDecimal());
    }

    [PostgreSqlFact]
    public async Task Public_product_list_and_detail_keep_a_published_product_when_its_primary_media_is_no_longer_public()
    {
        await fixture.ResetDatabaseAsync();
        var product = await SeedPublishedProductAsync();
        await using (var context = fixture.CreateDbContext())
        {
            var media = await context.MediaAssets.SingleAsync(x => x.Id == product.MediaAssetId);
            media.ChangeVisibility(MediaVisibility.Internal);
            await context.SaveChangesAsync();
        }

        await using var factory = new CatalogApiFactory(fixture);
        using var client = factory.CreateClient();
        using var list = await client.GetAsync("/api/v1/products");
        using var detail = await client.GetAsync($"/api/v1/products/{product.Slug}");

        Assert.Equal(HttpStatusCode.OK, list.StatusCode);
        using var document = JsonDocument.Parse(await list.Content.ReadAsStringAsync());
        var item = Assert.Single(document.RootElement.GetProperty("data").GetProperty("items").EnumerateArray(),
            item => string.Equals(item.GetProperty("slug").GetString(), product.Slug, StringComparison.Ordinal));
        Assert.Equal(JsonValueKind.Null, item.GetProperty("primaryMedia").ValueKind);
        Assert.Equal(HttpStatusCode.OK, detail.StatusCode);
        using var detailDocument = JsonDocument.Parse(await detail.Content.ReadAsStringAsync());
        Assert.Empty(detailDocument.RootElement.GetProperty("data").GetProperty("media").EnumerateArray());
    }

    [PostgreSqlFact]
    public async Task Public_product_list_and_detail_keep_a_published_product_when_a_primary_category_ancestor_is_not_published()
    {
        await fixture.ResetDatabaseAsync();
        var product = await SeedPublishedProductAsync();
        await using (var context = fixture.CreateDbContext())
        {
            var category = await context.Categories.SingleAsync(x => x.Id == product.CategoryId);
            var draftParent = Category.Create(null, "Draft Parent", $"draft-parent-{Guid.NewGuid():N}", null, 0);
            category.UpdateDetails(draftParent.Id, category.Name, category.Slug, category.Description, category.DisplayOrder);
            context.Categories.Add(draftParent);
            await context.SaveChangesAsync();
        }

        await using var factory = new CatalogApiFactory(fixture);
        using var client = factory.CreateClient();
        using var list = await client.GetAsync("/api/v1/products");
        using var detail = await client.GetAsync($"/api/v1/products/{product.Slug}");

        Assert.Equal(HttpStatusCode.OK, list.StatusCode);
        using var document = JsonDocument.Parse(await list.Content.ReadAsStringAsync());
        var item = Assert.Single(document.RootElement.GetProperty("data").GetProperty("items").EnumerateArray(),
            item => string.Equals(item.GetProperty("slug").GetString(), product.Slug, StringComparison.Ordinal));
        Assert.Equal(JsonValueKind.Null, item.GetProperty("primaryCategory").ValueKind);
        Assert.Equal(HttpStatusCode.OK, detail.StatusCode);
        using var detailDocument = JsonDocument.Parse(await detail.Content.ReadAsStringAsync());
        Assert.Empty(detailDocument.RootElement.GetProperty("data").GetProperty("categories").EnumerateArray());
    }

    [PostgreSqlFact]
    public async Task Public_product_list_and_detail_keep_a_published_product_when_its_producer_becomes_unverified()
    {
        await fixture.ResetDatabaseAsync();
        var product = await SeedPublishedProductAsync();
        await using (var context = fixture.CreateDbContext())
        {
            await context.Database.ExecuteSqlInterpolatedAsync($"""
                UPDATE "Tbl_Producer"
                SET "PublicStatus" = {"Draft"}, "IsVerified" = {false}
                WHERE "Id" = {product.ProducerId};
                """);
        }

        await using var factory = new CatalogApiFactory(fixture);
        using var client = factory.CreateClient();
        using var list = await client.GetAsync("/api/v1/products");
        using var detail = await client.GetAsync($"/api/v1/products/{product.Slug}");

        Assert.Equal(HttpStatusCode.OK, list.StatusCode);
        using var document = JsonDocument.Parse(await list.Content.ReadAsStringAsync());
        Assert.Contains(document.RootElement.GetProperty("data").GetProperty("items").EnumerateArray(),
            item => string.Equals(item.GetProperty("slug").GetString(), product.Slug, StringComparison.Ordinal));
        Assert.Equal(HttpStatusCode.OK, detail.StatusCode);
    }

    [PostgreSqlFact]
    public async Task Public_product_list_and_detail_keep_a_published_product_when_it_has_no_effective_price()
    {
        await fixture.ResetDatabaseAsync();
        var product = await SeedPublishedProductAsync();
        await using (var context = fixture.CreateDbContext())
        {
            await context.Database.ExecuteSqlInterpolatedAsync($"""
                UPDATE "Tbl_VariantPrice"
                SET "EffectiveTo" = {DateTime.UtcNow.AddMinutes(-1)}
                WHERE "ProductVariantId" IN (
                    SELECT "Id" FROM "Tbl_ProductVariant" WHERE "ProductId" = {product.ProductId});
                """);
        }

        await using var factory = new CatalogApiFactory(fixture);
        using var client = factory.CreateClient();
        using var list = await client.GetAsync("/api/v1/products");
        using var detail = await client.GetAsync($"/api/v1/products/{product.Slug}");
        using var pricedOnly = await client.GetAsync("/api/v1/products?minPrice=1");

        Assert.Equal(HttpStatusCode.OK, list.StatusCode);
        using (var document = JsonDocument.Parse(await list.Content.ReadAsStringAsync()))
        {
            var item = Assert.Single(document.RootElement.GetProperty("data").GetProperty("items").EnumerateArray(),
                item => string.Equals(item.GetProperty("slug").GetString(), product.Slug, StringComparison.Ordinal));
            Assert.False(item.GetProperty("hasEffectivePrice").GetBoolean());
            Assert.Equal(JsonValueKind.Null, item.GetProperty("fromPrice").ValueKind);
            Assert.Equal(JsonValueKind.Null, item.GetProperty("currencyCode").ValueKind);
        }
        Assert.Equal(HttpStatusCode.OK, detail.StatusCode);
        using (var detailDocument = JsonDocument.Parse(await detail.Content.ReadAsStringAsync()))
        {
            var data = detailDocument.RootElement.GetProperty("data");
            Assert.False(data.GetProperty("hasEffectivePrice").GetBoolean());
            Assert.Empty(data.GetProperty("variants").EnumerateArray());
        }
        using var pricedDocument = JsonDocument.Parse(await pricedOnly.Content.ReadAsStringAsync());
        Assert.DoesNotContain(pricedDocument.RootElement.GetProperty("data").GetProperty("items").EnumerateArray(),
            item => string.Equals(item.GetProperty("slug").GetString(), product.Slug, StringComparison.Ordinal));
    }

    [PostgreSqlFact]
    public async Task Public_product_price_sort_places_products_without_an_effective_price_last()
    {
        await fixture.ResetDatabaseAsync();
        var pricedProduct = await SeedPublishedProductAsync();
        var unpricedProduct = await SeedPublishedProductAsync();
        await using (var context = fixture.CreateDbContext())
        {
            await context.Database.ExecuteSqlInterpolatedAsync($"""
                UPDATE "Tbl_VariantPrice"
                SET "EffectiveTo" = {DateTime.UtcNow.AddMinutes(-1)}
                WHERE "ProductVariantId" IN (
                    SELECT "Id" FROM "Tbl_ProductVariant" WHERE "ProductId" = {unpricedProduct.ProductId});
                """);
        }

        await using var factory = new CatalogApiFactory(fixture);
        using var client = factory.CreateClient();
        using var response = await client.GetAsync("/api/v1/products?sort=price-asc");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var slugs = document.RootElement.GetProperty("data").GetProperty("items").EnumerateArray()
            .Select(item => item.GetProperty("slug").GetString()).ToList();
        Assert.True(slugs.IndexOf(pricedProduct.Slug) < slugs.IndexOf(unpricedProduct.Slug));
    }

    [PostgreSqlFact]
    public async Task Updating_a_published_category_rejects_a_draft_parent()
    {
        await fixture.ResetDatabaseAsync();
        var parent = Category.Create(null, "Draft Parent", $"draft-parent-{Guid.NewGuid():N}", null, 0);
        var category = Category.Create(null, "Published Child", $"published-child-{Guid.NewGuid():N}", null, 0);
        category.Publish();
        await using (var context = fixture.CreateDbContext())
        {
            context.Categories.AddRange(parent, category);
            await context.SaveChangesAsync();
        }

        await using var factory = new CatalogApiFactory(fixture);
        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new("Bearer", CreateAccessToken(Permissions.CatalogCategories.Update));

        using var response = await client.PutAsJsonAsync($"/api/v1/catalog/categories/{category.Id}", new
        {
            concurrencyStamp = category.ConcurrencyStamp,
            parentId = parent.Id,
            category.Name,
            category.Slug,
            category.Description,
            category.DisplayOrder
        });

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
    }

    [PostgreSqlFact]
    public async Task Public_product_price_filters_use_the_display_from_price()
    {
        await fixture.ResetDatabaseAsync();
        var product = await SeedPublishedProductAsync();
        await using var factory = new CatalogApiFactory(fixture);
        using var client = factory.CreateClient();

        using var matching = await client.GetAsync("/api/v1/products?maxPrice=123000");
        using var excluded = await client.GetAsync("/api/v1/products?minPrice=123001");

        using (var document = JsonDocument.Parse(await matching.Content.ReadAsStringAsync()))
        {
            Assert.Contains(document.RootElement.GetProperty("data").GetProperty("items").EnumerateArray(),
                item => string.Equals(item.GetProperty("slug").GetString(), product.Slug, StringComparison.Ordinal));
        }
        using (var document = JsonDocument.Parse(await excluded.Content.ReadAsStringAsync()))
        {
            Assert.DoesNotContain(document.RootElement.GetProperty("data").GetProperty("items").EnumerateArray(),
                item => string.Equals(item.GetProperty("slug").GetString(), product.Slug, StringComparison.Ordinal));
        }
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

    private async Task<PublicProductSeed> SeedPublishedProductAsync()
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
        var media = MediaAsset.CreatePending($"uploads/quarantine/{Guid.NewGuid():N}.jpg", "product.jpg", "image/jpeg", 128,
            MediaType.Image, MediaVisibility.Public);
        media.MarkClean($"uploads/public/{Guid.NewGuid():N}.jpg", MediaVisibility.Public, now);
        var productMedia = product.AttachMedia([], media.Id, 0, makePrimary: true, mediaIsPubliclyUsable: true);

        context.Categories.Add(category);
        context.Products.Add(product);
        context.ProductCategories.Add(productCategory);
        context.ProductVariants.Add(variant);
        context.VariantPrices.Add(price);
        context.MediaAssets.Add(media);
        context.ProductMedia.Add(productMedia);
        await context.SaveChangesAsync();
        return new PublicProductSeed(product.Id, producerId, category.Id, media.Id, slug);
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

    private sealed record PublicProductSeed(Guid ProductId, Guid ProducerId, Guid CategoryId, Guid MediaAssetId, string Slug);
}
