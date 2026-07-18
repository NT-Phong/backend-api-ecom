namespace Ecom.Application.Common.Interfaces;

public interface ICurrentUser
{
    /// <summary>
    /// User ID (Guid) từ JWT claims
    /// </summary>
    Guid UserId { get; }
    
    /// <summary>
    /// User ID as string (for legacy support)
    /// </summary>
    string? UserIdString { get; }
    
    /// <summary>
    /// Số điện thoại từ JWT claims
    /// </summary>
    string? PhoneNumber { get; }
    
    /// <summary>
    /// Email từ JWT claims
    /// </summary>
    string? Email { get; }
    
    /// <summary>
    /// Đã xác thực hay chưa
    /// </summary>
    bool IsAuthenticated { get; }
    
    /// <summary>
    /// Role code của user
    /// </summary>
    string? Role { get; }
    
    /// <summary>
    /// Danh sách roles
    /// </summary>
    IEnumerable<string> Roles { get; }
    
    /// <summary>
    /// Danh sách policies/permissions
    /// </summary>
    IEnumerable<string> Policies { get; }
    Guid SessionId { get; }
    string? SecurityStamp { get; }
    
    /// <summary>
    /// Kiểm tra user có role hay không
    /// </summary>
    bool HasRole(string role);
    
    /// <summary>
    /// Kiểm tra user có policy/permission hay không
    /// </summary>
    bool HasPolicy(string policy);
}

