using Ecom.Application.Common.Models;

namespace Ecom.Application.Common.Extensions;

public static class DatetimeExtension
{
    /// <summary>
    /// Get current UTC timestamp in seconds.
    /// </summary>
    public static long NowSeconds()
    {
        return DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    /// <summary>
    /// Convert DateTime to Unix timestamp in seconds (UTC).
    /// </summary>
    public static long TotalSeconds(this DateTime dateTime)
    {
        // Ensure we're working with UTC
        var utcDateTime = dateTime.Kind == DateTimeKind.Utc
            ? dateTime
            : dateTime.ToUniversalTime();
        var dateTimeOffset = new DateTimeOffset(utcDateTime);
        return dateTimeOffset.ToUnixTimeSeconds();
    }

    /// <summary>
    /// Get current UTC DateTime.
    /// </summary>
    public static DateTime UtcNow()
    {
        return DateTime.UtcNow;
    }

    /// <summary>
    /// Convert a nullable DateTime to a DatetimeQueryDto with start and end of day in UTC.
    /// Used for date range queries.
    /// </summary>
    public static DatetimeQueryDto? DatetimeQuery(this DateTime? dateTime)
    {
        if (dateTime == null || dateTime == default(DateTime))
            return null;

        // Convert input to UTC if not already
        var utcDateTime = dateTime.Value.Kind == DateTimeKind.Utc
            ? dateTime.Value
            : dateTime.Value.ToUniversalTime();

        // Get start of day (00:00:00.000) in UTC
        var startOfDay = new DateTime(utcDateTime.Year, utcDateTime.Month, utcDateTime.Day, 0, 0, 0, DateTimeKind.Utc);

        // Get end of day (23:59:59.999) in UTC
        var endOfDay = startOfDay.AddDays(1).AddMilliseconds(-1);

        return new DatetimeQueryDto
        {
            StartDateAt = startOfDay,
            EndDateAt = endOfDay
        };
    }

    /// <summary>
    /// Convert DateTime to UTC. If already UTC, returns as-is.
    /// </summary>
    public static DateTime ToUtc(this DateTime dateTime)
    {
        return dateTime.Kind == DateTimeKind.Utc
            ? dateTime
            : DateTime.SpecifyKind(dateTime.ToUniversalTime(), DateTimeKind.Utc);
    }

    /// <summary>
    /// Convert nullable DateTime to UTC.
    /// </summary>
    public static DateTime? ToUtc(this DateTime? dateTime)
    {
        return dateTime?.ToUtc();
    }

    /// <summary>
    /// Get start of day in UTC for the given date.
    /// </summary>
    public static DateTime StartOfDayUtc(this DateTime dateTime)
    {
        var utcDateTime = dateTime.ToUtc();
        return new DateTime(utcDateTime.Year, utcDateTime.Month, utcDateTime.Day, 0, 0, 0, DateTimeKind.Utc);
    }

    /// <summary>
    /// Get end of day in UTC for the given date (23:59:59.999).
    /// </summary>
    public static DateTime EndOfDayUtc(this DateTime dateTime)
    {
        return dateTime.StartOfDayUtc().AddDays(1).AddMilliseconds(-1);
    }

    /// <summary>
    /// Get start of month in UTC.
    /// </summary>
    public static DateTime StartOfMonthUtc(this DateTime dateTime)
    {
        var utcDateTime = dateTime.ToUtc();
        return new DateTime(utcDateTime.Year, utcDateTime.Month, 1, 0, 0, 0, DateTimeKind.Utc);
    }

    /// <summary>
    /// Get end of month in UTC.
    /// </summary>
    public static DateTime EndOfMonthUtc(this DateTime dateTime)
    {
        return dateTime.StartOfMonthUtc().AddMonths(1).AddMilliseconds(-1);
    }
}
