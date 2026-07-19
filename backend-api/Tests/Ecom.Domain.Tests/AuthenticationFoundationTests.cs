using Ecom.Domain.Entities;
using Ecom.Domain.Enums;
using Ecom.Domain.Extensions;

namespace Ecom.Domain.Tests;

public sealed class AuthenticationFoundationTests
{
    [Fact]
    public void User_cannot_be_activated_without_verified_identity()
    {
        var user = new User("0901234567", null);
        Assert.Throws<InvalidOperationException>(() => user.Activate());
        user.MarkPhoneVerified(); user.Activate();
        Assert.Equal(UserStatusEnum.Active, user.Status);
    }

    [Fact]
    public void Challenge_locks_at_max_attempts_and_cannot_be_consumed()
    {
        var now = DateTime.UtcNow;
        var challenge = new VerificationChallenge(null, VerificationChallengePurpose.Login, "destination", "secret", 2, now.AddMinutes(5), "ip");
        challenge.RegisterFailure(now); challenge.RegisterFailure(now);
        Assert.Equal(VerificationChallengeStatus.Locked, challenge.Status);
        Assert.Throws<InvalidOperationException>(() => challenge.Consume(now));
    }

    [Fact]
    public void Refresh_token_cannot_rotate_twice()
    {
        var now = DateTime.UtcNow;
        var token = new SessionRefreshToken(Guid.NewGuid(), Guid.NewGuid(), "hash", now, now.AddDays(1));
        token.MarkRotated(now, Guid.NewGuid());
        Assert.Throws<InvalidOperationException>(() => token.MarkRotated(now, Guid.NewGuid()));
    }

    [Fact]
    public void Revoking_session_is_idempotent()
    {
        var now = DateTime.UtcNow;
        var session = new UserSession(Guid.NewGuid(), SessionClientType.Mobile, AuthenticationMethod.Otp,
            AuthenticationStrength.SingleFactor, "stamp", null, now, now.AddHours(1), now.AddDays(1));
        session.Revoke(now, "Logout"); session.Revoke(now.AddMinutes(1), "Other");
        Assert.Equal(now, session.RevokedAt); Assert.Equal("Logout", session.RevocationReason);
    }

    [Fact]
    public void Username_is_normalized_and_password_credential_tracks_lockout()
    {
        var user = new User(null, null); user.SetUsername("Buyer.One");
        Assert.Equal("BUYER.ONE", user.NormalizedUsername);
        var now = DateTime.UtcNow; var credential = new PasswordCredential(user.Id, "bcrypt-hash", now);
        for (var i = 0; i < 5; i++) credential.RecordFailure(now);
        Assert.NotNull(credential.LockoutEnd);
        Assert.NotNull(credential.LockedUntil);
        Assert.NotNull(credential.LastFailedAt);
        Assert.Equal("bcrypt-v1", credential.AlgorithmVersion);
        credential.RecordSuccess(now.AddMinutes(1)); Assert.Null(credential.LockoutEnd);
    }

    [Theory]
    [InlineData("0912345678")]
    [InlineData("+84912345678")]
    [InlineData("84 912 345 678")]
    public void Vietnamese_phone_formats_normalize_to_one_canonical_value(string input)
    {
        Assert.True(VietnamesePhoneNumber.TryNormalize(input, out var normalized));
        Assert.Equal("0912345678", normalized);

        var user = new User(input, null);
        Assert.Equal("0912345678", user.PhoneNumber);
        Assert.Equal("0912345678", user.NormalizedPhoneNumber);
    }

    [Fact]
    public void Basic_profile_name_does_not_mark_full_profile_complete()
    {
        var user = new User("0912345678", null);
        user.SetBasicProfile("  Nguyen Van A ");

        Assert.Equal("Nguyen Van A", user.FullName);
        Assert.False(user.IsProfileCompleted);
    }
}
