namespace Ecom.Domain.Constants;

public static class ApplicationConstants
{
    public static class Timeouts
    {
        public const int DefaultCommandTimeoutSeconds = 30;
        public const int HttpClientTimeoutSeconds = 10;
        public const int CircuitBreakerTimeoutSeconds = 30;
        public const int RetryDelayBaseSeconds = 2;
        public const int MaxRetryAttempts = 3;
        public const int CircuitBreakerFailureThreshold = 5;
    }

    public static class HttpStatusMessages
    {
        public const string InternalServerError = "An internal server error occurred";
        public const string ValidationFailed = "Validation failed";
        public const string NotFound = "Resource not found";
        public const string Unauthorized = "Unauthorized access";
        public const string Forbidden = "Access forbidden";
        public const string BadRequest = "Bad request";
    }

    public static class Validation
    {
        public const int MaxNameLength = 200;
        public const int MaxDescriptionLength = 1000;
        public const int EmailMaxLength = 254;
        public const int PhoneMaxLength = 20;
        public const int AddressMaxLength = 500;
        public const int CurrencyCodeLength = 3;
    }

    public static class Pagination
    {
        public const int DefaultPageSize = 10;
        public const int MaxPageSize = 100;
    }

    public static class HttpHeaders
    {
        public const string CorrelationId = "x-correlation-id";
        public const string SourceIp = "x-source-ip";
        public const string TraceId = "x-trace-id";
        public const string SpanId = "x-span-id";
        public const string RequestId = "x-request-id";
        public const string ForwardedFor = "x-forwarded-for";
        public const string RealIp = "x-real-ip";
    }

    public static class Document
    {
        public const int thumbnailWidthDefault = 480;
        public const int thumbnailQualityDefault = 75;
    }
    public static class SystemConstants
    {
        public const string SuperAdminPhone = "0358357061";
    }
}

