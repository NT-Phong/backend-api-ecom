namespace Ecom.Application.Features.Auth.Commands.CreateAccount;

/// <summary>
/// Command để đăng ký tài khoản mới bằng số điện thoại
/// Sau đăng ký, user cần xác thực OTP để kích hoạt tài khoản
/// </summary>
public record CreateAccountCommand : IRequest<TResult<CreateAccountResult>>
{
    /// <summary>
    /// Số điện thoại (required - dùng để đăng ký và nhận OTP)
    /// </summary>
    public string PhoneNumber { get; set; } = string.Empty;
}

/// <summary>
/// Kết quả đăng ký tài khoản
/// </summary>
public class CreateAccountResult
{
    public Guid UserId { get; set; }
    public string PhoneNumber { get; set; } = string.Empty;
    public bool IsProfileCompleted { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? TestOtp { get; set; }
    public int ExpiresIn { get; set; }
}

