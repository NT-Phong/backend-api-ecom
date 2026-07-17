namespace Ecom.Infrastructure.Services;

public class DateTimeService : IDateTimeService
{
    public DateTimeOffset UtcNow => DateTime.UtcNow;
    public DateTimeOffset Now => DateTimeOffset.Now;
    public DateOnly Today => DateOnly.FromDateTime(DateTime.Today);
    public TimeOnly TimeNow => TimeOnly.FromDateTime(DateTime.Now);
} 
