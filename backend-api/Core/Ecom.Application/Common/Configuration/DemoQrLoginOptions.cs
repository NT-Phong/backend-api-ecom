namespace Ecom.Application.Common.Configuration;

public sealed class DemoQrLoginOptions
{
    public const string SectionName = "DemoQrLogin";

    public bool Enabled { get; set; }
    public int TtlSeconds { get; set; } = 120;
    public int PollIntervalMilliseconds { get; set; } = 1500;
}
