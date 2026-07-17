namespace Ecom.Domain.Entities;

/// <summary>
/// Entity đại diện cho vai trò trong hệ thống RBAC
/// 1 Role có thể có nhiều Policy
/// 1 User có 1 Role
/// </summary>
public class Role : BaseEntity
{
    /// <summary>
    /// Mã vai trò (unique, dùng trong code)
    /// VD: "SUPER_ADMIN", "ADMIN", "MANAGER", "USER"
    /// </summary>
    public string Code { get; set; } = string.Empty;
    
    /// <summary>
    /// Tên hiển thị của vai trò
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Mô tả chi tiết về vai trò
    /// </summary>
    public string? Description { get; set; }
    
    /// <summary>
    /// Vai trò có đang hoạt động không
    /// </summary>
    public bool IsActive { get; set; } = true;
    
    /// <summary>
    /// Thứ tự ưu tiên (số nhỏ = ưu tiên cao)
    /// Dùng để xác định quyền cao nhất khi cần
    /// </summary>
    public int Priority { get; set; } = 0;
    
    /// <summary>
    /// Vai trò hệ thống (không được xóa/sửa)
    /// </summary>
    public bool IsSystemRole { get; set; } = false;
    
    #region Navigation Properties
    
    /// <summary>
    /// Danh sách Policy của Role (many-to-many qua RolePolicy)
    /// </summary>
    public virtual ICollection<RolePolicy> RolePolicies { get; set; } = new List<RolePolicy>();
    
    /// <summary>
    /// Danh sách User thuộc Role này
    /// </summary>
    public virtual ICollection<User> Users { get; set; } = new List<User>();
    
    #endregion
}

