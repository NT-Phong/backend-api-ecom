namespace Ecom.Domain.Constants;

public static class DomainConstants
{
    // System user GUID - fixed GUID for system operations
    public static readonly Guid SystemUser = Guid.Parse("00000000-0000-0000-0000-000000000001");
    
    public static class Cache
    {
        public const int DefaultExpirationMinutes = 30;
        public const int LongExpirationMinutes = 120;
    }
    
    public static class RateLimit
    {
        public const int DefaultRequestsPerMinute = 60;
        public const int DefaultRequestsPerHour = 1000;
    }

    public static class ReportConstants
    {
        //Các khoảng ngày tuổi (DOC) cho báo cáo phân bố ngày tuổi
        public static readonly List<(string Label, int Min, int Max)> DocRanges = new()
    {
        ("< 40", 0, 39),
        ("60-70", 60, 70),
        ("70-80", 71, 80),
        ("> 80", 81, 999)
    };

        // Các chế độ xem cho báo cáo phân bố ngày tuổi
        public const string ViewModeArea = "Area"; // Biểu đồ phân bố ngày tuổi theo diện tích (Area)
        public const string ViewModeDOC = "DOC"; // Biểu đồ phân bố ngày tuổi (DOC)
    }
} 
