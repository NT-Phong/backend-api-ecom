using System.Globalization;

namespace Ecom.Domain.Extensions;

public static class StringParsingExtensions
{
    private static readonly string[] DateFormats =
    [
        "yyyy-MM-ddTHH:mm:ss.fffZ", "yyyy-MM-ddTHH:mm:ssZ", "yyyy-MM-ddTHH:mm:ss",
        "yyyy-MM-dd", "dd/MM/yyyy HH:mm:ss", "dd/MM/yyyy", "MM/dd/yyyy"
    ];

    private static readonly char[] ListDelimiters = [',', ';'];
    private static readonly char[] RangeDelimiter = ['~'];

    public static List<T> ToInternalList<T>(this string? input) where T : struct
    {
        if (string.IsNullOrWhiteSpace(input)) return [];

        // Sử dụng mảng static đã khai báo
        return input.Split(ListDelimiters, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                    .Select(x => x.ToInternalValue<T>())
                    .OfType<T>()
                    .ToList();
    }

    public static (T? From, T? To) ToInternalRange<T>(this string? input, bool autoSwap = false)
        where T : struct, IComparable<T>
    {
        if (string.IsNullOrWhiteSpace(input)) return (null, null);

        // Sử dụng mảng static và giới hạn 2 phần tử
        var parts = input.Split(RangeDelimiter, 2, StringSplitOptions.TrimEntries);

        var from = parts.Length > 0 ? parts[0].ToInternalValue<T>() : null;
        var to = parts.Length > 1 ? parts[1].ToInternalValue<T>() : from;

        if (autoSwap && from.HasValue && to.HasValue && from.Value.CompareTo(to.Value) > 0)
            (from, to) = (to, from);

        if (to is DateTime toDate && toDate.TimeOfDay == TimeSpan.Zero)
            to = (T)(object)toDate.Date.AddDays(1).AddTicks(-1);

        return (from, to);
    }

    private static T? ToInternalValue<T>(this string? value) where T : struct
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        var type = typeof(T);

        try
        {
            if (type.IsEnum) return Enum.TryParse<T>(value, true, out var e) ? e : null;
            if (type == typeof(Guid)) return Guid.TryParse(value, out var g) ? (T)(object)g : null;
            if (type == typeof(DateTime)) return TryParseDateTime(value, out var dt) ? (T)(object)dt : null;

            if (type == typeof(bool))
            {
                var v = value.ToLower().Trim();
                if (v is "true" or "1" or "yes") return (T)(object)true;
                if (v is "false" or "0" or "no") return (T)(object)false;
                return null;
            }

            return (T)Convert.ChangeType(value, type, CultureInfo.InvariantCulture);
        }
        catch { return null; }
    }

    private static bool TryParseDateTime(string s, out DateTime dt) =>
        DateTime.TryParseExact(s, DateFormats, CultureInfo.InvariantCulture,
            DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out dt)
        || DateTime.TryParse(s, CultureInfo.InvariantCulture, DateTimeStyles.None, out dt);
}

