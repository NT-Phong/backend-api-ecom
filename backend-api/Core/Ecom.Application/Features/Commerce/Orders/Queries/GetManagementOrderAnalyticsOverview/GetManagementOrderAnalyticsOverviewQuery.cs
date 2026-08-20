using System.Globalization;
using Ecom.Domain.Entities;

namespace Ecom.Application.Features.Commerce.Orders.Queries.GetManagementOrderAnalyticsOverview;

public enum ManagementOrderAnalyticsGranularity { Day, Week, Month }

public sealed record GetManagementOrderAnalyticsOverviewQuery : IRequest<TResult<ManagementOrderAnalyticsOverviewDto>>
{
    public DateOnly? From { get; init; }
    public DateOnly? To { get; init; }
    public ManagementOrderAnalyticsGranularity Granularity { get; init; } = ManagementOrderAnalyticsGranularity.Day;
    public int TopLimit { get; init; } = 10;
}

public sealed class GetManagementOrderAnalyticsOverviewQueryValidator : AbstractValidator<GetManagementOrderAnalyticsOverviewQuery>
{
    public GetManagementOrderAnalyticsOverviewQueryValidator()
    {
        RuleFor(x => x.Granularity).IsInEnum();
        RuleFor(x => x.TopLimit).InclusiveBetween(1, 50);
        RuleFor(x => x).Must(x => !x.From.HasValue || !x.To.HasValue || x.From <= x.To)
            .WithMessage("From must be earlier than or equal to To.");
        RuleFor(x => x).Must(x => !x.From.HasValue || !x.To.HasValue || x.To.Value.DayNumber - x.From.Value.DayNumber < 366)
            .WithMessage("The reporting range cannot exceed 366 calendar days.");
    }
}

public sealed class GetManagementOrderAnalyticsOverviewQueryHandler(IUnitOfWork unitOfWork, ICurrentUser currentUser)
    : IRequestHandler<GetManagementOrderAnalyticsOverviewQuery, TResult<ManagementOrderAnalyticsOverviewDto>>
{
    private static readonly TimeZoneInfo VietnamTimeZone = ResolveVietnamTimeZone();

    public async Task<TResult<ManagementOrderAnalyticsOverviewDto>> Handle(
        GetManagementOrderAnalyticsOverviewQuery request, CancellationToken cancellationToken)
    {
        if (!currentUser.IsAuthenticated)
            return TResult<ManagementOrderAnalyticsOverviewDto>.Failure(MessageKey.Unauthorized, ErrorCodes.UNAUTHORIZED);
        if (!currentUser.HasPolicy(Permissions.Orders.Read))
            return TResult<ManagementOrderAnalyticsOverviewDto>.Failure(MessageKey.Forbidden, ErrorCodes.FORBIDDEN);

        var today = DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, VietnamTimeZone));
        var to = request.To ?? today;
        var from = request.From ?? (request.To.HasValue ? to.AddDays(-29) : today.AddDays(-29));
        if (from > to)
            return TResult<ManagementOrderAnalyticsOverviewDto>.ValidationFailure(new Dictionary<string, string[]>
            {
                ["from"] = ["From must be earlier than or equal to To."]
            });
        if (to.DayNumber - from.DayNumber >= 366)
            return TResult<ManagementOrderAnalyticsOverviewDto>.ValidationFailure(new Dictionary<string, string[]>
            {
                ["range"] = ["The reporting range cannot exceed 366 calendar days."]
            });
        var fromUtc = ToUtc(from);
        var toExclusiveUtc = ToUtc(to.AddDays(1));

        var orderRows = await unitOfWork.Repository<Order>().QueryNoTracking()
            .Where(x => x.PlacedAt >= fromUtc && x.PlacedAt < toExclusiveUtc)
            .Select(x => new OrderRow(x.Id, x.Status, x.GrandTotalAmount, x.PlacedAt))
            .ToListAsync(cancellationToken);
        var completedRows = await (
            from history in unitOfWork.Repository<OrderStatusHistory>().QueryNoTracking()
            join order in unitOfWork.Repository<Order>().QueryNoTracking() on history.OrderId equals order.Id
            where history.ToStatus == OrderStatus.Completed
                  && history.ChangedAt >= fromUtc
                  && history.ChangedAt < toExclusiveUtc
            select new CompletedOrderRow(order.Id, order.GrandTotalAmount, history.ChangedAt))
            .ToListAsync(cancellationToken);
        var cashRows = await (
            from transaction in unitOfWork.Repository<PaymentTransaction>().QueryNoTracking()
            join payment in unitOfWork.Repository<Payment>().QueryNoTracking() on transaction.PaymentId equals payment.Id
            where transaction.OccurredAt >= fromUtc && transaction.OccurredAt < toExclusiveUtc
                  && ((transaction.Status == PaymentStatus.Paid &&
                       (transaction.TransactionType == PaymentTransactionType.Capture || transaction.TransactionType == PaymentTransactionType.Verify)) ||
                      (transaction.Status == PaymentStatus.Refunded && transaction.TransactionType == PaymentTransactionType.Refund))
            select new CashTransactionRow(payment.Method, transaction.TransactionType, transaction.Status,
                transaction.Amount, transaction.OccurredAt))
            .ToListAsync(cancellationToken);
        var topProducts = await (
            from item in unitOfWork.Repository<OrderItem>().QueryNoTracking()
            join history in unitOfWork.Repository<OrderStatusHistory>().QueryNoTracking() on item.OrderId equals history.OrderId
            where history.ToStatus == OrderStatus.Completed
                  && history.ChangedAt >= fromUtc
                  && history.ChangedAt < toExclusiveUtc
            group item by new { item.ProductVariantId, item.ProductNameSnapshot, item.VariantNameSnapshot, item.SkuSnapshot } into items
            select new ManagementTopProductSalesDto(items.Key.ProductVariantId, items.Key.ProductNameSnapshot,
                items.Key.VariantNameSnapshot, items.Key.SkuSnapshot, items.Sum(x => x.Quantity),
                items.Sum(x => x.LineTotalAmount)))
            .OrderByDescending(x => x.SalesAmount)
            .ThenByDescending(x => x.QuantitySold)
            .ThenBy(x => x.Sku)
            .Take(request.TopLimit)
            .ToListAsync(cancellationToken);

        var collectedRows = cashRows.Where(x => x.Status == PaymentStatus.Paid &&
            (x.TransactionType == PaymentTransactionType.Capture || x.TransactionType == PaymentTransactionType.Verify)).ToList();
        var refundRows = cashRows.Where(x => x.Status == PaymentStatus.Refunded && x.TransactionType == PaymentTransactionType.Refund).ToList();
        var buckets = CreateBuckets(from, to, request.Granularity);
        foreach (var order in orderRows)
            buckets[GetPeriod(order.PlacedAt, request.Granularity)].OrdersPlaced++;
        foreach (var completed in completedRows)
        {
            var bucket = buckets[GetPeriod(completed.CompletedAt, request.Granularity)];
            bucket.CompletedOrderCount++;
            bucket.CompletedSales += completed.GrandTotalAmount;
        }
        foreach (var collected in collectedRows)
            buckets[GetPeriod(collected.OccurredAt, request.Granularity)].CollectedGross += collected.Amount;
        foreach (var refund in refundRows)
            buckets[GetPeriod(refund.OccurredAt, request.Granularity)].RefundAmount += refund.Amount;

        var series = buckets.Select(x => new ManagementOrderAnalyticsSeriesItemDto(x.Key, x.Value.OrdersPlaced,
            x.Value.CompletedOrderCount, x.Value.CollectedGross, x.Value.RefundAmount,
            x.Value.CollectedGross - x.Value.RefundAmount, x.Value.CompletedSales)).ToList();
        var statusBreakdown = orderRows.GroupBy(x => x.Status)
            .OrderBy(x => x.Key)
            .Select(x => new ManagementOrderStatusBreakdownDto(x.Key, x.Count(), x.Sum(order => order.GrandTotalAmount)))
            .ToList();
        var paymentMethodBreakdown = collectedRows.Concat(refundRows)
            .GroupBy(x => x.PaymentMethod)
            .OrderBy(x => x.Key)
            .Select(x => new ManagementPaymentMethodCashBreakdownDto(x.Key,
                x.Where(transaction => transaction.Status == PaymentStatus.Paid).Sum(transaction => transaction.Amount),
                x.Where(transaction => transaction.Status == PaymentStatus.Refunded).Sum(transaction => transaction.Amount),
                x.Where(transaction => transaction.Status == PaymentStatus.Paid).Sum(transaction => transaction.Amount) -
                x.Where(transaction => transaction.Status == PaymentStatus.Refunded).Sum(transaction => transaction.Amount)))
            .ToList();

        var kpis = new ManagementOrderAnalyticsKpisDto(orderRows.Count, completedRows.Count,
            collectedRows.Sum(x => x.Amount), refundRows.Sum(x => x.Amount),
            collectedRows.Sum(x => x.Amount) - refundRows.Sum(x => x.Amount), completedRows.Sum(x => x.GrandTotalAmount));
        return TResult<ManagementOrderAnalyticsOverviewDto>.Success(new ManagementOrderAnalyticsOverviewDto(
            CommerceConstants.DefaultCurrency, Format(from), Format(to), kpis, series, statusBreakdown,
            paymentMethodBreakdown, topProducts));
    }

    private static SortedDictionary<string, BucketTotals> CreateBuckets(DateOnly from, DateOnly to,
        ManagementOrderAnalyticsGranularity granularity)
    {
        var result = new SortedDictionary<string, BucketTotals>();
        var current = granularity switch
        {
            ManagementOrderAnalyticsGranularity.Week => StartOfWeek(from),
            ManagementOrderAnalyticsGranularity.Month => new DateOnly(from.Year, from.Month, 1),
            _ => from
        };
        while (current <= to)
        {
            result.Add(Format(current), new BucketTotals());
            current = granularity switch
            {
                ManagementOrderAnalyticsGranularity.Week => current.AddDays(7),
                ManagementOrderAnalyticsGranularity.Month => current.AddMonths(1),
                _ => current.AddDays(1)
            };
        }
        return result;
    }

    private static string GetPeriod(DateTime timestamp, ManagementOrderAnalyticsGranularity granularity)
    {
        var local = DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(AsUtc(timestamp), VietnamTimeZone));
        return Format(granularity switch
        {
            ManagementOrderAnalyticsGranularity.Week => StartOfWeek(local),
            ManagementOrderAnalyticsGranularity.Month => new DateOnly(local.Year, local.Month, 1),
            _ => local
        });
    }

    private static DateOnly StartOfWeek(DateOnly value) => value.AddDays(-((int)value.DayOfWeek + 6) % 7);
    private static DateTime ToUtc(DateOnly value) => TimeZoneInfo.ConvertTimeToUtc(
        DateTime.SpecifyKind(value.ToDateTime(TimeOnly.MinValue), DateTimeKind.Unspecified), VietnamTimeZone);
    private static DateTime AsUtc(DateTime value) => value.Kind == DateTimeKind.Utc ? value : DateTime.SpecifyKind(value, DateTimeKind.Utc);
    private static string Format(DateOnly value) => value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

    private static TimeZoneInfo ResolveVietnamTimeZone()
    {
        try { return TimeZoneInfo.FindSystemTimeZoneById("Asia/Ho_Chi_Minh"); }
        catch (TimeZoneNotFoundException) { return TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"); }
    }

    private sealed record OrderRow(Guid Id, OrderStatus Status, decimal GrandTotalAmount, DateTime PlacedAt);
    private sealed record CompletedOrderRow(Guid OrderId, decimal GrandTotalAmount, DateTime CompletedAt);
    private sealed record CashTransactionRow(PaymentMethod PaymentMethod, PaymentTransactionType TransactionType,
        PaymentStatus Status, decimal Amount, DateTime OccurredAt);

    private sealed class BucketTotals
    {
        public int OrdersPlaced { get; set; }
        public int CompletedOrderCount { get; set; }
        public decimal CollectedGross { get; set; }
        public decimal RefundAmount { get; set; }
        public decimal CompletedSales { get; set; }
    }
}
