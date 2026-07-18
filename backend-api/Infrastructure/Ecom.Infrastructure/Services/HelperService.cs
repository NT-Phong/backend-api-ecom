using Ecom.Application.Common.Interfaces;
using System.Security.Cryptography;
using System.Text;

namespace Ecom.Infrastructure.Services;

public sealed class HelperService : IHelperService
{
    private const string Alphabet = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";

    public string GetRandomString(int length = 16, TextLetterCase letterCase = TextLetterCase.Normal)
    {
        if (length <= 0) throw new ArgumentOutOfRangeException(nameof(length));
        var value = new string(Enumerable.Range(0, length).Select(_ => Alphabet[RandomNumberGenerator.GetInt32(Alphabet.Length)]).ToArray());
        return letterCase switch { TextLetterCase.Upper => value.ToUpperInvariant(), TextLetterCase.Lower => value.ToLowerInvariant(), _ => value };
    }

    public string GetRandomStringNumber(int length = 8)
    {
        if (length <= 0) throw new ArgumentOutOfRangeException(nameof(length));
        return new string(Enumerable.Range(0, length).Select(_ => (char)('0' + RandomNumberGenerator.GetInt32(10))).ToArray());
    }

    public string GetPublicPhrase(string secretKey, string secretPhrase)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(secretKey);
        ArgumentException.ThrowIfNullOrWhiteSpace(secretPhrase);
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secretKey));
        return Convert.ToHexString(hmac.ComputeHash(Encoding.UTF8.GetBytes(secretPhrase))).ToLowerInvariant();
    }

    public byte[] ConvertBase64ToBinaries(string base64String) => Convert.FromBase64String(base64String);
}
