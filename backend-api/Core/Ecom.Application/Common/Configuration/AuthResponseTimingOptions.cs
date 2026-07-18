namespace Ecom.Application.Common.Configuration;

public sealed class AuthResponseTimingOptions
{
    public const string SectionName = "AuthenticationResponseTiming";
    public int MinimumPublicResponseMilliseconds { get; set; } = 250;
}
