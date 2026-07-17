namespace Ecom.Application.Common.Models;

public class ApiResponse<T>
{
    public bool Success { get; set; }
    public T? Data { get; set; }
    public string? Message { get; set; }
    public string? ErrorCode { get; set; }
    public Dictionary<string, string[]>? ValidationErrors { get; set; }
    public string? Details { get; set; }

    public DateTimeOffset Timestamp { get; set; } = DateTime.UtcNow;

    public static ApiResponse<T?> Ok(T? data, string? message = null)
    {
        return new ApiResponse<T?> { Success = true, Data = data, Message = message ?? "Success" };
    }

    public static ApiResponse<T> Fail(string? message, ErrorCodes? errorCode = null, Dictionary<string, string[]>? validationErrors = null)
    {
        string? details = null;

        if (validationErrors != null && validationErrors.Count != 0)
        {
            // Lấy tất cả các mảng string[] trong Value, sau đó gộp tất cả message lại
            var allErrors = validationErrors.Values.SelectMany(x => x);
            details = string.Join("; ", allErrors);
        }

        return new ApiResponse<T>
        {
            Success = false,
            Message = message,
            ErrorCode = errorCode?.ToString(),
            ValidationErrors = validationErrors,
            Details = details
        };
    }
} 

