namespace Ecom.Application.Features.Auth.Commands.SendOtp;

/// <summary>
/// Command gửi OTP đến số điện thoại
/// Dùng cho đăng nhập, kích hoạt tài khoản, xác thực số điện thoại
/// </summary>
[EnableUnitOfWork]
public record SendOtpCommand : IRequest<TResult<SendOtpResult>>
{
    /// <summary>
    /// Số điện thoại nhận OTP
    /// </summary>
    public string PhoneNumber { get; set; } = string.Empty;

    /// <summary>
    /// Nguồn đăng nhập (web hoặc mobile)
    /// </summary>
    public string? LoginSource { get; set; }
}

/// <summary>
/// Kết quả gửi OTP
/// </summary>
public class SendOtpResult
{
    /// <summary>
    /// Thời gian OTP có hiệu lực (giây)
    /// </summary>
    public int ExpiresInSeconds { get; set; }
    
    /// <summary>
    /// Thời điểm có thể gửi lại OTP
    /// </summary>
    public DateTime? CanResendAt { get; set; }
    
    /// <summary>
    /// Message thông báo
    /// </summary>
    public string Message { get; set; } = string.Empty;
    
    /// <summary>
    /// OTP code (chỉ trả về trong môi trường Development)
    /// </summary>
    public string? OtpCode { get; set; }
    public bool IsPending { get; set; }
    public string? Status { get; set; }
}

