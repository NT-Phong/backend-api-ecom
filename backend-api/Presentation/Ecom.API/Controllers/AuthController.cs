using Ecom.Application.Features.Auth.Commands.CompleteProfile;
using Ecom.Application.Features.Auth.Commands.CreateAccount;
using Ecom.Application.Features.Auth.Commands.DeleteAccount;
using Ecom.Application.Features.Auth.Commands.Logout;
using Ecom.Application.Features.Auth.Commands.RefreshToken;
using Ecom.Application.Features.Auth.Commands.RoleManagement.CreateRole;
using Ecom.Application.Features.Auth.Commands.RoleManagement.DeleteRole;
using Ecom.Application.Features.Auth.Commands.RoleManagement.UpdateRole;
using Ecom.Application.Features.Auth.Commands.SendOtp;
using Ecom.Application.Features.Auth.Commands.UpdateProfile;
using Ecom.Application.Features.Auth.Commands.UpdateBasicProfile;
using Ecom.Application.Features.Auth.Commands.UpdateUserRole;
using Ecom.Application.Features.Auth.Commands.UserManagement.Commands.CreateUser;
using Ecom.Application.Features.Auth.Commands.UserManagement.Commands.DeleteUser;
using Ecom.Application.Features.Auth.Commands.UserManagement.Commands.UpdateUser;
using Ecom.Application.Features.Auth.Commands.VerifyOtp;
using Ecom.Application.Features.Auth.Queries.GetAllUsers;
using Ecom.Application.Features.Auth.Queries.GetCurrentUser;
using Ecom.Application.Features.Auth.Queries.GetRoles;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;
using Ecom.Application.Common.Configuration;

namespace Ecom.API.Controllers;

/// <summary>
/// Controller xử lý các chức năng xác thực
/// Đăng ký và đăng nhập bằng số điện thoại + OTP
/// </summary>
[ApiVersion("1.0")]
[ApiExplorerSettings(GroupName = "v1")]
[Route("api/v{version:apiVersion}/auth")]
public class AuthController : BaseController
{
    private readonly ILogger<AuthController> _logger;

    public AuthController(ILogger<AuthController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Compatibility route. New clients should use send-otp as the single phone-first entry point.
    /// </summary>
    /// <param name="command">Thông tin đăng ký</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Thông tin tài khoản đã tạo</returns>
    [HttpPost("register")]
    [AllowAnonymous]
    [EnableRateLimiting(AuthRateLimitPolicyNames.RegisterIp)]
    [ProducesResponseType(typeof(ApiResponse<CreateAccountResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register(
        [FromBody] CreateAccountCommand command,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Register request received");

        var result = await Mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Gửi OTP đến số điện thoại
    /// Dùng cho đăng nhập hoặc kích hoạt tài khoản
    /// </summary>
    /// <param name="command">Thông tin yêu cầu OTP</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Thông tin OTP đã gửi</returns>
    [HttpPost("send-otp")]
    [AllowAnonymous]
    [EnableRateLimiting(AuthRateLimitPolicyNames.OtpSendIp)]
    [ProducesResponseType(typeof(ApiResponse<SendOtpResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SendOtp(
        [FromBody] SendOtpCommand command,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("SendOtp request received");

        var result = await Mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Xác thực OTP và đăng nhập
    /// Trả về JWT tokens nếu OTP đúng
    /// </summary>
    /// <param name="command">Thông tin xác thực OTP</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Access Token và Refresh Token</returns>
    [HttpPost("verify-otp")]
    [AllowAnonymous]
    [EnableRateLimiting(AuthRateLimitPolicyNames.OtpVerifyIp)]
    [ProducesResponseType(typeof(ApiResponse<VerifyOtpResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> VerifyOtp(
        [FromBody] VerifyOtpCommand command,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("VerifyOtp request received");

        var result = await Mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }
    /// <summary>
    /// Update thông tin hoàn thiện hồ sơ người dùng khi đăng nhập lần đầu
    /// </summary>
    /// <param name="command"></param>
    /// <returns></returns>
    [HttpPost("complete-profile")]
    [Authorize]
    public async Task<IActionResult> CompleteProfile([FromBody] CompleteProfileCommand command)
    {
        var result = await Mediator.Send(command);
        return HandleResult(result);
    }
    /// <summary>
    /// Lưu tên hiển thị sau OTP. Đây là bước tùy chọn và không hoàn thiện full profile.
    /// </summary>
    [HttpPatch("profile/basic")]
    [Authorize(Policy = Permissions.User.Update)]
    public async Task<IActionResult> UpdateBasicProfile([FromBody] UpdateBasicProfileCommand command, CancellationToken cancellationToken)
    {
        return HandleResult(await Mediator.Send(command, cancellationToken));
    }
    /// <summary>
    /// Update thông tin cá nhân người dùng
    /// </summary>
    /// <param name="command"></param>
    /// <returns></returns>
    [HttpPatch("profile")]
    [Authorize(Policy = Permissions.User.Update)]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileCommand command)
    {
        var result = await Mediator.Send(command);
        return HandleResult(result);
    }
    /// <summary>
    /// Làm mới Access Token bằng Refresh Token
    /// </summary>
    /// <param name="command">Access token và Refresh token</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Access Token mới và Refresh Token mới (nếu enable rotation)</returns>
    [HttpPost("refresh-token")]
    [AllowAnonymous]
    [EnableRateLimiting(AuthRateLimitPolicyNames.RefreshIp)]
    [ProducesResponseType(typeof(ApiResponse<RefreshTokenResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RefreshToken(
        [FromBody] RefreshTokenCommand command,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("RefreshToken request");

        var result = await Mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Đăng xuất - thu hồi Refresh Token
    /// </summary>
    /// <param name="command">Refresh token cần thu hồi</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Kết quả đăng xuất</returns>
    [HttpPost("logout")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Logout(
        [FromBody] LogoutCommand command,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Logout request. LogoutAllDevices: {LogoutAll}", command.LogoutAllDevices);

        var result = await Mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Lấy thông tin user đang đăng nhập
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Thông tin user hiện tại</returns>
    [HttpGet("me")]
    [Authorize(Policy = Permissions.User.Read)]
    [ProducesResponseType(typeof(ApiResponse<CurrentUserResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetCurrentUser(CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new GetCurrentUserQuery(), cancellationToken);
        return HandleResult(result);
    }
    /// <summary>
    /// Delete account hiện tại
    /// </summary>
    /// <param name="command"></param>
    /// <returns></returns>
    [HttpDelete("delete-account")]
    [Authorize(Policy = Permissions.User.Delete)]
    public async Task<IActionResult> DeleteAccount([FromBody] DeleteAccountCommand command)
    {
        var result = await Mediator.Send(command);
        return HandleResult(result);
    }

    /// <summary>
    /// Lấy danh sách tất cả người dùng (Phân trang)
    /// </summary>
    [HttpGet("users")]
    [Authorize(Policy = Permissions.UsersManage.Read)]
    [ProducesResponseType(typeof(ApiResponse<PaginatedList<CurrentUserResult>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAllUsers([FromQuery] GetAllUsersQuery query, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(query, cancellationToken);
        return HandleResult(result);
    }
    
    /// <summary>
    /// API Admin/Quản lý thêm mới thành viên (Portal + Mobile)
    /// </summary>
    [HttpPost("admin/create-user")]
    [Authorize(Policy = Permissions.UsersManage.Create)]
    public async Task<IActionResult> AdminCreateUser([FromBody] CreateUserCommand command)
    {
        var result = await Mediator.Send(command);
        return HandleResult(result);
    }
    /// <summary>
    /// Chỉnh sửa thông tin thành viên
    /// </summary>
    [HttpPut("admin/users/{id}")]
    [Authorize(Policy = Permissions.UsersManage.Update)]
    public async Task<IActionResult> UpdateUser(Guid id, [FromBody] UpdateUserCommand command)
    {
        // Gán ID từ URL vào Command
        var result = await Mediator.Send(command with { Id = id });
        return HandleResult(result);
    }
    /// <summary>
    /// Xóa thành viên
    /// </summary>
    [HttpDelete("admin/users/{id}")]
    [Authorize(Policy = Permissions.UsersManage.Delete)]
    public async Task<IActionResult> DeleteUser(Guid id)
    {
        var result = await Mediator.Send(new DeleteUserCommand(id));
        return HandleResult(result);
    }
    [HttpGet("admin/roles")]
    [Authorize(Policy = Permissions.Roles.Read)]
    public async Task<IActionResult> GetRoles()
    {
        var result = await Mediator.Send(new GetRolesQuery());
        return HandleResult(result);
    }

    [HttpPost("admin/roles")]
    [Authorize(Policy = Permissions.Roles.Create)]
    public async Task<IActionResult> CreateRole([FromBody] CreateRoleCommand command)
    {
        var result = await Mediator.Send(command);
        return HandleResult(result);
    }

    [HttpPut("admin/roles/{id}")]
    [Authorize(Policy = Permissions.Roles.Update)]
    public async Task<IActionResult> UpdateRole(Guid id, [FromBody] UpdateRoleCommand command)
    {
        var result = await Mediator.Send(command with { Id = id });
        return HandleResult(result);
    }

    [HttpDelete("admin/roles/{id}")]
    [Authorize(Policy = Permissions.Roles.Delete)]
    public async Task<IActionResult> DeleteRole(Guid id)
    {
        var result = await Mediator.Send(new DeleteRoleCommand(id));
        return HandleResult(result);
    }

    /// <summary>
    /// Cập nhật quyền hạn cho người dùng (Chỉ dành cho SystemAdmin)
    /// </summary>
    [HttpPut("user-role/{id}")]
    [Authorize(Policy = Permissions.Roles.AssignRole)]
    [ProducesResponseType(typeof(ApiResponse<UpdateUserRoleResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateUserRole(
        [FromRoute] Guid id,
        [FromBody] UpdateUserRoleCommand command,
        CancellationToken cancellationToken)
    {
        // Gán ID từ URL vào Command trước khi gửi qua Mediator
        var result = await Mediator.Send(command with { TargetUserId = id }, cancellationToken);
        return HandleResult(result);
    }
}

