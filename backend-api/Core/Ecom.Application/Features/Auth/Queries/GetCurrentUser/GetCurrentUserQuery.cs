namespace Ecom.Application.Features.Auth.Queries.GetCurrentUser;

/// <summary>
/// Query lấy thông tin user đang đăng nhập
/// </summary>
public record GetCurrentUserQuery : IRequest<TResult<CurrentUserResult>>;

/// <summary>
/// Kết quả thông tin user hiện tại
/// </summary>
public class CurrentUserResult
{
    public Guid UserId { get; set; }
    public string PhoneNumber { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string? Email { get; set; }
    public string? FullName { get; set; }
    public Guid? AvatarId { get; set; }
    public string Status { get; set; } = string.Empty;
    public Guid? RoleId { get; set; }
    public string? RoleCode { get; set; }
    public string? RoleName { get; set; }
    public List<string> Policies { get; set; } = new();
    public DateTime? LastLoginAt { get; set; }
    public bool PhoneNumberConfirmed { get; set; }
    public bool EmailConfirmed { get; set; }
}
