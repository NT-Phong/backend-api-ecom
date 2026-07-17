namespace Ecom.Domain.Entities;

/// <summary>
/// Entity lưu trữ OTP (One-Time Password) cho xác thực
/// Hỗ trợ các loại: Login, VerifyEmail, ResetPassword, VerifyPhone
/// </summary>
public class OtpToken : BaseEntity
{
    #region User Info
    
    /// <summary>
    /// User ID sở hữu OTP này
    /// </summary>
    public Guid UserId { get; set; }
    
    /// <summary>
    /// Navigation đến User
    /// </summary>
    public virtual User? User { get; set; }
    
    #endregion
    
    #region OTP Info
    
    /// <summary>
    /// Mã OTP (4 số)
    /// </summary>
    public string Code { get; set; } = string.Empty;
    
    /// <summary>
    /// Loại OTP: Login, VerifyEmail, ResetPassword, VerifyPhone
    /// </summary>
    public OtpTokenTypeEnum OtpTokenType { get; set; } = OtpTokenTypeEnum.VerifyEmail;
    
    /// <summary>
    /// Thời điểm OTP hết hạn
    /// </summary>
    public DateTime ExpiredAt { get; set; }
    
    /// <summary>
    /// OTP đã được sử dụng chưa
    /// </summary>
    public bool IsUsed { get; set; } = false;
    
    /// <summary>
    /// Thời điểm OTP được sử dụng
    /// </summary>
    public DateTime? UsedAt { get; set; }
    
    #endregion
    
    #region Verification Target
    
    /// <summary>
    /// Số điện thoại cần xác thực (nếu OTP gửi qua SMS)
    /// </summary>
    public string? PhoneNumber { get; set; }
    
    /// <summary>
    /// Email cần xác thực (nếu OTP gửi qua Email)
    /// </summary>
    public string? Email { get; set; }
    
    #endregion
    
    #region Security & Tracking
    
    /// <summary>
    /// Số lần nhập sai OTP
    /// </summary>
    public int FailedAttempts { get; set; } = 0;
    
    /// <summary>
    /// Số lần tối đa được phép nhập sai (default: 5)
    /// </summary>
    public int MaxAttempts { get; set; } = 5;
    
    /// <summary>
    /// IP address khi tạo OTP
    /// </summary>
    public string? CreatedByIp { get; set; }
    
    /// <summary>
    /// IP address khi verify OTP
    /// </summary>
    public string? VerifiedByIp { get; set; }
    
    #endregion
    
    #region Helper Properties
    
    /// <summary>
    /// Kiểm tra OTP đã hết hạn chưa
    /// </summary>
    public bool IsExpired => DateTime.UtcNow >= ExpiredAt;
    
    /// <summary>
    /// Kiểm tra OTP còn có thể sử dụng không
    /// </summary>
    public bool IsValid => !IsUsed && !IsExpired && FailedAttempts < MaxAttempts;
    
    /// <summary>
    /// Kiểm tra đã vượt quá số lần nhập sai
    /// </summary>
    public bool IsLocked => FailedAttempts >= MaxAttempts;

    #endregion

    // Hàm nghiệp vụ để dùng trong VerifyHandler
    public void MarkAsUsed()
    {
        IsUsed = true;
        UsedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }
    public void UpdateNewCode(string code, int expSeconds, int maxAttempts)
    {
        Code = code;
        ExpiredAt = DateTime.UtcNow.AddSeconds(expSeconds);
        IsUsed = false;
        UsedAt = null;
        FailedAttempts = 0;
        MaxAttempts = maxAttempts;
        UpdatedAt = DateTime.UtcNow;
    }
}


