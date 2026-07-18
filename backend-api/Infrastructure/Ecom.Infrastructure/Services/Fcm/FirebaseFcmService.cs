using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using Ecom.Application.Common.Configuration;
using Ecom.Application.Common.Interfaces;
using Polly;

namespace Ecom.Infrastructure.Services.Fcm;

public class FirebaseFcmService(
    IOptions<FcmSettings> fcmOptions,
    ILogger<FirebaseFcmService> logger
) : IFcmService
{
    private static FirebaseApp? _app;
    private static readonly object _lock = new();

    private FirebaseApp GetOrCreateApp()
    {
        if (_app != null) return _app;
        lock (_lock)
        {
            if (_app != null) return _app;

            if (string.IsNullOrWhiteSpace(fcmOptions.Value.CredentialsJson))
            {
                throw new InvalidOperationException("Firebase Credentials JSON is not configured in FcmSettings.");
            }

            var credential = CredentialFactory
                .FromJson<ServiceAccountCredential>(fcmOptions.Value.CredentialsJson)
                .ToGoogleCredential();
            _app = FirebaseApp.Create(new AppOptions { Credential = credential });
        }
        return _app;
    }

    public async Task<FcmSendResult> SendMulticastAsync(
        IEnumerable<string> tokens,
        string title, string body,
        Dictionary<string, string>? data = null,
        CancellationToken cancellationToken = default)
    {
        var tokenList = tokens.ToList();
        if (!tokenList.Any()) return FcmSendResult.Empty;

        var successfulTokens = new List<string>();
        var tokenFailures = new List<FcmTokenFailure>();
        var batchErrors = new List<string>();
        var batchFailureCount = 0;

        // FCM Multicast limits to 500 tokens per batch
        foreach (var chunk in tokenList.Chunk(500))
        {
            var message = new MulticastMessage
            {
                Tokens = chunk,
                Notification = new FirebaseAdmin.Messaging.Notification { Title = title, Body = body },
                Data = data,
                Android = new AndroidConfig
                {
                    Priority = Priority.High,
                    Notification = new AndroidNotification
                    {
                        Sound = "default",
                        ChannelId = "default"
                    }
                },
                Apns = new ApnsConfig
                {
                    Aps = new Aps
                    {
                        Sound = "default",
                        ContentAvailable = true
                    },
                    Headers = new Dictionary<string, string>
                    {
                        ["apns-priority"] = "10"
                    }
                }
            };

            try
            {
                var messaging = FirebaseMessaging.GetMessaging(GetOrCreateApp());

                // Retry policy: thử tối đa 3 lần 
                var retryPolicy = Polly.Policy
                    .Handle<FirebaseException>()
                    .Or<HttpRequestException>()
                    .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                    (exception, timeSpan, retryCount, context) =>
                    {
                        logger.LogWarning(exception, "[FCM] Multicast send failed on attempt {RetryCount}. Waiting {Delay}s before retrying", retryCount, timeSpan.TotalSeconds);
                    });

                var response = await retryPolicy.ExecuteAsync(() => messaging.SendEachForMulticastAsync(message, cancellationToken));

                for (int i = 0; i < response.Responses.Count; i++)
                {
                    var r = response.Responses[i];
                    if (r.IsSuccess)
                    {
                        successfulTokens.Add(chunk[i]);
                        continue;
                    }

                    var errorCode = r.Exception?.MessagingErrorCode;

                    // Deactivate các lỗi không retry được
                    bool isUnrecoverable = errorCode
                        is MessagingErrorCode.Unregistered
                        or MessagingErrorCode.InvalidArgument
                        or MessagingErrorCode.SenderIdMismatch;

                    tokenFailures.Add(new FcmTokenFailure(
                        chunk[i],
                        errorCode?.ToString(),
                        isUnrecoverable));
                }

                logger.LogInformation(
                    "FCM_SEND_RESULT: {SuccessCount} success, {FailureCount} failure / {Total}",
                    response.SuccessCount, response.FailureCount, chunk.Length);
            }
            catch (Exception ex)
            {
                batchFailureCount++;
                batchErrors.Add(ex.GetType().Name);
                logger.LogError(ex,
                    "FCM_BATCH_EXCEPTION: Multicast batch failed after retries. Tokens are kept active for later sends. TokenCount={TokenCount}",
                    chunk.Length);
            }
        }

        return new FcmSendResult(
            tokenList.Count,
            successfulTokens.Count,
            tokenFailures.Count,
            batchFailureCount,
            successfulTokens,
            tokenFailures,
            batchErrors);
    }
}
