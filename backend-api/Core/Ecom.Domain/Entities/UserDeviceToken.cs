namespace Ecom.Domain.Entities;

/// <summary>
/// Entity lưu trữ FCM Device Token của người dùng để hỗ trợ Push Notification
/// </summary>
public class UserDeviceToken : BaseEntity
{
    /// <summary>
    /// ID của User sở hữu thiết bị này
    /// </summary>
    public Guid UserId { get; private set; }
    
    public virtual User? User { get; private set; }
    
    /// <summary>
    /// Token FCM do Google Firebase cấp
    /// </summary>
    public string FcmToken { get; private set; }
    
    /// <summary>
    /// Nền tảng thiết bị (Web, Android, iOS)
    /// </summary>
    public string Platform { get; private set; }
    
    /// <summary>
    /// Trạng thái hoạt động của token
    /// </summary>
    public bool IsActive { get; private set; } = true;
    
    /// <summary>
    /// Lần cuối token này được sử dụng 
    /// </summary>
    public DateTime? LastUsedAt { get; private set; }

    // Dùng cho EF Core
    private UserDeviceToken() 
    { 
        FcmToken = string.Empty;
        Platform = string.Empty;
    }

    public UserDeviceToken(Guid userId, string fcmToken, string platform)
    {
        UserId = userId;
        FcmToken = fcmToken;
        Platform = platform;
        IsActive = true;
        LastUsedAt = DateTime.UtcNow;
    }

    public void Deactivate() => IsActive = false;

    public void Register(Guid userId, string platform)
    {
        UserId = userId;
        Platform = platform;
        IsActive = true;
        IsDeleted = false;
        DeletedAt = null;
        DeletedBy = null;
        LastUsedAt = DateTime.UtcNow;
    }
    
    public void UpdateLastUsed() => LastUsedAt = DateTime.UtcNow;
}

