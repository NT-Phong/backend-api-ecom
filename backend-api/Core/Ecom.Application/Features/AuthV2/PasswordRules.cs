using System.Text.RegularExpressions;
namespace Ecom.Application.Features.AuthV2;
internal static partial class PasswordRules
{
    private static readonly HashSet<string> Reserved = new(StringComparer.OrdinalIgnoreCase) { "admin", "administrator", "system", "support", "security", "root", "api" };
    private static readonly HashSet<string> Common = new(StringComparer.OrdinalIgnoreCase) { "password", "password123", "1234567890", "qwerty12345", "letmein123" };
    public static bool ValidUsername(string value) => value.Length is >= 4 and <= 32 && UsernameRegex().IsMatch(value) && value.Any(char.IsLetter) && !Reserved.Contains(value);
    public static bool ValidPassword(string value, int minimumLength) =>
        value.Length >= minimumLength && value.Length <= 128 && !Common.Contains(value);
    [GeneratedRegex("^[A-Za-z0-9._-]+$")]
    private static partial Regex UsernameRegex();
}
