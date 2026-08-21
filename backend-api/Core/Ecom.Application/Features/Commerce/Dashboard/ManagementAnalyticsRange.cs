using Ecom.Application.Features.Commerce.Orders.Queries.GetManagementOrderAnalyticsOverview;

namespace Ecom.Application.Features.Commerce.Dashboard;

public sealed record ManagementAnalyticsRange(DateOnly From, DateOnly To, DateTime FromUtc, DateTime ToExclusiveUtc)
{
    public const string VietnamTimeZoneId = "Asia/Ho_Chi_Minh";
    private static readonly TimeZoneInfo VietnamTimeZone = ResolveVietnamTimeZone();

    public static bool TryCreate(
        DateOnly? requestedFrom,
        DateOnly? requestedTo,
        ManagementOrderAnalyticsGranularity granularity,
        int topLimit,
        out ManagementAnalyticsRange range,
        out Dictionary<string, string[]> validationErrors)
    {
        validationErrors = new Dictionary<string, string[]>();
        if (!Enum.IsDefined(granularity))
            validationErrors["granularity"] = ["Granularity is invalid."];
        if (topLimit is < 1 or > 50)
            validationErrors["topLimit"] = ["TopLimit must be between 1 and 50."];

        var today = DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, VietnamTimeZone));
        var to = requestedTo ?? today;
        var from = requestedFrom ?? (requestedTo.HasValue ? to.AddDays(-29) : today.AddDays(-29));
        if (from > to)
            validationErrors["from"] = ["From must be earlier than or equal to To."];
        if (to.DayNumber - from.DayNumber >= 366)
            validationErrors["range"] = ["The reporting range cannot exceed 366 calendar days."];

        if (validationErrors.Count > 0)
        {
            range = null!;
            return false;
        }

        range = new ManagementAnalyticsRange(from, to, ToUtc(from), ToUtc(to.AddDays(1)));
        return true;
    }

    public static DateOnly StartOfBucket(DateOnly value, ManagementOrderAnalyticsGranularity granularity) => granularity switch
    {
        ManagementOrderAnalyticsGranularity.Week => value.AddDays(-((int)value.DayOfWeek + 6) % 7),
        ManagementOrderAnalyticsGranularity.Month => new DateOnly(value.Year, value.Month, 1),
        _ => value
    };

    private static DateTime ToUtc(DateOnly value) => TimeZoneInfo.ConvertTimeToUtc(
        DateTime.SpecifyKind(value.ToDateTime(TimeOnly.MinValue), DateTimeKind.Unspecified), VietnamTimeZone);

    private static TimeZoneInfo ResolveVietnamTimeZone()
    {
        try { return TimeZoneInfo.FindSystemTimeZoneById(VietnamTimeZoneId); }
        catch (TimeZoneNotFoundException) { return TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"); }
    }
}
