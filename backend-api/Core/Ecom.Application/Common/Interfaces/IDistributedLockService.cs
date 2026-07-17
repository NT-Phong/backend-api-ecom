namespace Ecom.Application.Common.Interfaces;

/// <summary>
/// Quản lý khóa (Lock) để tránh xung đột khi xử lý song song.
/// </summary>
public interface IDistributedLockService
{
    /// <summary>
    /// Thử chiếm khóa theo mã Key.
    /// </summary>
    Task<IAsyncDisposable?> TryAcquireAsync(
        string key,
        TimeSpan waitTimeout,
        CancellationToken cancellationToken = default);
}

