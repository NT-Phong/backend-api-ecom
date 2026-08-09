using Ecom.Domain.Enums;

namespace Ecom.Application.Common.Interfaces;

public interface IOtpSecurityService
{
    string GenerateCode();
    string Protect(Guid userId, OtpTokenTypeEnum purpose, string code);
    bool Verify(Guid userId, OtpTokenTypeEnum purpose, string suppliedCode, string protectedOrLegacyCode);
    bool IsDevelopmentTestAccount(string phoneNumber);
    string? GetTestOtp(string phoneNumber, string? controlledBypassKey);
    bool CanExposeDevelopmentOtp { get; }
    string DevelopmentOtp { get; }
}
