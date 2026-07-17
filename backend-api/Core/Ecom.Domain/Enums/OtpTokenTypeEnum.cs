namespace Ecom.Domain.Enums;

/// <summary>
/// Loại OTP token
/// </summary>
public enum OtpTokenTypeEnum
{
    /// <summary>
    /// Chưa xác định
    /// </summary>
    Unspecified = 0,
    
    /// <summary>
    /// OTP đăng nhập (2FA)
    /// </summary>
    Login = 1,
    
    /// <summary>
    /// OTP xác thực email
    /// </summary>
    VerifyEmail = 2,
    
    /// <summary>
    /// OTP đặt lại mật khẩu
    /// </summary>
    ResetPassword = 3,
    
    /// <summary>
    /// OTP xác thực số điện thoại
    /// </summary>
    VerifyPhone = 4,
    
    /// <summary>
    /// OTP kích hoạt tài khoản
    /// </summary>
    ActivateAccount = 5
}
