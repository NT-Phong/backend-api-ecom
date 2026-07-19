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

    public bool IsDevelopmentTestAccount(string phoneNumber) =>
        _environment.IsDevelopment() &&
        (_settings.EnableDevelopmentFixedOtp ||
         (_settings.EnableDevelopmentTestAccounts &&
          TestAccounts.All.Contains(phoneNumber, StringComparer.Ordinal)));

    private byte[] ComputeDigest(Guid userId, OtpTokenTypeEnum purpose, string code)
    {
        if (string.IsNullOrWhiteSpace(_settings.HashKey) || _settings.HashKey.Length < 32)
            throw new InvalidOperationException("OTP protection key is not configured.");

        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_settings.HashKey));
        return hmac.ComputeHash(Encoding.UTF8.GetBytes($"{userId:N}:{(int)purpose}:{code}"));
    }

    private static bool FixedTimeEquals(string left, string right)
    {
        var leftBytes = Encoding.UTF8.GetBytes(left);
        var rightBytes = Encoding.UTF8.GetBytes(right);
        return leftBytes.Length == rightBytes.Length &&
               CryptographicOperations.FixedTimeEquals(leftBytes, rightBytes);
    }
}
