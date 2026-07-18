using Ecom.Application.Common.Interfaces;

namespace Ecom.Infrastructure.Services.Sms;

/// <summary>
/// Deliberate fail-closed placeholder: Ecom has no approved SMS provider configuration yet.
/// It is registered so production OTP issuance returns 503 instead of reporting a fake delivery.
/// </summary>
public sealed class SmsSender : ISmsSender
{
    public bool IsConfigured => false;

    public ValueTask SendAsync(string number, string otp, int expiresInMinutes, CancellationToken cancellationToken = default) =>
        ValueTask.FromException(new InvalidOperationException("SMS delivery is not configured for Ecom."));
}
