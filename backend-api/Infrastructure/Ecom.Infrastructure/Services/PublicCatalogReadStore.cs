using System.Data;
using Ecom.Application.Common.Interfaces;
using Ecom.Application.Common.Models;
using Ecom.Application.Features.Catalog.Common;
using Ecom.Application.Features.Catalog.Queries.GetProductList;
using Ecom.Infrastructure.Persistence.Database;
using Microsoft.EntityFrameworkCore;

namespace Ecom.Infrastructure.Services;

/// <summary>
/// Native PostgreSQL projection for the anonymous storefront. Keeping the pricing window
/// expression in SQL prevents EF LINQ translation failures from taking down product browsing.
/// </summary>
public sealed class PublicCatalogReadStore(
    ApplicationDbContext db,
    IStorageService storage,
    ILogger<PublicCatalogReadStore> logger) : IPublicCatalogReadStore
{
    public async Task<PaginatedList<ProductListItemDto>> GetProductListAsync(
        GetProductListQuery query,
        CancellationToken cancellationToken = default)
    {
        var connection = db.Database.GetDbConnection();
        var closeConnection = connection.State != ConnectionState.Open;
        if (closeConnection)
            await connection.OpenAsync(cancellationToken);

        try
        {
            await using var command = connection.CreateCommand();
            command.CommandText = BuildSql(query.Sort);
            AddParameter(command, "asOfUtc", DateTime.UtcNow, DbType.DateTime);
            AddParameter(command, "search", string.IsNullOrWhiteSpace(query.Q) ? null : query.Q.Trim(), DbType.String);
            AddParameter(command, "categorySlug", string.IsNullOrWhiteSpace(query.CategorySlug) ? null : query.CategorySlug.Trim(), DbType.String);
            AddParameter(command, "producerId", query.ProducerId, DbType.Guid);
            AddParameter(command, "minPrice", query.MinPrice, DbType.Decimal);
            AddParameter(command, "maxPrice", query.MaxPrice, DbType.Decimal);
            AddParameter(command, "pageSize", query.PageSize, DbType.Int32);
            AddParameter(command, "offset", query.Skip(), DbType.Int32);

            var items = new List<ProductListItemDto>();
            var totalCount = 0;
            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                totalCount = reader.GetInt32(0);
                var productId = reader.GetGuid(1);
                var primaryMedia = CreatePrimaryMedia(reader, productId);
                items.Add(new ProductListItemDto(
                    productId,
                    reader.GetString(2),
                    reader.GetString(3),
                    reader.IsDBNull(4) ? null : reader.GetString(4),
                    new ProducerSummaryDto(
                        reader.GetGuid(6),
                        reader.GetString(7),
                        reader.GetString(8),
                        reader.IsDBNull(9) ? null : reader.GetString(9),
                        reader.IsDBNull(10) ? null : reader.GetString(10)),
                    new CategorySummaryDto(reader.GetGuid(11), reader.GetString(12), reader.GetString(13), true, reader.GetInt32(14)),
                    primaryMedia,
                    reader.GetDecimal(15),
                    reader.GetString(16),
                    reader.GetFieldValue<DateTime>(5)));
            }

            return PaginatedList<ProductListItemDto>.Create(items, totalCount, query.Page, query.PageSize);
        }
        finally
        {
            if (closeConnection)
                await connection.CloseAsync();
        }
    }

    private ProductMediaDto? CreatePrimaryMedia(IDataRecord row, Guid productId)
    {
        if (row.IsDBNull(17))
            return null;

        var mediaAssetId = row.GetGuid(17);
        try
        {
            return new ProductMediaDto(
                mediaAssetId,
                storage.GetPublicFileUrl(row.GetString(18)),
                row.GetString(19),
                row.IsDBNull(20) ? null : row.GetString(20),
                row.IsDBNull(21) ? null : row.GetString(21),
                row.GetInt32(22),
                true);
        }
        catch (InvalidOperationException exception)
        {
            logger.LogWarning(exception,
                "Ignoring invalid public-media storage key for ProductId {ProductId} and MediaAssetId {MediaAssetId}.",
                productId,
                mediaAssetId);
            return null;
        }
    }

    private static void AddParameter(IDbCommand command, string name, object? value, DbType dbType)
    {
        var parameter = command.CreateParameter();
        parameter.ParameterName = name;
        parameter.DbType = dbType;
        parameter.Value = value ?? DBNull.Value;
        command.Parameters.Add(parameter);
    }

    private static string BuildSql(string sort)
    {
        var orderBy = sort switch
        {
            ProductSort.NameAscending => "\"Name\" ASC, \"Id\" ASC",
            ProductSort.PriceAscending => "\"FromPrice\" ASC, \"Id\" ASC",
            ProductSort.PriceDescending => "\"FromPrice\" DESC, \"Id\" ASC",
            _ => "\"PublishedAt\" DESC NULLS LAST, \"Id\" ASC"
        };

        return $$"""
            WITH RECURSIVE "CategoryAncestors" AS (
                SELECT c."Id" AS "CategoryId", c."Id" AS "AncestorId", c."ParentId", c."Status", c."IsDeleted"
                FROM "Tbl_Category" AS c
                UNION ALL
                SELECT ancestors."CategoryId", parent."Id", parent."ParentId", parent."Status", parent."IsDeleted"
                FROM "CategoryAncestors" AS ancestors
                INNER JOIN "Tbl_Category" AS parent ON parent."Id" = ancestors."ParentId"
            ),
            "EffectiveVariantPrices" AS (
                SELECT
                    variant."ProductId",
                    price."ProductVariantId",
                    price."Amount",
                    price."CurrencyCode",
                    ROW_NUMBER() OVER (
                        PARTITION BY price."ProductVariantId"
                        ORDER BY
                            CASE WHEN price."PriceType" = 'Sale' THEN 0 ELSE 1 END,
                            price."EffectiveFrom" DESC,
                            price."Id" ASC) AS "PriceRank"
                FROM "Tbl_VariantPrice" AS price
                INNER JOIN "Tbl_ProductVariant" AS variant
                    ON variant."Id" = price."ProductVariantId"
                    AND variant."IsDeleted" = FALSE
                    AND variant."Status" = 'Active'
                LEFT JOIN "Tbl_PriceList" AS priceList
                    ON priceList."Id" = price."PriceListId"
                    AND priceList."IsDeleted" = FALSE
                WHERE price."IsDeleted" = FALSE
                    AND price."CurrencyCode" = 'VND'
                    AND price."MinQuantity" = 1
                    AND price."PriceType" IN ('Sale', 'Public')
                    AND price."EffectiveFrom" <= @asOfUtc
                    AND (price."EffectiveTo" IS NULL OR price."EffectiveTo" > @asOfUtc)
                    AND (price."PriceListId" IS NULL OR (
                        priceList."Id" IS NOT NULL
                        AND priceList."Status" = 'Active'
                        AND (priceList."StartsAt" IS NULL OR priceList."StartsAt" <= @asOfUtc)
                        AND (priceList."EndsAt" IS NULL OR priceList."EndsAt" > @asOfUtc)))
            ),
            "ProductPrices" AS (
                SELECT "ProductId", MIN("Amount") AS "FromPrice", MIN("CurrencyCode") AS "CurrencyCode"
                FROM "EffectiveVariantPrices"
                WHERE "PriceRank" = 1
                GROUP BY "ProductId"
            ),
            "PublicProducts" AS (
                SELECT
                    product."Id",
                    product."Slug",
                    product."Name",
                    product."ShortDescription",
                    product."PublishedAt",
                    producer."Id" AS "ProducerId",
                    producer."Code" AS "ProducerCode",
                    producer."Name" AS "ProducerName",
                    producer."Description" AS "ProducerDescription",
                    producer."WebsiteUrl" AS "ProducerWebsiteUrl",
                    category."Id" AS "CategoryId",
                    category."Name" AS "CategoryName",
                    category."Slug" AS "CategorySlug",
                    category."DisplayOrder" AS "CategoryDisplayOrder",
                    prices."FromPrice",
                    prices."CurrencyCode",
                    media."MediaAssetId",
                    media."StorageKey" AS "MediaStorageKey",
                    media."ContentType" AS "MediaContentType",
                    media."AltText" AS "MediaAltText",
                    media."Caption" AS "MediaCaption",
                    media."DisplayOrder" AS "MediaDisplayOrder"
                FROM "Tbl_Product" AS product
                INNER JOIN "Tbl_Producer" AS producer
                    ON producer."Id" = product."ProducerId"
                    AND producer."IsDeleted" = FALSE
                INNER JOIN "Tbl_ProductCategory" AS productCategory
                    ON productCategory."ProductId" = product."Id"
                    AND productCategory."IsDeleted" = FALSE
                    AND productCategory."IsPrimary" = TRUE
                INNER JOIN "Tbl_Category" AS category
                    ON category."Id" = productCategory."CategoryId"
                    AND category."IsDeleted" = FALSE
                INNER JOIN "ProductPrices" AS prices ON prices."ProductId" = product."Id"
                LEFT JOIN LATERAL (
                    SELECT
                        productMedia."MediaAssetId",
                        mediaAsset."StorageKey",
                        mediaAsset."ContentType",
                        mediaAsset."AltText",
                        productMedia."Caption",
                        productMedia."DisplayOrder"
                    FROM "Tbl_ProductMedia" AS productMedia
                    INNER JOIN "Tbl_MediaAsset" AS mediaAsset
                        ON mediaAsset."Id" = productMedia."MediaAssetId"
                        AND mediaAsset."IsDeleted" = FALSE
                    WHERE productMedia."ProductId" = product."Id"
                        AND productMedia."IsDeleted" = FALSE
                        AND productMedia."IsPrimary" = TRUE
                        AND mediaAsset."Visibility" = 'Public'
                        AND mediaAsset."ScanStatus" = 'Clean'
                    ORDER BY productMedia."DisplayOrder", productMedia."Id"
                    LIMIT 1
                ) AS media ON TRUE
                WHERE product."IsDeleted" = FALSE
                    AND product."Status" = 'Published'
                    AND producer."PublicStatus" = 'Published'
                    AND producer."IsVerified" = TRUE
                    AND NOT EXISTS (
                        SELECT 1
                        FROM "CategoryAncestors" AS ancestors
                        WHERE ancestors."CategoryId" = category."Id"
                            AND (ancestors."IsDeleted" = TRUE OR ancestors."Status" <> 'Published'))
                    AND (@search IS NULL OR product."Name" ILIKE '%' || @search || '%'
                        OR COALESCE(product."ShortDescription", '') ILIKE '%' || @search || '%')
                    AND (@producerId IS NULL OR producer."Id" = @producerId)
                    AND (@minPrice IS NULL OR prices."FromPrice" >= @minPrice)
                    AND (@maxPrice IS NULL OR prices."FromPrice" <= @maxPrice)
                    AND (@categorySlug IS NULL OR EXISTS (
                        SELECT 1
                        FROM "Tbl_ProductCategory" AS categoryFilter
                        INNER JOIN "Tbl_Category" AS filteredCategory
                            ON filteredCategory."Id" = categoryFilter."CategoryId"
                            AND filteredCategory."IsDeleted" = FALSE
                        WHERE categoryFilter."ProductId" = product."Id"
                            AND categoryFilter."IsDeleted" = FALSE
                            AND filteredCategory."Slug" = @categorySlug
                            AND NOT EXISTS (
                                SELECT 1
                                FROM "CategoryAncestors" AS filteredAncestors
                                WHERE filteredAncestors."CategoryId" = filteredCategory."Id"
                                    AND (filteredAncestors."IsDeleted" = TRUE OR filteredAncestors."Status" <> 'Published'))))
            )
            SELECT
                COUNT(*) OVER()::integer AS "TotalCount",
                "Id", "Slug", "Name", "ShortDescription", "PublishedAt",
                "ProducerId", "ProducerCode", "ProducerName", "ProducerDescription", "ProducerWebsiteUrl",
                "CategoryId", "CategoryName", "CategorySlug", "CategoryDisplayOrder",
                "FromPrice", "CurrencyCode",
                "MediaAssetId", "MediaStorageKey", "MediaContentType", "MediaAltText", "MediaCaption", "MediaDisplayOrder"
            FROM "PublicProducts"
            ORDER BY {{orderBy}}
            LIMIT @pageSize OFFSET @offset;
            """;
    }
}
