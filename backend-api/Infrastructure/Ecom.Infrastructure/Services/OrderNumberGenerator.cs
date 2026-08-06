using System.Security.Cryptography;

namespace Ecom.Infrastructure.Services;

public sealed class OrderNumberGenerator : IOrderNumberGenerator
{
    public string Create(DateTime nowUtc) => $"ORD-{nowUtc:yyyyMMdd}-{Convert.ToHexString(RandomNumberGenerator.GetBytes(5))}";
}
