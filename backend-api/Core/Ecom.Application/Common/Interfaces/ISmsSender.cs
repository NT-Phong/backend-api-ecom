namespace Ecom.Application.Common.Interfaces;

public interface ISmsSender
{
    ValueTask SendAsync(string number, string otp, int expiresInMinutes, CancellationToken cancellationToken = default);
}
