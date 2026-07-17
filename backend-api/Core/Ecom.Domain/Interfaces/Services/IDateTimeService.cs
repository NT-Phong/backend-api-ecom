namespace Ecom.Domain.Interfaces.Services;

public interface IDateTimeService
{
    DateTimeOffset UtcNow { get; }
    DateTimeOffset Now { get; }
    DateOnly Today { get; }
    TimeOnly TimeNow { get; }
} 
