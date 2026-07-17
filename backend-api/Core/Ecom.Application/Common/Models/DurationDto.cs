namespace Ecom.Application.Common.Models
{
    public class DurationDto
    {
        public int Days { get; set; }
        public int Hours { get; set; }
        public int Minutes { get; set; }

        public static DurationDto FromTimeSpan(TimeSpan? t)
        => t is null
            ? new DurationDto()
            : new DurationDto
            {
                Days = t.Value.Days,
                Hours = t.Value.Hours,
                Minutes = t.Value.Minutes
            };

    }
}

