using System.Globalization;

namespace Ecom.Application.Common.Models;

/// <summary>
/// Model cho date range filter
/// Dùng cho các API cần filter theo khoảng thời gian
/// Input format: "from~to" (ISO 8601)
/// Ví dụ: "2025-12-19T00:00:00.000Z~2025-12-26T23:59:59.999Z"
/// </summary>
public class DateTimeRange
{
    /// <summary>
    /// Ngày bắt đầu (inclusive)
    /// </summary>
    public DateTime? From { get; set; }
    
    /// <summary>
    /// Ngày kết thúc (inclusive)
    /// </summary>
    public DateTime? To { get; set; }
    
    /// <summary>
    /// Kiểm tra có giá trị filter không
    /// </summary>
    public bool HasValue => From.HasValue || To.HasValue;
    
    /// <summary>
    /// Parse date range string thành DateTimeRange
    /// Format: "from~to" với from/to là ISO 8601 datetime
    /// Ví dụ: "2025-12-19T00:00:00.000Z~2025-12-26T23:59:59.999Z"
    /// </summary>
    public static DateTimeRange? Parse(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return null;
        
        DateTime? from = null;
        DateTime? to = null;
        
        var parts = input.Split('~');
        if (parts.Length == 2)
        {
            if (DateTime.TryParse(parts[0], CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var f))
                from = DateTime.SpecifyKind(f, DateTimeKind.Utc);
            if (DateTime.TryParse(parts[1], CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var t))
                to = DateTime.SpecifyKind(t, DateTimeKind.Utc);
        }
        
        if (from.HasValue || to.HasValue)
            return new DateTimeRange { From = from, To = to };
            
        return null;
    }
}

