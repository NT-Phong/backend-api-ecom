namespace Ecom.Application.Common.Interfaces;

public interface IFcmService
{
    Task<FcmSendResult> SendMulticastAsync(IEnumerable<string> tokens, string title, string body, Dictionary<string, string>? data = null, CancellationToken cancellationToken = default);
}

public sealed record FcmSendResult(int TotalTokens, int SuccessCount, int FailureCount, int BatchFailureCount, IReadOnlyList<string> SuccessfulTokens, IReadOnlyList<FcmTokenFailure> TokenFailures, IReadOnlyList<string> BatchErrors)
{
    public static FcmSendResult Empty { get; } = new(0, 0, 0, 0, [], [], []);
}

public sealed record FcmTokenFailure(string Token, string? ErrorCode, bool IsUnrecoverable);
