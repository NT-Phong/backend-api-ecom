using Ecom.Application.Common.Configuration;
using Ecom.Application.Common.Interfaces;
using Ecom.Application.Common.Models;
using Ecom.Application.Features.Auth.Commands.RefreshToken;
using Ecom.Domain.Entities;
using Ecom.Domain.Enums;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Ecom.Infrastructure.Security;

/// <summary>
/// JWT Token Service implementation
/// Hỗ trợ tạo Access Token, Refresh Token và validate token
/// </summary>
public class JwtTokenService : IJwtTokenService
{
    private readonly JwtSettings _settings;
    private readonly SymmetricSecurityKey _signingKey;
    private readonly TokenValidationParameters _validationParameters;
    private readonly IUnitOfWork _unitOfWork;

    public JwtTokenService(IOptions<JwtSettings> settings, IUnitOfWork unitOfWork)
    {
        _settings = settings.Value;
        _unitOfWork = unitOfWork;

        // Validate secret key length (minimum 32 bytes for HS256)
        if (string.IsNullOrEmpty(_settings.SecretKey) || _settings.SecretKey.Length < 32)
        {
            throw new InvalidOperationException(
                "JWT SecretKey must be at least 32 characters for HS256 algorithm. " +
                "Please configure 'Jwt:SecretKey' in appsettings.json");
        }

        _signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.SecretKey));

        _validationParameters = new TokenValidationParameters
        {
            ValidateIssuer = _settings.ValidateIssuer,
            ValidateAudience = _settings.ValidateAudience,
            ValidateLifetime = _settings.ValidateLifetime,
            ValidateIssuerSigningKey = true,
            ValidIssuer = _settings.Issuer,
            ValidAudience = _settings.Audience,
            IssuerSigningKey = _signingKey,
            ClockSkew = TimeSpan.FromSeconds(_settings.ClockSkewSeconds)
        };
    }

    /// <inheritdoc />
    public string GenerateAccessToken(User user, IEnumerable<string> policies)
    {
        var claims = new List<Claim>
        {
            // Standard claims
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(),
                ClaimValueTypes.Integer64),

            // Custom claims
            new("userId", user.Id.ToString()),
        };

        // Add phone number if available
        if (!string.IsNullOrEmpty(user.PhoneNumber))
        {
            claims.Add(new Claim("phone", user.PhoneNumber));
            claims.Add(new Claim(ClaimTypes.MobilePhone, user.PhoneNumber));
        }

        // Add role if available
        if (user.Role != null)
        {
            claims.Add(new Claim("role", user.Role.Code));
            claims.Add(new Claim(ClaimTypes.Role, user.Role.Code));
        }

        // Add policies as claims
        foreach (var policy in policies)
        {
            claims.Add(new Claim("policy", policy));
        }

        var credentials = new SigningCredentials(_signingKey, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _settings.Issuer,
            audience: _settings.Audience,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: GetAccessTokenExpiration(),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    /// <inheritdoc />
    public string GenerateRefreshToken()
    {
        // Generate cryptographically secure random bytes
        var randomBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);

        // Convert to URL-safe base64 string
        return Convert.ToBase64String(randomBytes)
            .Replace("+", "-")
            .Replace("/", "_")
            .Replace("=", "");
    }

    public async Task<TResult<RefreshTokenResult>> RefreshJwtToken(string refreshTokenId,
        CancellationToken cancellationToken)
    {
        // 1. Tìm Refresh Token trong Database
        var refreshToken = await _unitOfWork.Repository<JwtRefreshToken>()
            .FindOneAsync(filters: [t => t.Token == refreshTokenId]);

        if (refreshToken == null)
            return TResult<RefreshTokenResult>.Failure("Refresh token không tồn tại", ErrorCodes.UNAUTHORIZED);

        // 2. Kiểm tra Token còn active không
        if (!refreshToken.IsActive)
            return TResult<RefreshTokenResult>.Failure("Refresh token đã hết hạn hoặc bị thu hồi",
                ErrorCodes.UNAUTHORIZED);

        // 3. Lấy thông tin User
        var user = await _unitOfWork.Repository<User>().FindByIdAsync(refreshToken.UserId, u => u.Role!);
        if (user == null || user.Status != UserStatusEnum.Active)
            return TResult<RefreshTokenResult>.Failure("Người dùng không tồn tại hoặc bị khóa", ErrorCodes.FORBIDDEN);

        // 4. Tạo Access Token mới
        // Bạn cần logic lấy Policies ở đây (giống như trong file cũ bạn gửi)
        var policies = await GetUserPoliciesAsync(user);
        var newAccessToken = GenerateAccessToken(user, policies);

        var newRefreshTokenStr = refreshToken.Token;
        var expiresAt = refreshToken.ExpiresAt;

        // 5. Token Rotation (Nếu bật)
        if (_settings.EnableTokenRotation)
        {
            newRefreshTokenStr = GenerateRefreshToken();
            expiresAt = GetRefreshTokenExpiration();

            // Vô hiệu hóa token cũ
            refreshToken.RevokedAt = DateTime.UtcNow;
            refreshToken.Status = JwtRefreshTokenStatusEnum.Revoked;
            refreshToken.ReplacedByToken = newRefreshTokenStr;

            // Tạo token mới lưu vào DB
            var newRefreshTokenEntity = new JwtRefreshToken
            {
                UserId = user.Id,
                Token = newRefreshTokenStr,
                ExpiresAt = expiresAt,
                Status = JwtRefreshTokenStatusEnum.Active,
                CreatedAt = DateTime.UtcNow
            };
            await _unitOfWork.Repository<JwtRefreshToken>().InsertAsync(newRefreshTokenEntity, cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return TResult<RefreshTokenResult>.Success(new RefreshTokenResult
        {
            AccessToken = newAccessToken,
            RefreshToken = newRefreshTokenStr,
            AccessTokenExpiresAt = GetAccessTokenExpiration(),
            RefreshTokenExpiresAt = expiresAt
        });
    }

    public async Task<TResult> RevokeRefreshToken(string refreshTokenId, CancellationToken cancellationToken)
    {
        var refreshToken = await _unitOfWork.Repository<JwtRefreshToken>()
            .FindOneAsync(filters: [t => t.Token == refreshTokenId]);

        if (refreshToken != null && refreshToken.IsActive)
        {
            refreshToken.RevokedAt = DateTime.UtcNow;
            refreshToken.Status = JwtRefreshTokenStatusEnum.Revoked;
            refreshToken.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.Repository<JwtRefreshToken>().UpdateAsync(refreshToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return TResult.Success();
    }

    /// <inheritdoc />
    public JwtValidationResult ValidateAccessToken(string token)
    {
        if (string.IsNullOrEmpty(token))
        {
            return new JwtValidationResult
            {
                IsValid = false,
                ErrorMessage = "Token is null or empty"
            };
        }

        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var principal = tokenHandler.ValidateToken(token, _validationParameters, out var validatedToken);

            if (validatedToken is not JwtSecurityToken jwtToken)
            {
                return new JwtValidationResult
                {
                    IsValid = false,
                    ErrorMessage = "Invalid token format"
                };
            }

            // Verify algorithm
            if (!jwtToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                return new JwtValidationResult
                {
                    IsValid = false,
                    ErrorMessage = "Invalid token algorithm"
                };
            }

            return new JwtValidationResult
            {
                IsValid = true,
                UserId = GetClaimValue<Guid?>(principal, "userId", s => Guid.TryParse(s, out var g) ? g : null),
                Username = GetClaimValue<string?>(principal, "username"),
                PhoneNumber = GetClaimValue<string?>(principal, "phone"),
                Role = GetClaimValue<string?>(principal, "role"),
                Policies = principal.FindAll("policy").Select(c => c.Value),
                ExpiresAt = jwtToken.ValidTo
            };
        }
        catch (SecurityTokenExpiredException)
        {
            return new JwtValidationResult
            {
                IsValid = false,
                ErrorMessage = "Token has expired"
            };
        }
        catch (SecurityTokenInvalidSignatureException)
        {
            return new JwtValidationResult
            {
                IsValid = false,
                ErrorMessage = "Invalid token signature"
            };
        }
        catch (SecurityTokenException ex)
        {
            return new JwtValidationResult
            {
                IsValid = false,
                ErrorMessage = $"Token validation failed: {ex.Message}"
            };
        }
        catch (Exception ex)
        {
            return new JwtValidationResult
            {
                IsValid = false,
                ErrorMessage = $"Unexpected error: {ex.Message}"
            };
        }
    }

    /// <inheritdoc />
    public Guid? GetUserIdFromExpiredToken(string token)
    {
        if (string.IsNullOrEmpty(token))
        {
            return null;
        }

        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();

            // Validate token without checking lifetime
            var validationParamsNoLifetime = _validationParameters.Clone();
            validationParamsNoLifetime.ValidateLifetime = false;

            var principal = tokenHandler.ValidateToken(token, validationParamsNoLifetime, out var validatedToken);

            if (validatedToken is not JwtSecurityToken jwtToken)
            {
                return null;
            }

            // Verify algorithm
            if (!jwtToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                return null;
            }

            var userIdClaim = principal.FindFirst("userId")?.Value;
            if (!string.IsNullOrEmpty(userIdClaim) && Guid.TryParse(userIdClaim, out var userId))
            {
                return userId;
            }

            return null;
        }
        catch
        {
            return null;
        }
    }

    /// <inheritdoc />
    public DateTime GetAccessTokenExpiration()
    {
        return DateTime.UtcNow.AddMinutes(_settings.AccessTokenExpirationMinutes);
    }

    /// <inheritdoc />
    public DateTime GetRefreshTokenExpiration()
    {
        return DateTime.UtcNow.AddDays(_settings.RefreshTokenExpirationDays);
    }

    #region Private Helpers

    private static T? GetClaimValue<T>(ClaimsPrincipal principal, string claimType, Func<string, T>? converter = null)
    {
        var claim = principal.FindFirst(claimType);
        if (claim == null)
        {
            return default;
        }

        if (converter != null)
        {
            return converter(claim.Value);
        }

        if (typeof(T) == typeof(string))
        {
            return (T)(object)claim.Value;
        }

        return default;
    }

    #endregion

    private async Task<IEnumerable<string>> GetUserPoliciesAsync(User user)
    {
        var policies = new HashSet<string>();

        // 1. Lấy policies từ Role
        if (user.RoleId.HasValue)
        {
            var rolePolicies = await _unitOfWork.Repository<RolePolicy>()
                .FindAsync(
                    filters: [
                                rp => rp.RoleId == user.RoleId.Value,
                                rp => !rp.IsDeleted,
                            ],
                    includes: [rp => rp.Policy!]);

            foreach (var rp in rolePolicies.Where(x => x.Policy != null && x.Policy.IsActive && !x.IsDeleted))
            {
                policies.Add(rp.Policy!.Code);
            }
        }

        // 2. Lấy UserPolicy (những quyền được cấp riêng hoặc bị thu hồi riêng)
        var userPolicies = await _unitOfWork.Repository<UserPolicy>()
            .FindAsync(
                filters:
                [
                    up => up.UserId == user.Id,
                    up => up.ExpiresAt == null || up.ExpiresAt > DateTime.UtcNow
                ],
                includes: [up => up.Policy!]);

        foreach (var up in userPolicies.Where(x => x.Policy != null && x.Policy.IsActive))
        {
            if (up.IsGranted)
            {
                policies.Add(up.Policy!.Code);
            }
            else
            {
                policies.Remove(up.Policy!.Code);
            }
        }

        return policies;
    }
}
