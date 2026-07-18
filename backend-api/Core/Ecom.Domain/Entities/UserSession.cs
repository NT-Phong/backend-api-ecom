using Ecom.Domain.Enums;

namespace Ecom.Domain.Entities;

public sealed class UserSession : BaseEntity
{
    private UserSession() { }
    public UserSession(Guid userId, SessionClientType clientType, AuthenticationMethod method,
        AuthenticationStrength strength, string securityStamp, string? deviceId, DateTime now,
        DateTime idleExpiresAt, DateTime absoluteExpiresAt)
    { UserId=userId; ClientType=clientType; AuthenticationMethod=method; AuthenticationStrength=strength;
      SecurityStamp=securityStamp; DeviceId=deviceId; CreatedAt=now; LastSeenAt=now; IdleExpiresAt=idleExpiresAt; AbsoluteExpiresAt=absoluteExpiresAt; }
    public Guid UserId { get; private set; }
    public SessionClientType ClientType { get; private set; }
    public AuthenticationMethod AuthenticationMethod { get; private set; }
    public AuthenticationStrength AuthenticationStrength { get; private set; }
    public string SecurityStamp { get; private set; } = null!;
    public string? DeviceId { get; private set; }
    public DateTime LastSeenAt { get; private set; }
    public DateTime IdleExpiresAt { get; private set; }
    public DateTime AbsoluteExpiresAt { get; private set; }
    public DateTime? RevokedAt { get; private set; }
    public string? RevocationReason { get; private set; }
    public bool IsActive(DateTime now, string currentStamp) => RevokedAt is null && now < IdleExpiresAt && now < AbsoluteExpiresAt && SecurityStamp == currentStamp;
    public void Revoke(DateTime now, string reason) { if (RevokedAt is null) { RevokedAt=now; RevocationReason=reason; } }
    public void Touch(DateTime now, DateTime idleExpiresAt) { if (RevokedAt is not null) throw new InvalidOperationException("Session is revoked."); LastSeenAt=now; IdleExpiresAt=idleExpiresAt < AbsoluteExpiresAt ? idleExpiresAt : AbsoluteExpiresAt; }
}
