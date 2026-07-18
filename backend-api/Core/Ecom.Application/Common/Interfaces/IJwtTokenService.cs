using Ecom.Application.Features.Auth.Commands.RefreshToken;
using Ecom.Domain.Entities;

namespace Ecom.Application.Common.Interfaces;

/// <summary>
/// Interface cho JWT Token Service
/// </summary>
public interface IJwtTokenService
{
    /// <summary>
    /// Tạo Access Token cho user
    /// </summary>
    /// <param name="user">User entity</param>
    /// <param name="policies">Danh sách policies của user (đã tính toán từ Role + UserPolicy)</param>
    /// <returns>JWT Access Token string</returns>
    string GenerateAccessToken(User user, IEnumerable<string> policies);
    string GenerateAccessToken(User user, IEnumerable<string> policies, Guid sessionId, string securityStamp);
    
    /// <summary>
    /// Tạo Refresh Token
    /// </summary>
    /// <returns>Refresh token string (random secure string)</returns>
    string GenerateRefreshToken();
    
    /// <summary>
    /// Validate Access Token và lấy thông tin claims
    /// </summary>
    /// <param name="token">JWT token string</param>
    /// <returns>JwtValidationResult chứa claims nếu valid</returns>
    JwtValidationResult ValidateAccessToken(string token);
    
    /// <summary>
    /// Lấy User ID từ token (kể cả token đã hết hạn)
    /// Dùng cho refresh token flow
    /// </summary>
    /// <param name="token">JWT token string</param>
    /// <returns>User ID hoặc null nếu token không hợp lệ</returns>
    Guid? GetUserIdFromExpiredToken(string token);
    
    /// <summary>
    /// Lấy thời gian hết hạn của Access Token
    /// </summary>
    /// <returns>DateTime UTC</returns>
    DateTime GetAccessTokenExpiration();
    
    /// <summary>
    /// Lấy thời gian hết hạn của Refresh Token
    /// </summary>
    /// <returns>DateTime UTC</returns>
    DateTime GetRefreshTokenExpiration();
    /// <summary>
    /// Xử lý logic làm mới Token (Kiểm tra DB, User, Rotate token...)
    /// </summary>
    /// <param name="refreshToken">ID của refresh token</param>
    /// <param name="cancellationToken"></param>
    /// <returns>Kết quả chứa AccessToken và RefreshToken mới</returns>
    Task<TResult<RefreshTokenResult>> RefreshJwtToken(string refreshToken, CancellationToken cancellationToken);

    /// <summary>
    /// Thu hồi Refresh Token (Đăng xuất)
    /// </summary>
    /// <param name="refreshToken">ID của refresh token</param>
    /// <param name="cancellationToken"></param>
    /// <returns>Kết quả thành công hoặc thất bại</returns>
    Task<TResult> RevokeRefreshToken(string refreshToken, CancellationToken cancellationToken);
}

/// <summary>
/// Kết quả validate JWT token
/// </summary>
public class JwtValidationResult
{
    public bool IsValid { get; set; }
    public Guid? UserId { get; set; }
    public string? Username { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Role { get; set; }
    public IEnumerable<string> Policies { get; set; } = Enumerable.Empty<string>();
    public string? ErrorMessage { get; set; }
    public DateTime? ExpiresAt { get; set; }
}

