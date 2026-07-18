namespace Ecom.Domain.Entities;

public sealed class SessionRefreshToken : BaseEntity
{
    private SessionRefreshToken() { }
    public SessionRefreshToken(Guid sessionId, Guid familyId, string tokenHash, DateTime now, DateTime expiresAt)
    { SessionId=sessionId; FamilyId=familyId; TokenHash=tokenHash; CreatedAt=now; ExpiresAt=expiresAt; }
    public Guid SessionId { get; private set; }
    public Guid FamilyId { get; private set; }
    public string TokenHash { get; private set; } = null!;
    public DateTime ExpiresAt { get; private set; }
    public DateTime? UsedAt { get; private set; }
    public DateTime? RevokedAt { get; private set; }
    public Guid? ReplacedByTokenId { get; private set; }
    public bool CanRotate(DateTime now) => UsedAt is null && RevokedAt is null && now < ExpiresAt;
    public void MarkRotated(DateTime now, Guid replacementId) { if (!CanRotate(now)) throw new InvalidOperationException("Refresh token cannot be rotated."); UsedAt=now; ReplacedByTokenId=replacementId; }
    public void Revoke(DateTime now) { RevokedAt ??= now; }
}
