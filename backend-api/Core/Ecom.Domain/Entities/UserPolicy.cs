namespace Ecom.Domain.Entities;

/// <summary>
/// Entity cho phép thêm/bớt Policy riêng cho từng User (ngoài Role mặc định)
/// Điều này cho phép tùy chỉnh quyền hạn của user mà không cần tạo role mới
/// </summary>
public class UserPolicy : BaseEntity
{
    /// <summary>
    /// User ID
    /// </summary>
    public Guid UserId { get; set; }
    
    /// <summary>
    /// Policy ID
    /// </summary>
    public Guid PolicyId { get; set; }
    
    /// <summary>
    /// True = Cấp thêm quyền (Grant) - User có policy này dù Role không có
    /// False = Thu hồi quyền (Revoke) - User không có policy này dù Role có
    /// </summary>
    public bool IsGranted { get; set; } = true;
    
    /// <summary>
    /// Lý do cấp/thu hồi quyền (optional, dùng để audit)
    /// </summary>
    public string? Reason { get; set; }
    
    /// <summary>
    /// Ngày hết hạn của việc cấp/thu hồi (null = vĩnh viễn)
    /// Hữu ích cho trường hợp cấp quyền tạm thời
    /// </summary>
    public DateTime? ExpiresAt { get; set; }
    
    #region Navigation Properties
    
    /// <summary>
    /// Navigation đến User
    /// </summary>
    public virtual User? User { get; set; }
    
    /// <summary>
    /// Navigation đến Policy
    /// </summary>
    public virtual Policy? Policy { get; set; }
    
    #endregion
}

