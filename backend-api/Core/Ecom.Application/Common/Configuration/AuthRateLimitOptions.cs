namespace Ecom.Application.Common.Configuration;

public static class AuthRateLimitPolicyNames
{
    public const string RegisterIp = "auth-register-ip";
    public const string OtpSendIp = "auth-otp-send-ip";
    public const string OtpVerifyIp = "auth-otp-verify-ip";
    public const string RefreshIp = "auth-refresh-ip";
    public const string PasswordLoginIp = "auth-password-login-ip";
    public const string RegisterDestinationDaily = "auth-register-destination-daily";
    public const string OtpSendDestinationBurst = "auth-otp-send-destination-burst";
    public const string OtpSendDestinationDaily = "auth-otp-send-destination-daily";
    public const string OtpVerifyChallenge = "auth-otp-verify-challenge";
    public const string RefreshSession = "auth-refresh-session";
    public const string PasswordLoginAccount = "auth-password-login-account";
    public const string PasswordLoginDevice = "auth-password-login-device";
}

public sealed class AuthRateLimitOptions
{
    public const string SectionName = "AuthenticationRateLimits";
    public string RedisKeyPrefix { get; set; } = "ecom:auth:ratelimit";

    public RateLimitRule RegisterIp { get; set; } = new(5, 3600);
    public RateLimitRule OtpSendIp { get; set; } = new(20, 3600);
    public RateLimitRule OtpVerifyIp { get; set; } = new(30, 900);
    public RateLimitRule RefreshIp { get; set; } = new(60, 60);
    public RateLimitRule PasswordLoginIp { get; set; } = new(30, 900);
    public RateLimitRule RegisterDestinationDaily { get; set; } = new(3, 86400);
    public RateLimitRule OtpSendDestinationBurst { get; set; } = new(3, 900);
    public RateLimitRule OtpSendDestinationDaily { get; set; } = new(10, 86400);
    public RateLimitRule OtpVerifyChallenge { get; set; } = new(5, 900);
    public RateLimitRule RefreshSession { get; set; } = new(30, 60);
    public RateLimitRule PasswordLoginAccount { get; set; } = new(10, 900);
    public RateLimitRule PasswordLoginDevice { get; set; } = new(10, 900);

    public RateLimitRule GetDistributedPolicy(string policyName) => policyName switch
    {
        AuthRateLimitPolicyNames.RegisterDestinationDaily => RegisterDestinationDaily,
        AuthRateLimitPolicyNames.OtpSendDestinationBurst => OtpSendDestinationBurst,
        AuthRateLimitPolicyNames.OtpSendDestinationDaily => OtpSendDestinationDaily,
        AuthRateLimitPolicyNames.OtpVerifyChallenge => OtpVerifyChallenge,
        AuthRateLimitPolicyNames.RefreshSession => RefreshSession,
        AuthRateLimitPolicyNames.PasswordLoginAccount => PasswordLoginAccount,
        AuthRateLimitPolicyNames.PasswordLoginDevice => PasswordLoginDevice,
        _ => throw new ArgumentOutOfRangeException(nameof(policyName), policyName, "Unknown auth rate-limit policy.")
    };
}

public sealed class RateLimitRule
{
    public RateLimitRule() { }
    public RateLimitRule(int permitLimit, int windowSeconds)
    {
        PermitLimit = permitLimit;
        WindowSeconds = windowSeconds;
    }

    public int PermitLimit { get; set; }
    public int WindowSeconds { get; set; }
}
