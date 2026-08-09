using Ecom.Application.Common.Configuration;
using Ecom.Domain.Enums;
using Microsoft.Extensions.Hosting;
using System.Security.Cryptography;
using System.Text;

namespace Ecom.Infrastructure.Security;

public sealed class OtpSecurityService : IOtpSecurityService
{
    private const int StoredHashLength = 10;
    private readonly OtpSettings _settings;
    private readonly IHostEnvironment _environment;

    public OtpSecurityService(IOptions<OtpSettings> options, IHostEnvironment environment)
    {
        _settings = options.Value;
        _environment = environment;
    }

    public bool CanExposeDevelopmentOtp =>
        _environment.IsDevelopment() &&
        (_settings.EnableDevelopmentTestAccounts || _settings.EnableDevelopmentFixedOtp) &&
        _settings.ExposeDevelopmentOtp;

    public string DevelopmentOtp => _settings.DefaultOtp;

    public string GenerateCode()
    {
        if (_settings.OtpLength is < 4 or > 9)
            throw new InvalidOperationException("OTP length must be between 4 and 9 digits.");

        var upperBound = (int)Math.Pow(10, _settings.OtpLength);
        return RandomNumberGenerator.GetInt32(upperBound).ToString($"D{_settings.OtpLength}");
    }

    public string Protect(Guid userId, OtpTokenTypeEnum purpose, string code)
    {
        var digest = ComputeDigest(userId, purpose, code);
        return Convert.ToHexString(digest)[..StoredHashLength];
    }

    public bool Verify(Guid userId, OtpTokenTypeEnum purpose, string suppliedCode, string protectedOrLegacyCode)
    {
        if (string.IsNullOrWhiteSpace(suppliedCode) || string.IsNullOrWhiteSpace(protectedOrLegacyCode))
            return false;

        var expected = Protect(userId, purpose, suppliedCode);
        if (FixedTimeEquals(expected, protectedOrLegacyCode))
            return true;

        // Transitional dual-read for legacy rows. New and rotated OTP values are always protected.
        return protectedOrLegacyCode.Length < StoredHashLength &&
               FixedTimeEquals(suppliedCode, protectedOrLegacyCode);
    }

    public bool IsDevelopmentTestAccount(string phoneNumber)
    {
        if (!_environment.IsDevelopment())
            return false;

        if (_settings.EnableDevelopmentFixedOtp)
            return true;

        if (!_settings.EnableDevelopmentTestAccounts)
            return false;

        // A configured account narrows the bypass to one deterministic local test identity.
        // The legacy TestAccounts list remains the fallback for test harnesses that omit it.
        var configuredPhone = _settings.TestPhoneNumber?.Trim();
        return !string.IsNullOrWhiteSpace(configuredPhone)
            ? string.Equals(phoneNumber, configuredPhone, StringComparison.Ordinal)
            : TestAccounts.All.Contains(phoneNumber, StringComparer.Ordinal);
    }

    public string? GetTestOtp(string phoneNumber, string? controlledBypassKey)
    {
        if (IsDevelopmentTestAccount(phoneNumber))
            return DevelopmentOtp;

        if (!_settings.EnableControlledTestBypass ||
            !_settings.ControlledTestBypassExpiresAtUtc.HasValue ||
            _settings.ControlledTestBypassExpiresAtUtc <= DateTimeOffset.UtcNow ||
            !string.Equals(phoneNumber, _settings.ControlledTestPhoneNumber.Trim(), StringComparison.Ordinal) ||
            !FixedTimeEquals(controlledBypassKey, _settings.ControlledTestBypassKey))
        {
            return null;
        }

        return _settings.ControlledTestOtp;
    }

    private byte[] ComputeDigest(Guid userId, OtpTokenTypeEnum purpose, string code)
    {
        if (string.IsNullOrWhiteSpace(_settings.HashKey) || _settings.HashKey.Length < 32)
            throw new InvalidOperationException("OTP protection key is not configured.");

        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_settings.HashKey));
        return hmac.ComputeHash(Encoding.UTF8.GetBytes($"{userId:N}:{(int)purpose}:{code}"));
    }

    private static bool FixedTimeEquals(string? left, string? right)
    {
        if (left is null || right is null)
            return false;

        var leftBytes = Encoding.UTF8.GetBytes(left);
        var rightBytes = Encoding.UTF8.GetBytes(right);
        return leftBytes.Length == rightBytes.Length &&
               CryptographicOperations.FixedTimeEquals(leftBytes, rightBytes);
    }
}
