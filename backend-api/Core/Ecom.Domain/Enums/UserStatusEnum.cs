namespace Ecom.Domain.Enums;

/// <summary>
/// Trạng thái tài khoản người dùng
/// </summary>
public enum UserStatusEnum
{
    /// <summary>
    /// Chưa xác định
    /// </summary>
    Unspecified = 0,
    
    /// <summary>
    /// Mới tạo tài khoản, chưa kích hoạt (cần xác thực OTP/Email)
    /// </summary>
    Pending = 1,
    
    /// <summary>
    /// Đã kích hoạt và đang hoạt động
    /// </summary>
    Active = 2,
    
    /// <summary>
    /// Đã bị vô hiệu hóa
    /// </summary>
    Deactivated = 3
}
