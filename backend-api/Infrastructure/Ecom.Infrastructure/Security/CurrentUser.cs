using System.Security.Claims;
using Ecom.Application.Common.Interfaces;
using Microsoft.AspNetCore.Http;

namespace Ecom.Infrastructure.Security;

/// <summary>
/// Implementation của ICurrentUser - lấy thông tin user từ JWT claims
/// </summary>
public class CurrentUser : ICurrentUser
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUser(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    /// <inheritdoc />
    public Guid UserId
    {
        get
        {
            var userIdString = GetClaimValue("userId") ?? GetClaimValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(userIdString, out var userId) ? userId : Guid.Empty;
        }
    }

    /// <inheritdoc />
    public string? UserIdString => GetClaimValue("userId") ?? GetClaimValue(ClaimTypes.NameIdentifier);

    /// <inheritdoc />
    public string? PhoneNumber => GetClaimValue("phone") ?? GetClaimValue(ClaimTypes.MobilePhone);

    /// <inheritdoc />
    public string? Email => GetClaimValue(ClaimTypes.Email);

    /// <inheritdoc />
    public bool IsAuthenticated => _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;

    /// <inheritdoc />
    public string? Role => GetClaimValue("role") ?? GetClaimValue(ClaimTypes.Role);

    /// <inheritdoc />
    public IEnumerable<string> Roles => GetClaimValues(ClaimTypes.Role);

    /// <inheritdoc />
    public IEnumerable<string> Policies => GetClaimValues("policy");
    public Guid SessionId => Guid.TryParse(GetClaimValue("session_id"), out var value) ? value : Guid.Empty;
    public string? SecurityStamp => GetClaimValue("security_stamp");

    /// <inheritdoc />
    public bool HasRole(string role)
    {
        return Roles.Contains(role, StringComparer.OrdinalIgnoreCase);
    }

    /// <inheritdoc />
    public bool HasPolicy(string policy)
    {
        return Policies.Contains(policy, StringComparer.OrdinalIgnoreCase);
    }

    private string? GetClaimValue(string claimType)
    {
        return _httpContextAccessor.HttpContext?.User?.FindFirst(claimType)?.Value;
    }

    private IEnumerable<string> GetClaimValues(string claimType)
    {
        return _httpContextAccessor.HttpContext?.User?.FindAll(claimType)?.Select(c => c.Value) ?? [];
    }
}

