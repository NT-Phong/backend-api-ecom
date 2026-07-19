namespace Ecom.Domain.Extensions;

public static class VietnamesePhoneNumber
{
    public static bool TryNormalize(string? value, out string normalized)
    {
        normalized = string.Empty;
        if (string.IsNullOrWhiteSpace(value)) return false;

        var digits = new string(value.Where(char.IsDigit).ToArray());
        if (digits.StartsWith("84", StringComparison.Ordinal) && digits.Length == 11)
            digits = "0" + digits[2..];

        if (digits.Length != 10 || !digits.StartsWith('0') || !"35789".Contains(digits[1]))
            return false;

        normalized = digits;
        return true;
    }

    public static string Normalize(string value)
    {
        if (!TryNormalize(value, out var normalized))
            throw new ArgumentException("Invalid Vietnamese phone number.", nameof(value));

        return normalized;
    }
}
