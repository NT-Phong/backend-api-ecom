using Ecom.Domain.Enums;

namespace Ecom.Domain.Entities;

public sealed class VerificationChallenge : BaseEntity
{
    private VerificationChallenge() { }
    public VerificationChallenge(Guid? userId, VerificationChallengePurpose purpose, string destinationHash,
        string secretHash, int maxAttempts, DateTime expiresAt, string createdByIpHash)
    {
        if (maxAttempts < 1) throw new ArgumentOutOfRangeException(nameof(maxAttempts));
        UserId = userId; Purpose = purpose; DestinationHash = destinationHash; SecretHash = secretHash;
        MaxAttempts = maxAttempts; ExpiresAt = expiresAt; CreatedByIpHash = createdByIpHash;
    }
    public Guid? UserId { get; private set; }
    public VerificationChallengePurpose Purpose { get; private set; }
    public string DestinationHash { get; private set; } = null!;
    public string SecretHash { get; private set; } = null!;
    public VerificationChallengeStatus Status { get; private set; } = VerificationChallengeStatus.Pending;
    public int FailedAttempts { get; private set; }
    public int MaxAttempts { get; private set; }
    public DateTime ExpiresAt { get; private set; }
    public DateTime? ConsumedAt { get; private set; }
    public DateTime? SupersededAt { get; private set; }
    public string CreatedByIpHash { get; private set; } = null!;
    public bool IsUsable(DateTime now) => Status == VerificationChallengeStatus.Pending && now < ExpiresAt;
    public void Consume(DateTime now) { EnsurePending(now); Status = VerificationChallengeStatus.Consumed; ConsumedAt = now; }
    public void RegisterFailure(DateTime now) { EnsurePending(now); FailedAttempts++; if (FailedAttempts >= MaxAttempts) Status = VerificationChallengeStatus.Locked; }
    public void Supersede(DateTime now) { EnsurePending(now); Status = VerificationChallengeStatus.Superseded; SupersededAt = now; }
    public void Expire(DateTime now) { if (Status == VerificationChallengeStatus.Pending && now >= ExpiresAt) Status = VerificationChallengeStatus.Expired; }
    private void EnsurePending(DateTime now) { Expire(now); if (Status != VerificationChallengeStatus.Pending) throw new InvalidOperationException("Challenge is not pending."); }
}
