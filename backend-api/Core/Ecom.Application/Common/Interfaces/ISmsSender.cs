namespace Ecom.Application.Common.Interfaces;

public interface ISmsSender
{
    bool IsConfigured { get; }
    ValueTask SendAsync(string number, string otp, int expiresInMinutes, CancellationToken cancellationToken = default);
}
