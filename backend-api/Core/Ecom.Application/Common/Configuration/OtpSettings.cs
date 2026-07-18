namespace Ecom.Application.Common.Configuration;

/// <summary>
/// Cấu hình OTP settings từ appsettings.json
/// </summary>
public class OtpSettings
{
    public const string SectionName = "Otp";
    
    /// <summary>
    /// Độ dài mã OTP (mặc định 4 số)
    /// </summary>
    public int OtpLength { get; set; } = 4;
    
    /// <summary>
    /// Thời gian hết hạn OTP (giây) - mặc định 60 giây
    /// </summary>
    public int ExpirationSeconds { get; set; } = 60;
    
    /// <summary>
    /// Thời gian chờ trước khi gửi lại OTP (giây) - mặc định 60 giây
    /// </summary>
    public int ResendCooldownSeconds { get; set; } = 60;
    
    /// <summary>
    /// Số lần nhập sai tối đa - mặc định 5 lần
    /// </summary>
    public int MaxAttempts { get; set; } = 5;
    public string? TemplateId { get; set; }
    public string? TemplateIdVina { get; set; }
    public string TestPhoneNumber { get; set; } = string.Empty;
    public string DefaultOtp { get; set; } = "0000";
    /// <summary>
    /// Keyed HMAC secret used to protect OTP values at rest. Supply from a secret store outside Development.
    /// </summary>
    public string HashKey { get; set; } = string.Empty;
    /// <summary>
    /// Enables the fixed development OTP for the configured legacy test accounts.
    /// This option is rejected outside the Development environment.
    /// </summary>
    public bool EnableDevelopmentTestAccounts { get; set; }
    /// <summary>
    /// Allows returning a development OTP in the existing V1 DTO. Rejected outside Development.
    /// </summary>
    public bool ExposeDevelopmentOtp { get; set; }
}

