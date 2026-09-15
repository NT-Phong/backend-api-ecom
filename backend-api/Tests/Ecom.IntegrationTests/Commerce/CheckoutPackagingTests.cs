using Ecom.Application.Common.Commerce;

namespace Ecom.IntegrationTests.Commerce;

public sealed class CheckoutPackagingTests
{
    [Theory]
    [InlineData(null, CheckoutPackaging.Standard)]
    [InlineData(" STANDARD ", CheckoutPackaging.Standard)]
    [InlineData("COLD_CHAIN", CheckoutPackaging.ColdChain)]
    public void TryNormalize_returns_the_canonical_supported_value(string? input, string expected)
    {
        Assert.True(CheckoutPackaging.TryNormalize(input, out var actual));
        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData("")]
    [InlineData("express")]
    [InlineData("cold chain")]
    public void TryNormalize_rejects_unsupported_values(string input)
    {
        Assert.False(CheckoutPackaging.TryNormalize(input, out _));
        Assert.False(CheckoutPackaging.IsSupported(input));
    }
}
