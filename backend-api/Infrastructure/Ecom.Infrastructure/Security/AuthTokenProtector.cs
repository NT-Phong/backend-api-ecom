using System.Security.Cryptography;
using System.Text;

namespace Ecom.Infrastructure.Security;

public sealed class AuthTokenProtector : IAuthTokenProtector
{
    private const string Prefix = "sha256:";

    public string Protect(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
            throw new ArgumentException("A token is required.", nameof(token));

        return Prefix + Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));
    }

    public bool IsProtected(string value) => value.StartsWith(Prefix, StringComparison.Ordinal);
}
