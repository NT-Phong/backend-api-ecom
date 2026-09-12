using System.Data;
using Ecom.Application.Common.Interfaces;
using Ecom.Application.Features.Catalog.Common;
using Ecom.Application.Features.Catalog.Queries.SearchProducts;
using Ecom.Infrastructure.Persistence.Database;

namespace Ecom.Infrastructure.Services;

/// <summary>
/// Parameterized PostgreSQL discovery projection. The SQL deliberately operates only on
/// published, public and price-effective facts; it never joins management price history or ledgers.
/// </summary>
public sealed class PublicCatalogSearchStore(ApplicationDbContext db, IStorageService storage,
    ILogger<PublicCatalogSearchStore> logger) : IPublicCatalogSearchStore
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public async Task<ProductSearchResponseDto> SearchAsync(SearchProductsQuery query,
        CancellationToken cancellationToken = default)
    {
        var connection = db.Database.GetDbConnection();
        var close = connection.State != ConnectionState.Open;
        if (close) await connection.OpenAsync(cancellationToken);
        try
        {
            await using var command = connection.CreateCommand();
            command.CommandText = BuildSearchSql(query.Sort);
            AddParameters(command, query);
            var items = new List<ProductListItemDto>();
            var totalCount = 0;
            List<CatalogSearchFacetDto> categories = [];
            List<CatalogSearchFacetDto> producers = [];
            List<CatalogAvailabilityFacetDto> availability = [];
            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                totalCount = reader.GetInt32(0);
                var productId = reader.GetGuid(1);
                var media = CreateMedia(reader, productId);
                items.Add(new ProductListItemDto(productId, reader.GetString(2), reader.GetString(3),
                    reader.IsDBNull(4) ? null : reader.GetString(4),
                    new ProducerSummaryDto(reader.GetGuid(6), reader.GetString(7), reader.GetString(8),
                        reader.IsDBNull(9) ? null : reader.GetString(9), reader.IsDBNull(10) ? null : reader.GetString(10)),
                    reader.IsDBNull(11) ? null : new CategorySummaryDto(reader.GetGuid(11), reader.GetString(12),
                        reader.GetString(13), true, reader.GetInt32(14)),
                    media, reader.GetDecimal(15), reader.GetString(16), true,
                    Enum.Parse<CatalogAvailabilityStatus>(reader.GetString(23), true), reader.GetFieldValue<DateTime>(5)));
                if (items.Count == 1)
                {
                    categories = Deserialize<List<CatalogSearchFacetDto>>(reader.GetString(25));
                    producers = Deserialize<List<CatalogSearchFacetDto>>(reader.GetString(26));
                    availability = Deserialize<List<CatalogAvailabilityFacetDto>>(reader.GetString(27));
                }
            }
            return new ProductSearchResponseDto(items, totalCount, query.Page, query.PageSize, categories, producers,
                availability);
        }
        finally
        {
            if (close) await connection.CloseAsync();
        }
    }

    public async Task<IReadOnlyList<ProductSearchSuggestionDto>> SuggestAsync(ProductSearchSuggestionsQuery query,
        CancellationToken cancellationToken = default)
    {
        var connection = db.Database.GetDbConnection();
        var close = connection.State != ConnectionState.Open;
        if (close) await connection.OpenAsync(cancellationToken);
        try
        {
            await using var command = connection.CreateCommand();
            command.CommandText = SuggestionSql;
            AddParameter(command, "asOfUtc", DateTime.UtcNow, DbType.DateTime);
            AddParameter(command, "search", query.Q.Trim(), DbType.String);
            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            var suggestions = new List<ProductSearchSuggestionDto>();
            while (await reader.ReadAsync(cancellationToken))
            {
                var productId = reader.GetGuid(0);
                suggestions.Add(new ProductSearchSuggestionDto(reader.GetString(1), reader.GetString(2),
                    CreateMedia(reader, productId, 3)));
            }
            return suggestions;
        }
        finally
        {
            if (close) await connection.CloseAsync();
        }
    }

    private static T Deserialize<T>(string json) where T : new() =>
        JsonSerializer.Deserialize<T>(json, JsonOptions) ?? new T();

    private ProductMediaDto? CreateMedia(IDataRecord row, Guid productId, int start = 17)
    {
        if (row.IsDBNull(start)) return null;
        var mediaAssetId = row.GetGuid(start);
        try
        {
            return new ProductMediaDto(mediaAssetId, storage.GetPublicFileUrl(row.GetString(start + 1)),
                row.GetString(start + 2), row.IsDBNull(start + 3) ? null : row.GetString(start + 3),
                row.IsDBNull(start + 4) ? null : row.GetString(start + 4), row.GetInt32(start + 5), true);
        }
        catch (InvalidOperationException exception)
        {
            logger.LogWarning(exception, "Ignoring invalid public-media key for product {ProductId}.", productId);
            return null;
        }
    }

    private static void AddParameters(IDbCommand command, SearchProductsQuery query)
    {
        AddParameter(command, "asOfUtc", DateTime.UtcNow, DbType.DateTime);
        AddParameter(command, "search", query.Q.Trim(), DbType.String);
        AddParameter(command, "categorySlug", string.IsNullOrWhiteSpace(query.CategorySlug) ? null : query.CategorySlug.Trim(), DbType.String);
        AddParameter(command, "producerId", query.ProducerId, DbType.Guid);
        AddParameter(command, "minPrice", query.MinPrice, DbType.Decimal);
        AddParameter(command, "maxPrice", query.MaxPrice, DbType.Decimal);
        AddParameter(command, "availability", query.Availability?.ToString(), DbType.String);
        AddParameter(command, "pageSize", query.PageSize, DbType.Int32);
        AddParameter(command, "offset", query.Skip(), DbType.Int32);
    }

    private static void AddParameter(IDbCommand command, string name, object? value, DbType type)
    {
        var parameter = command.CreateParameter();
        parameter.ParameterName = name;
        parameter.DbType = type;
        parameter.Value = value ?? DBNull.Value;
        command.Parameters.Add(parameter);
    }

    private static string BuildSearchSql(string sort)
    {
        var orderBy = sort switch
        {
            ProductSearchSort.PriceAscending => "\"FromPrice\" ASC, \"Id\" ASC",
            ProductSearchSort.PriceDescending => "\"FromPrice\" DESC, \"Id\" ASC",
            ProductSearchSort.Newest => "\"PublishedAt\" DESC, \"Id\" ASC",
            _ => "\"Rank\" DESC, \"PublishedAt\" DESC, \"Id\" ASC"
        };
        return $$"""
            {{CommonCte}},
            "Filtered" AS (
                SELECT product."Id", product."Slug", product."Name", product."ShortDescription", product."PublishedAt",
                    producer."Id" AS "ProducerId", producer."Code" AS "ProducerCode", producer."Name" AS "ProducerName",
                    producer."Description" AS "ProducerDescription", producer."WebsiteUrl" AS "ProducerWebsiteUrl",
                    category."Id" AS "CategoryId", category."Name" AS "CategoryName", category."Slug" AS "CategorySlug",
                    category."DisplayOrder" AS "CategoryDisplayOrder", prices."FromPrice", prices."CurrencyCode",
                    media."MediaAssetId", media."StorageKey" AS "MediaStorageKey", media."ContentType" AS "MediaContentType",
                    media."AltText" AS "MediaAltText", media."Caption" AS "MediaCaption", media."DisplayOrder" AS "MediaDisplayOrder",
                    COALESCE(productAvailability."Availability", 'Unavailable') AS "Availability",
                    GREATEST(similarity(immutable_unaccent(product."Name"), immutable_unaccent(@search)),
                        similarity(immutable_unaccent(COALESCE(product."ShortDescription", '')), immutable_unaccent(@search)),
                        similarity(immutable_unaccent(COALESCE(product."BrandName", '')), immutable_unaccent(@search))) AS "Rank"
                FROM "Tbl_Product" AS product
                INNER JOIN "Tbl_Producer" AS producer ON producer."Id" = product."ProducerId"
                    AND producer."IsDeleted" = FALSE AND producer."PublicStatus" = 'Published' AND producer."IsVerified" = TRUE
                INNER JOIN "ProductPrices" AS prices ON prices."ProductId" = product."Id"
                LEFT JOIN "ProductAvailability" AS productAvailability ON productAvailability."ProductId" = product."Id"
                LEFT JOIN "Tbl_ProductCategory" AS productCategory ON productCategory."ProductId" = product."Id"
                    AND productCategory."IsDeleted" = FALSE AND productCategory."IsPrimary" = TRUE
                LEFT JOIN "Tbl_Category" AS category ON category."Id" = productCategory."CategoryId"
                    AND category."IsDeleted" = FALSE AND category."Status" = 'Published'
                    AND NOT EXISTS (SELECT 1 FROM "CategoryAncestors" ancestors WHERE ancestors."CategoryId" = category."Id"
                        AND (ancestors."IsDeleted" OR ancestors."Status" <> 'Published'))
                LEFT JOIN LATERAL (
                    SELECT productMedia."MediaAssetId", mediaAsset."StorageKey", mediaAsset."ContentType", mediaAsset."AltText",
                        productMedia."Caption", productMedia."DisplayOrder"
                    FROM "Tbl_ProductMedia" productMedia
                    INNER JOIN "Tbl_MediaAsset" mediaAsset ON mediaAsset."Id" = productMedia."MediaAssetId"
                        AND mediaAsset."IsDeleted" = FALSE AND mediaAsset."Visibility" = 'Public' AND mediaAsset."ScanStatus" = 'Clean'
                    WHERE productMedia."ProductId" = product."Id" AND productMedia."IsDeleted" = FALSE AND productMedia."IsPrimary" = TRUE
                    ORDER BY productMedia."DisplayOrder", productMedia."Id" LIMIT 1) media ON TRUE
                WHERE product."IsDeleted" = FALSE AND product."Status" = 'Published' AND category."Id" IS NOT NULL
                    AND (immutable_unaccent(product."Name") ILIKE '%' || immutable_unaccent(@search) || '%'
                        OR immutable_unaccent(COALESCE(product."ShortDescription", '')) ILIKE '%' || immutable_unaccent(@search) || '%'
                        OR immutable_unaccent(COALESCE(product."BrandName", '')) ILIKE '%' || immutable_unaccent(@search) || '%')
                    AND (@producerId IS NULL OR producer."Id" = @producerId)
                    AND (@minPrice IS NULL OR prices."FromPrice" >= @minPrice)
                    AND (@maxPrice IS NULL OR prices."FromPrice" <= @maxPrice)
                    AND (@availability IS NULL OR COALESCE(productAvailability."Availability", 'Unavailable') = @availability)
                    AND (@categorySlug IS NULL OR EXISTS (
                        SELECT 1 FROM "Tbl_ProductCategory" categoryFilter
                        INNER JOIN "Tbl_Category" filteredCategory ON filteredCategory."Id" = categoryFilter."CategoryId"
                            AND filteredCategory."IsDeleted" = FALSE AND filteredCategory."Status" = 'Published'
                        WHERE categoryFilter."ProductId" = product."Id" AND categoryFilter."IsDeleted" = FALSE
                            AND filteredCategory."Slug" = @categorySlug AND NOT EXISTS (
                                SELECT 1 FROM "CategoryAncestors" filteredAncestors
                                WHERE filteredAncestors."CategoryId" = filteredCategory."Id"
                                    AND (filteredAncestors."IsDeleted" OR filteredAncestors."Status" <> 'Published'))))
            )
            SELECT COUNT(*) OVER()::integer, f.*, 
                COALESCE((SELECT jsonb_agg(to_jsonb(categoryFacet)) FROM (
                    SELECT category."Id", category."Name", category."Slug", COUNT(DISTINCT f2."Id")::integer AS "Count"
                    FROM "Filtered" f2 INNER JOIN "Tbl_ProductCategory" map ON map."ProductId" = f2."Id" AND map."IsDeleted" = FALSE
                    INNER JOIN "Tbl_Category" category ON category."Id" = map."CategoryId" AND category."IsDeleted" = FALSE AND category."Status" = 'Published'
                    WHERE NOT EXISTS (SELECT 1 FROM "CategoryAncestors" ancestors WHERE ancestors."CategoryId" = category."Id"
                        AND (ancestors."IsDeleted" OR ancestors."Status" <> 'Published'))
                    GROUP BY category."Id", category."Name", category."Slug") categoryFacet), '[]'::jsonb)::text,
                COALESCE((SELECT jsonb_agg(to_jsonb(producerFacet)) FROM (
                    SELECT f2."ProducerId" AS "Id", f2."ProducerName" AS "Name", NULL::text AS "Slug", COUNT(*)::integer AS "Count"
                    FROM "Filtered" f2 GROUP BY f2."ProducerId", f2."ProducerName") producerFacet), '[]'::jsonb)::text,
                COALESCE((SELECT jsonb_agg(to_jsonb(availabilityFacet)) FROM (
                    SELECT f2."Availability", COUNT(*)::integer AS "Count" FROM "Filtered" f2 GROUP BY f2."Availability") availabilityFacet), '[]'::jsonb)::text
            FROM "Filtered" f
            ORDER BY {{orderBy}}
            LIMIT @pageSize OFFSET @offset;
            """;
    }

    private const string CommonCte = """
        WITH RECURSIVE "CategoryAncestors" AS (
            SELECT c."Id" AS "CategoryId", c."ParentId", c."Status", c."IsDeleted" FROM "Tbl_Category" c
            UNION ALL
            SELECT ancestors."CategoryId", parent."ParentId", parent."Status", parent."IsDeleted"
            FROM "CategoryAncestors" ancestors INNER JOIN "Tbl_Category" parent ON parent."Id" = ancestors."ParentId"
        ),
        "EffectiveVariantPrices" AS (
            SELECT variant."ProductId", price."ProductVariantId", price."Amount", price."CurrencyCode",
                ROW_NUMBER() OVER (PARTITION BY price."ProductVariantId" ORDER BY
                    CASE WHEN price."PriceType" = 'Sale' THEN 0 ELSE 1 END, price."EffectiveFrom" DESC, price."Id") AS "PriceRank"
            FROM "Tbl_VariantPrice" price INNER JOIN "Tbl_ProductVariant" variant ON variant."Id" = price."ProductVariantId"
                AND variant."IsDeleted" = FALSE
            LEFT JOIN "Tbl_PriceList" priceList ON priceList."Id" = price."PriceListId" AND priceList."IsDeleted" = FALSE
            WHERE price."IsDeleted" = FALSE AND price."CurrencyCode" = 'VND' AND price."MinQuantity" = 1
                AND price."PriceType" IN ('Sale', 'Public') AND price."EffectiveFrom" <= @asOfUtc
                AND (price."EffectiveTo" IS NULL OR price."EffectiveTo" > @asOfUtc)
                AND (price."PriceListId" IS NULL OR (priceList."Id" IS NOT NULL AND priceList."Status" = 'Active'
                    AND (priceList."StartsAt" IS NULL OR priceList."StartsAt" <= @asOfUtc)
                    AND (priceList."EndsAt" IS NULL OR priceList."EndsAt" > @asOfUtc)))
        ),
        "ProductPrices" AS (
            SELECT "ProductId", MIN("Amount") AS "FromPrice", MIN("CurrencyCode") AS "CurrencyCode"
            FROM "EffectiveVariantPrices" WHERE "PriceRank" = 1 GROUP BY "ProductId"
        ),
        "MainAvailability" AS (
            SELECT item."ProductVariantId", SUM(level."StockedQuantity" - level."ReservedQuantity") AS "AvailableQuantity"
            FROM "Tbl_InventoryItem" item INNER JOIN "Tbl_InventoryLevel" level ON level."InventoryItemId" = item."Id"
            INNER JOIN "Tbl_StockLocation" location ON location."Id" = level."StockLocationId"
            WHERE item."IsDeleted" = FALSE AND level."IsDeleted" = FALSE AND location."IsDeleted" = FALSE
                AND location."Code" = 'MAIN' AND location."IsActive" = TRUE GROUP BY item."ProductVariantId"
        ),
        "VariantAvailability" AS (
            SELECT variant."ProductId", variant."Id", CASE
                WHEN variant."Status" <> 'Active' OR effective."ProductVariantId" IS NULL THEN 'Unavailable'
                WHEN variant."InventoryMode" = 'NotTracked' THEN 'Available'
                WHEN variant."InventoryMode" = 'Tracked' AND COALESCE(main."AvailableQuantity", 0) > 0 THEN 'Available'
                WHEN variant."InventoryMode" = 'Tracked' THEN 'OutOfStock'
                ELSE 'Unavailable' END AS "Availability"
            FROM "Tbl_ProductVariant" variant LEFT JOIN "EffectiveVariantPrices" effective ON effective."ProductVariantId" = variant."Id" AND effective."PriceRank" = 1
            LEFT JOIN "MainAvailability" main ON main."ProductVariantId" = variant."Id" WHERE variant."IsDeleted" = FALSE
        ),
        "ProductAvailability" AS (
            SELECT "ProductId", CASE WHEN BOOL_OR("Availability" = 'Available') THEN 'Available'
                WHEN BOOL_OR("Availability" = 'OutOfStock') THEN 'OutOfStock' ELSE 'Unavailable' END AS "Availability"
            FROM "VariantAvailability" GROUP BY "ProductId"
        )
        """;

    private const string SuggestionSql = CommonCte + """
        SELECT product."Id", product."Slug", product."Name", media."MediaAssetId", media."StorageKey", media."ContentType",
            media."AltText", media."Caption", media."DisplayOrder"
        FROM "Tbl_Product" product
        INNER JOIN "Tbl_Producer" producer ON producer."Id" = product."ProducerId" AND producer."IsDeleted" = FALSE
            AND producer."PublicStatus" = 'Published' AND producer."IsVerified" = TRUE
        INNER JOIN "ProductPrices" prices ON prices."ProductId" = product."Id"
        LEFT JOIN LATERAL (
            SELECT productMedia."MediaAssetId", mediaAsset."StorageKey", mediaAsset."ContentType", mediaAsset."AltText",
                productMedia."Caption", productMedia."DisplayOrder"
            FROM "Tbl_ProductMedia" productMedia INNER JOIN "Tbl_MediaAsset" mediaAsset ON mediaAsset."Id" = productMedia."MediaAssetId"
                AND mediaAsset."IsDeleted" = FALSE AND mediaAsset."Visibility" = 'Public' AND mediaAsset."ScanStatus" = 'Clean'
            WHERE productMedia."ProductId" = product."Id" AND productMedia."IsDeleted" = FALSE AND productMedia."IsPrimary" = TRUE
            ORDER BY productMedia."DisplayOrder", productMedia."Id" LIMIT 1) media ON TRUE
        WHERE product."IsDeleted" = FALSE AND product."Status" = 'Published'
            AND EXISTS (
                SELECT 1 FROM "Tbl_ProductCategory" primaryMap
                INNER JOIN "Tbl_Category" primaryCategory ON primaryCategory."Id" = primaryMap."CategoryId"
                    AND primaryCategory."IsDeleted" = FALSE AND primaryCategory."Status" = 'Published'
                WHERE primaryMap."ProductId" = product."Id" AND primaryMap."IsDeleted" = FALSE AND primaryMap."IsPrimary" = TRUE
                    AND NOT EXISTS (SELECT 1 FROM "CategoryAncestors" ancestors WHERE ancestors."CategoryId" = primaryCategory."Id"
                        AND (ancestors."IsDeleted" OR ancestors."Status" <> 'Published')))
            AND (immutable_unaccent(product."Name") ILIKE '%' || immutable_unaccent(@search) || '%'
                OR immutable_unaccent(COALESCE(product."ShortDescription", '')) ILIKE '%' || immutable_unaccent(@search) || '%'
                OR immutable_unaccent(COALESCE(product."BrandName", '')) ILIKE '%' || immutable_unaccent(@search) || '%')
        ORDER BY GREATEST(similarity(immutable_unaccent(product."Name"), immutable_unaccent(@search)),
                similarity(immutable_unaccent(COALESCE(product."ShortDescription", '')), immutable_unaccent(@search)),
                similarity(immutable_unaccent(COALESCE(product."BrandName", '')), immutable_unaccent(@search))) DESC,
            product."PublishedAt" DESC, product."Id" ASC LIMIT 8;
        """;
}
