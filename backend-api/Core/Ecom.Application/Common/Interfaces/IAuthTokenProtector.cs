namespace Ecom.Application.Common.Interfaces;

/// <summary>
/// One-way protection for high-entropy bearer credentials persisted for lookup.
/// </summary>
public interface IAuthTokenProtector
{
    string Protect(string token);
    bool IsProtected(string value);
}
