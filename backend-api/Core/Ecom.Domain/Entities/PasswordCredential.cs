namespace Ecom.Domain.Entities;

public sealed class PasswordCredential : BaseEntity
{
    private PasswordCredential() { }
    public PasswordCredential(Guid userId, string passwordHash, DateTime now, bool mustChangePassword = false)
    { UserId=userId; SetHash(passwordHash, now); MustChangePassword=mustChangePassword; }
    public Guid UserId { get; private set; }
    public string PasswordHash { get; private set; } = null!;
    public string Algorithm { get; private set; } = "bcrypt";
    public string AlgorithmVersion { get; private set; } = "bcrypt-v1";
    public DateTime PasswordChangedAt { get; private set; }
    public DateTime? LastVerifiedAt { get; private set; }
    public int FailedAttempts { get; private set; }
    public DateTime? LockoutEnd { get; private set; }
    public DateTime? LastFailedAt { get; private set; }
    public DateTime? LockedUntil { get; private set; }
    public bool MustChangePassword { get; private set; }
    public void RecordFailure(DateTime now) { FailedAttempts++; LastFailedAt=now; if (FailedAttempts >= 5) LockedUntil = LockoutEnd = now.AddMinutes(Math.Min(30, FailedAttempts)); }
    public void RecordSuccess(DateTime now) { FailedAttempts=0; LockedUntil=LockoutEnd=null; LastVerifiedAt=now; }
    public void SetHash(string hash, DateTime now) { if (string.IsNullOrWhiteSpace(hash)) throw new ArgumentException("Hash is required."); PasswordHash=hash; PasswordChangedAt=now; MustChangePassword=false; }
}
