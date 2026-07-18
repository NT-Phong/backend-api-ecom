namespace Ecom.Application.Features.Auth.Commands.Logout;
/// <summary>
/// Logout - revoke refresh token
/// </summary>
[EnableUnitOfWork]
public record LogoutCommand : IRequest<TResult>
{
    public string RefreshToken { get; set; } = string.Empty;
    public bool LogoutAllDevices { get; set; } = false;
    public string? FcmToken { get; set; }
    public Guid? SessionId { get; set; }
}
