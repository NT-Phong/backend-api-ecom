using System.Data;
using System.Data.Common;
using System.Globalization;
using Ecom.Application.Common.Interfaces;
using Ecom.Application.Features.Commerce.Dashboard;
using Ecom.Application.Features.Commerce.Dashboard.Queries.GetManagementDashboardOverview;
using Ecom.Application.Features.Commerce.Orders;
using Ecom.Application.Features.Commerce.Orders.Queries.GetManagementOrderAnalyticsOverview;
using Ecom.Domain.Constants;
using Ecom.Domain.Enums;
using Ecom.Infrastructure.Persistence.Database;

namespace Ecom.Infrastructure.Services;

public sealed class ManagementDashboardReadStore(ApplicationDbContext db) : IManagementDashboardReadStore
{
    public async Task<ManagementOrderAnalyticsOverviewDto> GetOrderAnalyticsAsync(
        ManagementAnalyticsRange range,
        ManagementOrderAnalyticsGranularity granularity,
        int topLimit,
        CancellationToken cancellationToken = default)
    {
        return await WithOpenConnectionAsync(async connection =>
        {
            var kpis = await ReadKpisAsync(connection, range, cancellationToken);
            var series = await ReadSeriesAsync(connection, range, granularity, cancellationToken);
            var statusBreakdown = await ReadStatusBreakdownAsync(connection, range, cancellationToken);
            var paymentMethodBreakdown = await ReadPaymentMethodBreakdownAsync(connection, range, cancellationToken);
            var topProducts = await ReadTopProductsAsync(connection, range, topLimit, cancellationToken);
            return new ManagementOrderAnalyticsOverviewDto(
                CommerceConstants.DefaultCurrency,
                range.From.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                range.To.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                kpis,
                series,
                statusBreakdown,
                paymentMethodBreakdown,
                topProducts);
        }, cancellationToken);
    }

    public Task<ManagementDashboardSnapshotDto> GetSnapshotAsync(
        ManagementAnalyticsRange range,
        CancellationToken cancellationToken = default) =>
        WithOpenConnectionAsync(async connection =>
        {
            await using var command = connection.CreateCommand();
            command.CommandText = """
WITH inventory_by_variant AS (
    SELECT v."Id",
           COALESCE(SUM(l."StockedQuantity"), 0) AS "StockedQuantity",
           COALESCE(SUM(l."ReservedQuantity"), 0) AS "ReservedQuantity",
           COALESCE(SUM(l."IncomingQuantity"), 0) AS "IncomingQuantity"
    FROM "Tbl_ProductVariant" v
    LEFT JOIN "Tbl_InventoryItem" i ON i."ProductVariantId" = v."Id" AND i."IsDeleted" = false
    LEFT JOIN "Tbl_InventoryLevel" l ON l."InventoryItemId" = i."Id" AND l."IsDeleted" = false
    WHERE v."IsDeleted" = false AND v."InventoryMode" = 'Tracked'
    GROUP BY v."Id"
)
SELECT
    (SELECT COUNT(*) FROM "Tbl_Order" o
     WHERE o."IsDeleted" = false AND o."Status" IN ('Pending', 'Confirmed', 'Preparing', 'Shipping'))::int,
    (SELECT COUNT(*) FROM "Tbl_Product" p WHERE p."IsDeleted" = false)::int,
    (SELECT COUNT(*) FROM "Tbl_Product" p WHERE p."IsDeleted" = false AND p."Status" = 'Draft')::int,
    (SELECT COUNT(*) FROM "Tbl_Product" p WHERE p."IsDeleted" = false AND p."Status" = 'Review')::int,
    (SELECT COUNT(*) FROM "Tbl_Product" p WHERE p."IsDeleted" = false AND p."Status" = 'Published')::int,
    (SELECT COUNT(*) FROM "Tbl_Product" p WHERE p."IsDeleted" = false AND p."Status" = 'Paused')::int,
    (SELECT COUNT(*) FROM "Tbl_Product" p WHERE p."IsDeleted" = false AND p."Status" = 'Discontinued')::int,
    (SELECT COUNT(*) FROM "Tbl_ProductVariant" v WHERE v."IsDeleted" = false AND v."Status" = 'Active')::int,
    (SELECT COUNT(*) FROM "Tbl_ProductVariant" v
     JOIN "Tbl_Product" p ON p."Id" = v."ProductId" AND p."IsDeleted" = false
     WHERE v."IsDeleted" = false AND v."Status" = 'Active' AND p."Status" = 'Published')::int,
    (SELECT COUNT(*) FROM "Tbl_Product" p
     WHERE p."IsDeleted" = false AND NOT EXISTS (
         SELECT 1 FROM "Tbl_ProductVariant" v
         WHERE v."ProductId" = p."Id" AND v."IsDeleted" = false AND v."Status" = 'Active'))::int,
    (SELECT COUNT(*) FROM "Tbl_Producer" p WHERE p."IsDeleted" = false)::int,
    (SELECT COUNT(*) FROM "Tbl_Producer" p WHERE p."IsDeleted" = false AND p."PublicStatus" = 'Published')::int,
    (SELECT COUNT(*) FROM "Tbl_Producer" p WHERE p."IsDeleted" = false AND p."IsVerified" = true)::int,
    (SELECT COUNT(*) FROM inventory_by_variant)::int,
    (SELECT COALESCE(SUM("StockedQuantity"), 0) FROM inventory_by_variant),
    (SELECT COALESCE(SUM("ReservedQuantity"), 0) FROM inventory_by_variant),
    (SELECT COALESCE(SUM("IncomingQuantity"), 0) FROM inventory_by_variant),
    (SELECT COUNT(*) FROM inventory_by_variant WHERE "StockedQuantity" - "ReservedQuantity" <= 0)::int,
    (SELECT COUNT(*) FROM "Tbl_User" u WHERE u."IsDeleted" = false)::int,
    (SELECT COUNT(*) FROM "Tbl_User" u
     WHERE u."IsDeleted" = false AND u."CreatedAt" >= @fromUtc AND u."CreatedAt" < @toExclusiveUtc)::int;
""";
            AddRangeParameters(command, range);
            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            if (!await reader.ReadAsync(cancellationToken))
                throw new InvalidOperationException("Dashboard snapshot query did not return a row.");

            var currentPending = reader.GetInt32(0);
            var totalProducts = reader.GetInt32(1);
            var draftProducts = reader.GetInt32(2);
            var reviewProducts = reader.GetInt32(3);
            var publishedProducts = reader.GetInt32(4);
            var pausedProducts = reader.GetInt32(5);
            var discontinuedProducts = reader.GetInt32(6);
            var activeVariants = reader.GetInt32(7);
            var sellableActiveVariants = reader.GetInt32(8);
            var productsWithoutActiveVariant = reader.GetInt32(9);
            var totalProducers = reader.GetInt32(10);
            var publishedProducers = reader.GetInt32(11);
            var verifiedProducers = reader.GetInt32(12);
            var trackedVariantCount = reader.GetInt32(13);
            var stockedQuantity = reader.GetDecimal(14);
            var reservedQuantity = reader.GetDecimal(15);
            var incomingQuantity = reader.GetDecimal(16);
            var outOfStockVariantCount = reader.GetInt32(17);
            var totalRegistered = reader.GetInt32(18);
            var newRegisteredInPeriod = reader.GetInt32(19);

            return new ManagementDashboardSnapshotDto(
                currentPending,
                new ManagementDashboardCatalogDto(totalProducts, draftProducts, reviewProducts, publishedProducts,
                    pausedProducts, discontinuedProducts, activeVariants, sellableActiveVariants, productsWithoutActiveVariant),
                new ManagementDashboardProducersDto(totalProducers, publishedProducers, verifiedProducers,
                    totalProducers - verifiedProducers),
                new ManagementDashboardInventoryDto(trackedVariantCount, stockedQuantity, reservedQuantity,
                    stockedQuantity - reservedQuantity, incomingQuantity, outOfStockVariantCount),
                new ManagementDashboardUsersDto(totalRegistered, newRegisteredInPeriod));
        }, cancellationToken);

    private async Task<ManagementOrderAnalyticsKpisDto> ReadKpisAsync(DbConnection connection, ManagementAnalyticsRange range,
        CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = """
SELECT
    (SELECT COUNT(*) FROM "Tbl_Order" o
     WHERE o."IsDeleted" = false AND o."PlacedAt" >= @fromUtc AND o."PlacedAt" < @toExclusiveUtc)::int,
    (SELECT COUNT(*) FROM "Tbl_OrderStatusHistory" h
     JOIN "Tbl_Order" o ON o."Id" = h."OrderId" AND o."IsDeleted" = false
     WHERE h."IsDeleted" = false AND h."ToStatus" = 'Completed'
       AND h."ChangedAt" >= @fromUtc AND h."ChangedAt" < @toExclusiveUtc)::int,
    (SELECT COALESCE(SUM(t."Amount") FILTER (WHERE t."Status" = 'Paid' AND t."TransactionType" IN ('Capture', 'Verify')), 0)
     FROM "Tbl_PaymentTransaction" t
     JOIN "Tbl_Payment" p ON p."Id" = t."PaymentId" AND p."IsDeleted" = false
     WHERE t."IsDeleted" = false AND t."OccurredAt" >= @fromUtc AND t."OccurredAt" < @toExclusiveUtc),
    (SELECT COALESCE(SUM(t."Amount") FILTER (WHERE t."Status" = 'Refunded' AND t."TransactionType" = 'Refund'), 0)
     FROM "Tbl_PaymentTransaction" t
     JOIN "Tbl_Payment" p ON p."Id" = t."PaymentId" AND p."IsDeleted" = false
     WHERE t."IsDeleted" = false AND t."OccurredAt" >= @fromUtc AND t."OccurredAt" < @toExclusiveUtc),
    (SELECT COALESCE(SUM(o."GrandTotalAmount"), 0)
     FROM "Tbl_OrderStatusHistory" h
     JOIN "Tbl_Order" o ON o."Id" = h."OrderId" AND o."IsDeleted" = false
     WHERE h."IsDeleted" = false AND h."ToStatus" = 'Completed'
       AND h."ChangedAt" >= @fromUtc AND h."ChangedAt" < @toExclusiveUtc);
""";
        AddRangeParameters(command, range);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
            throw new InvalidOperationException("Order analytics KPI query did not return a row.");

        var collectedGross = reader.GetDecimal(2);
        var refundAmount = reader.GetDecimal(3);
        return new ManagementOrderAnalyticsKpisDto(reader.GetInt32(0), reader.GetInt32(1), collectedGross,
            refundAmount, collectedGross - refundAmount, reader.GetDecimal(4));
    }

    private async Task<IReadOnlyList<ManagementOrderAnalyticsSeriesItemDto>> ReadSeriesAsync(DbConnection connection,
        ManagementAnalyticsRange range, ManagementOrderAnalyticsGranularity granularity, CancellationToken cancellationToken)
    {
        var interval = granularity switch
        {
            ManagementOrderAnalyticsGranularity.Week => "7 days",
            ManagementOrderAnalyticsGranularity.Month => "1 month",
            _ => "1 day"
        };
        var bucketExpression = granularity switch
        {
            ManagementOrderAnalyticsGranularity.Week => "date_trunc('week', {0} AT TIME ZONE 'Asia/Ho_Chi_Minh')::date",
            ManagementOrderAnalyticsGranularity.Month => "date_trunc('month', {0} AT TIME ZONE 'Asia/Ho_Chi_Minh')::date",
            _ => "({0} AT TIME ZONE 'Asia/Ho_Chi_Minh')::date"
        };
        var orderBucket = string.Format(CultureInfo.InvariantCulture, bucketExpression, "o.\"PlacedAt\"");
        var completedBucket = string.Format(CultureInfo.InvariantCulture, bucketExpression, "h.\"ChangedAt\"");
        var cashBucket = string.Format(CultureInfo.InvariantCulture, bucketExpression, "t.\"OccurredAt\"");

        await using var command = connection.CreateCommand();
        command.CommandText = $"""
WITH buckets AS (
    SELECT generate_series(@bucketStart::date, @toDate::date, INTERVAL '{interval}')::date AS "Period"
), order_aggregate AS (
    SELECT {orderBucket} AS "Period", COUNT(*)::int AS "OrdersPlaced"
    FROM "Tbl_Order" o
    WHERE o."IsDeleted" = false AND o."PlacedAt" >= @fromUtc AND o."PlacedAt" < @toExclusiveUtc
    GROUP BY 1
), completed_aggregate AS (
    SELECT {completedBucket} AS "Period", COUNT(*)::int AS "CompletedOrderCount",
           COALESCE(SUM(o."GrandTotalAmount"), 0) AS "CompletedSales"
    FROM "Tbl_OrderStatusHistory" h
    JOIN "Tbl_Order" o ON o."Id" = h."OrderId" AND o."IsDeleted" = false
    WHERE h."IsDeleted" = false AND h."ToStatus" = 'Completed'
      AND h."ChangedAt" >= @fromUtc AND h."ChangedAt" < @toExclusiveUtc
    GROUP BY 1
), cash_aggregate AS (
    SELECT {cashBucket} AS "Period",
           COALESCE(SUM(t."Amount") FILTER (WHERE t."Status" = 'Paid' AND t."TransactionType" IN ('Capture', 'Verify')), 0) AS "CollectedGross",
           COALESCE(SUM(t."Amount") FILTER (WHERE t."Status" = 'Refunded' AND t."TransactionType" = 'Refund'), 0) AS "RefundAmount"
    FROM "Tbl_PaymentTransaction" t
    JOIN "Tbl_Payment" p ON p."Id" = t."PaymentId" AND p."IsDeleted" = false
    WHERE t."IsDeleted" = false AND t."OccurredAt" >= @fromUtc AND t."OccurredAt" < @toExclusiveUtc
    GROUP BY 1
)
SELECT to_char(b."Period", 'YYYY-MM-DD'),
       COALESCE(o."OrdersPlaced", 0),
       COALESCE(c."CompletedOrderCount", 0),
       COALESCE(m."CollectedGross", 0),
       COALESCE(m."RefundAmount", 0),
       COALESCE(m."CollectedGross", 0) - COALESCE(m."RefundAmount", 0),
       COALESCE(c."CompletedSales", 0)
FROM buckets b
LEFT JOIN order_aggregate o ON o."Period" = b."Period"
LEFT JOIN completed_aggregate c ON c."Period" = b."Period"
LEFT JOIN cash_aggregate m ON m."Period" = b."Period"
ORDER BY b."Period";
""";
        AddRangeParameters(command, range);
        AddParameter(command, "@bucketStart", ManagementAnalyticsRange.StartOfBucket(range.From, granularity));
        AddParameter(command, "@toDate", range.To);
        var result = new List<ManagementOrderAnalyticsSeriesItemDto>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
            result.Add(new ManagementOrderAnalyticsSeriesItemDto(reader.GetString(0), reader.GetInt32(1), reader.GetInt32(2),
                reader.GetDecimal(3), reader.GetDecimal(4), reader.GetDecimal(5), reader.GetDecimal(6)));
        return result;
    }

    private async Task<IReadOnlyList<ManagementOrderStatusBreakdownDto>> ReadStatusBreakdownAsync(DbConnection connection,
        ManagementAnalyticsRange range, CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = """
SELECT o."Status", COUNT(*)::int, COALESCE(SUM(o."GrandTotalAmount"), 0)
FROM "Tbl_Order" o
WHERE o."IsDeleted" = false AND o."PlacedAt" >= @fromUtc AND o."PlacedAt" < @toExclusiveUtc
GROUP BY o."Status"
ORDER BY o."Status";
""";
        AddRangeParameters(command, range);
        var result = new List<ManagementOrderStatusBreakdownDto>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
            result.Add(new ManagementOrderStatusBreakdownDto(Enum.Parse<OrderStatus>(reader.GetString(0)), reader.GetInt32(1), reader.GetDecimal(2)));
        return result;
    }

    private async Task<IReadOnlyList<ManagementPaymentMethodCashBreakdownDto>> ReadPaymentMethodBreakdownAsync(
        DbConnection connection, ManagementAnalyticsRange range, CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = """
SELECT p."Method",
       COALESCE(SUM(t."Amount") FILTER (WHERE t."Status" = 'Paid' AND t."TransactionType" IN ('Capture', 'Verify')), 0),
       COALESCE(SUM(t."Amount") FILTER (WHERE t."Status" = 'Refunded' AND t."TransactionType" = 'Refund'), 0)
FROM "Tbl_PaymentTransaction" t
JOIN "Tbl_Payment" p ON p."Id" = t."PaymentId" AND p."IsDeleted" = false
WHERE t."IsDeleted" = false AND t."OccurredAt" >= @fromUtc AND t."OccurredAt" < @toExclusiveUtc
  AND ((t."Status" = 'Paid' AND t."TransactionType" IN ('Capture', 'Verify'))
       OR (t."Status" = 'Refunded' AND t."TransactionType" = 'Refund'))
GROUP BY p."Method"
ORDER BY p."Method";
""";
        AddRangeParameters(command, range);
        var result = new List<ManagementPaymentMethodCashBreakdownDto>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            var collectedGross = reader.GetDecimal(1);
            var refundAmount = reader.GetDecimal(2);
            result.Add(new ManagementPaymentMethodCashBreakdownDto(Enum.Parse<PaymentMethod>(reader.GetString(0)),
                collectedGross, refundAmount, collectedGross - refundAmount));
        }
        return result;
    }

    private async Task<IReadOnlyList<ManagementTopProductSalesDto>> ReadTopProductsAsync(DbConnection connection,
        ManagementAnalyticsRange range, int topLimit, CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = """
SELECT i."ProductVariantId", i."ProductNameSnapshot", i."VariantNameSnapshot", i."SkuSnapshot",
       SUM(i."Quantity")::int, COALESCE(SUM(i."LineTotalAmount"), 0)
FROM "Tbl_OrderItem" i
JOIN "Tbl_OrderStatusHistory" h ON h."OrderId" = i."OrderId" AND h."IsDeleted" = false
JOIN "Tbl_Order" o ON o."Id" = i."OrderId" AND o."IsDeleted" = false
WHERE i."IsDeleted" = false AND h."ToStatus" = 'Completed'
  AND h."ChangedAt" >= @fromUtc AND h."ChangedAt" < @toExclusiveUtc
GROUP BY i."ProductVariantId", i."ProductNameSnapshot", i."VariantNameSnapshot", i."SkuSnapshot"
ORDER BY COALESCE(SUM(i."LineTotalAmount"), 0) DESC, SUM(i."Quantity") DESC, i."SkuSnapshot"
LIMIT @topLimit;
""";
        AddRangeParameters(command, range);
        AddParameter(command, "@topLimit", topLimit);
        var result = new List<ManagementTopProductSalesDto>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
            result.Add(new ManagementTopProductSalesDto(reader.IsDBNull(0) ? null : reader.GetGuid(0), reader.GetString(1),
                reader.GetString(2), reader.GetString(3), reader.GetInt32(4), reader.GetDecimal(5)));
        return result;
    }

    private async Task<T> WithOpenConnectionAsync<T>(Func<DbConnection, Task<T>> action, CancellationToken cancellationToken)
    {
        var connection = db.Database.GetDbConnection();
        var closeWhenFinished = connection.State != ConnectionState.Open;
        if (closeWhenFinished)
            await db.Database.OpenConnectionAsync(cancellationToken);
        try
        {
            return await action(connection);
        }
        finally
        {
            if (closeWhenFinished)
                await db.Database.CloseConnectionAsync();
        }
    }

    private static void AddRangeParameters(DbCommand command, ManagementAnalyticsRange range)
    {
        AddParameter(command, "@fromUtc", range.FromUtc);
        AddParameter(command, "@toExclusiveUtc", range.ToExclusiveUtc);
    }

    private static void AddParameter(DbCommand command, string name, object value)
    {
        var parameter = command.CreateParameter();
        parameter.ParameterName = name;
        parameter.Value = value;
        command.Parameters.Add(parameter);
    }
}
