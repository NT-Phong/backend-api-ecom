namespace Ecom.Application.Common.Configuration;

/// <summary>
/// Cấu hình JWT Token
/// </summary>
public class JwtSettings
{
    public const string SectionName = "Jwt";
    
    /// <summary>
    /// Secret key để ký JWT (tối thiểu 32 ký tự cho HS256)
    /// </summary>
    public string SecretKey { get; set; } = string.Empty;
    
    /// <summary>
    /// Issuer của token (thường là tên ứng dụng hoặc domain)
    /// </summary>
    public string Issuer { get; set; } = string.Empty;
    
    /// <summary>
    /// Audience của token (client apps được phép sử dụng)
    /// </summary>
    public string Audience { get; set; } = string.Empty;
    
    /// <summary>
    /// Thời gian sống của Access Token (phút)
    /// Default: 15 phút
    /// </summary>
    public int AccessTokenExpirationMinutes { get; set; } = 10;
    
    /// <summary>
    /// Thời gian sống của Refresh Token (ngày)
    /// Default: 7 ngày
    /// </summary>
    public int RefreshTokenExpirationDays { get; set; } = 7;
    
    /// <summary>
    /// Cho phép refresh token rotation (tạo token mới khi refresh)
    /// </summary>
    public bool EnableTokenRotation { get; set; } = true;
    
    /// <summary>
    /// Số ngày token cũ vẫn còn hợp lệ sau khi rotation (grace period)
    /// Hữu ích cho trường hợp request bị retry
    /// </summary>
    public int TokenRotationGracePeriodMinutes { get; set; } = 2;
    
    /// <summary>
    /// Validate Issuer khi verify token
    /// </summary>
    public bool ValidateIssuer { get; set; } = true;
    
    /// <summary>
    /// Validate Audience khi verify token
    /// </summary>
    public bool ValidateAudience { get; set; } = true;
    
    /// <summary>
    /// Validate Lifetime khi verify token
    /// </summary>
    public bool ValidateLifetime { get; set; } = true;
    
    /// <summary>
    /// Clock skew tolerance (giây) - để xử lý sai lệch đồng hồ giữa các server
    /// Default: 30 giây
    /// </summary>
    public int ClockSkewSeconds { get; set; } = 30;
}

