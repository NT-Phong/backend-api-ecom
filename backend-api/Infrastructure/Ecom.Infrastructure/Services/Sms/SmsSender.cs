using Ecom.Application.Common.Interfaces;

namespace Ecom.Infrastructure.Services.Sms;

/// <summary>
/// Deliberate safe placeholder: Ecom has no approved SMS provider configuration yet.
/// It is not registered in DI, so OTP flows continue using the existing application mechanism.
/// </summary>
internal sealed class SmsSender : ISmsSender
{
    public ValueTask SendAsync(string number, string otp, int expiresInMinutes, CancellationToken cancellationToken = default) =>
        ValueTask.FromException(new InvalidOperationException("SMS delivery is not configured for Ecom."));
}
