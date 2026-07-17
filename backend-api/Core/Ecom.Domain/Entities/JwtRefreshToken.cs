namespace Ecom.Domain.Entities;

/// <summary>
/// Entity lưu trữ Refresh Token cho JWT authentication
/// Hỗ trợ token rotation và revocation tracking
/// </summary>
public class JwtRefreshToken : BaseEntity
{
    #region Token Info
    
    /// <summary>
    /// Refresh token value (unique)
    /// </summary>
    public string Token { get; set; } = string.Empty;
    
    /// <summary>
    /// Thời điểm token hết hạn
    /// </summary>
    public DateTime ExpiresAt { get; set; }
    
    /// <summary>
    /// Trạng thái token: Active, Revoked
    /// </summary>
    public JwtRefreshTokenStatusEnum Status { get; set; } = JwtRefreshTokenStatusEnum.Active;
    
    #endregion
    
    #region User Info
    
    /// <summary>
    /// User ID sở hữu token này
    /// </summary>
    public Guid UserId { get; set; }
    
    /// <summary>
    /// Navigation đến User
    /// </summary>
    public virtual User? User { get; set; }
    
    #endregion
    
    #region Creation Info
    
    /// <summary>
    /// IP address khi tạo token
    /// </summary>
    public string? CreatedByIp { get; set; }
    
    /// <summary>
    /// User Agent khi tạo token (browser/device info)
    /// </summary>
    public string? CreatedByUserAgent { get; set; }
    
    #endregion
    
    #region Revocation Info
    
    /// <summary>
    /// Thời điểm token bị revoke (null nếu chưa revoke)
    /// </summary>
    public DateTime? RevokedAt { get; set; }
    
    /// <summary>
    /// IP address khi revoke token
    /// </summary>
    public string? RevokedByIp { get; set; }
    
    /// <summary>
    /// Lý do revoke token
    /// VD: "Logout", "Token Rotation", "Security Breach", "Admin Revoked"
    /// </summary>
    public string? RevokedReason { get; set; }
    
    #endregion
    
    #region Token Rotation
    
    /// <summary>
    /// Token mới thay thế token này (khi sử dụng token rotation)
    /// Null nếu chưa được thay thế
    /// </summary>
    public string? ReplacedByToken { get; set; }
    
    #endregion
    
    #region Helper Properties
    
    /// <summary>
    /// Kiểm tra token đã hết hạn chưa
    /// </summary>
    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
    
    /// <summary>
    /// Kiểm tra token đã bị revoke chưa
    /// </summary>
    public bool IsRevoked => RevokedAt != null || Status == JwtRefreshTokenStatusEnum.Revoked;
    
    /// <summary>
    /// Kiểm tra token còn có thể sử dụng không
    /// </summary>
    public bool IsActive => !IsRevoked && !IsExpired && Status == JwtRefreshTokenStatusEnum.Active;
    
    #endregion
}


