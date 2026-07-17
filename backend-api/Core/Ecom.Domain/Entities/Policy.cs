namespace Ecom.Domain.Entities;

/// <summary>
/// Entity đại diện cho quyền hạn (Policy) trong hệ thống
/// Policy định nghĩa một hành động cụ thể mà user có thể thực hiện
/// </summary>
public class Policy : BaseEntity
{
    /// <summary>
    /// Mã policy (unique, dùng trong code và JWT claims)
    /// VD: "users.read", "users.create", "orders.approve", "reports.export"
    /// </summary>
    public string Code { get; set; } = string.Empty;
    
    /// <summary>
    /// Tên hiển thị của policy
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Mô tả chi tiết về policy
    /// </summary>
    public string? Description { get; set; }
    
    /// <summary>
    /// Nhóm/Module mà policy thuộc về (dùng để phân loại)
    /// VD: "Users", "Orders", "Reports", "Settings"
    /// </summary>
    public string? Module { get; set; }
    
    /// <summary>
    /// Policy có đang hoạt động không
    /// </summary>
    public bool IsActive { get; set; } = true;
    
    /// <summary>
    /// Policy hệ thống (không được xóa/sửa)
    /// </summary>
    public bool IsSystemPolicy { get; set; } = false;
    
    #region Navigation Properties
    
    /// <summary>
    /// Danh sách Role có policy này (many-to-many qua RolePolicy)
    /// </summary>
    public virtual ICollection<RolePolicy> RolePolicies { get; set; } = new List<RolePolicy>();
    
    /// <summary>
    /// Danh sách User được gán/bỏ policy này riêng (qua UserPolicy)
    /// </summary>
    public virtual ICollection<UserPolicy> UserPolicies { get; set; } = new List<UserPolicy>();
    
    #endregion
}

