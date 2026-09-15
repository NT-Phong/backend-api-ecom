namespace Ecom.Application.Common.Commerce;

/// <summary>
/// Canonical checkout packaging values.  Keep this at the application boundary so preview,
/// order creation and idempotency all reason about the same value.
/// </summary>
public static class CheckoutPackaging
{
    public const string Standard = "standard";
    public const string ColdChain = "cold_chain";

    public static bool IsSupported(string? value) => value is null ||
        string.Equals(value.Trim(), Standard, StringComparison.OrdinalIgnoreCase) ||
        string.Equals(value.Trim(), ColdChain, StringComparison.OrdinalIgnoreCase);

    public static bool TryNormalize(string? value, out string packaging)
    {
        if (value is null)
        {
            packaging = Standard;
            return true;
        }

        var normalized = value.Trim();
        if (string.Equals(normalized, Standard, StringComparison.OrdinalIgnoreCase))
        {
            packaging = Standard;
            return true;
        }

        if (string.Equals(normalized, ColdChain, StringComparison.OrdinalIgnoreCase))
        {
            packaging = ColdChain;
            return true;
        }

        packaging = string.Empty;
        return false;
    }
}
