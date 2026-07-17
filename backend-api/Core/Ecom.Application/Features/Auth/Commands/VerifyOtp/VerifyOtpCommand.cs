namespace Ecom.Application.Features.Auth.Commands.VerifyOtp;

/// <summary>
/// Command xác thực OTP và đăng nhập
/// </summary>
public record VerifyOtpCommand : IRequest<TResult<VerifyOtpResult>>
{
    /// <summary>
    /// Số điện thoại
    /// </summary>
    public string PhoneNumber { get; set; } = string.Empty;
    
    /// <summary>
    /// Mã OTP
    /// </summary>
    public string OtpCode { get; set; } = string.Empty;
}

/// <summary>
/// Kết quả xác thực OTP
/// </summary>
public class VerifyOtpResult
{
    /// <summary>
    /// User ID
    /// </summary>
    public Guid UserId { get; set; }
    
    /// <summary>
    /// Số điện thoại
    /// </summary>
    public string PhoneNumber { get; set; } = string.Empty;
    /// <summary>
    /// Cờ hoàn thiện hồ sơ người dùng
    /// </summary>
    public bool IsProfileCompleted { get; set; }
    /// <summary>
    /// Trạng thái đăng nhập
    /// </summary>
    public string LoginStatus { get; set; } = string.Empty;

    /// <summary>
    /// Access Token (JWT)
    /// </summary>
    public string AccessToken { get; set; } = string.Empty;
    
    /// <summary>
    /// Refresh Token
    /// </summary>
    public string RefreshToken { get; set; } = string.Empty;

    /// <summary>
    /// Thời điểm Access Token hết hạn
    /// </summary>
    public DateTime AccessTokenExpiresAt { get; set; }
    
    /// <summary>
    /// Thời điểm Refresh Token hết hạn
    /// </summary>
    public DateTime RefreshTokenExpiresAt { get; set; }
    
    /// <summary>
    /// Role của user
    /// </summary>
    public string? RoleCode { get; set; }
    public Guid? RoleId { get; set; }
    public string? RoleName { get; set; }

    /// <summary>
    /// Danh sách policies
    /// </summary>
    public List<string> Policies { get; set; } = new();
}

