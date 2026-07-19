using Ecom.Application.Features.Demo.QrLogin;

namespace Ecom.Application.Common.Interfaces;

public interface IDemoQrLoginStore
{
    Task CreateAsync(DemoQrLoginAttempt attempt, CancellationToken cancellationToken = default);
    Task<DemoQrLoginAttempt?> GetAsync(Guid id, CancellationToken cancellationToken = default);
    Task<DemoQrLoginTransitionResult> TryTransitionAsync(
        Guid id,
        DemoQrLoginStatus targetStatus,
        Guid userId,
        DateTime now,
        CancellationToken cancellationToken = default);
}

public sealed class DemoQrLoginStoreUnavailableException(string message, Exception innerException)
    : Exception(message, innerException);
