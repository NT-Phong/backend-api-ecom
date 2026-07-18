namespace Ecom.Application.Common.Configuration;

public sealed class PasswordAuthenticationV2Options
{
    public const string SectionName = "PasswordAuthenticationV2";
    public bool Enabled { get; set; }
    public bool ExposeDevelopmentVerificationToken { get; set; }
    public int AccessTokenMinutes { get; set; } = 10;
    public int RefreshTokenDays { get; set; } = 30;
}
