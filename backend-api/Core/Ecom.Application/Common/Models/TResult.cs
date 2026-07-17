namespace Ecom.Application.Common.Models;

public class TResult<T> : IResult
{
    public bool IsSuccess { get; }
    public T Data { get; }
    public string? Error { get; }
    public ErrorCodes? ErrorCode { get; }
    public Dictionary<string, string[]>? ValidationErrors { get; }

    private TResult(bool isSuccess, T data, string? error, ErrorCodes? errorCode, Dictionary<string, string[]>? validationErrors)
    {
        IsSuccess = isSuccess;
        Data = data;
        Error = error;
        ErrorCode = errorCode;
        ValidationErrors = validationErrors;
    }

    public static TResult<T> Success(T data) => new(true, data, null, null, null);
    public static TResult<T> Failure(string error, ErrorCodes? errorCode = null) => new(false, default, error, errorCode, null);
    public static TResult<T> ValidationFailure(Dictionary<string, string[]> validationErrors) => new(false, default, "Validation failed", ErrorCodes.BAD_REQUEST, validationErrors);

    public static implicit operator TResult<T>(T data) => Success(data);
}

public class TResult : IResult
{
    public bool IsSuccess { get; }
    public string? Error { get; }
    public ErrorCodes? ErrorCode { get; }
    public Dictionary<string, string[]>? ValidationErrors { get; }

    private TResult(bool isSuccess, string? error, ErrorCodes? errorCode, Dictionary<string, string[]>? validationErrors)
    {
        IsSuccess = isSuccess;
        Error = error;
        ErrorCode = errorCode;
        ValidationErrors = validationErrors;
    }

    public static TResult Success() => new(true, null, null, null);
    public static TResult Failure(string error, ErrorCodes? errorCode = null) => new(false, error, errorCode, null);
    public static TResult ValidationFailure(Dictionary<string, string[]> validationErrors) => new(false, "Validation failed", ErrorCodes.BAD_REQUEST, validationErrors);
}

