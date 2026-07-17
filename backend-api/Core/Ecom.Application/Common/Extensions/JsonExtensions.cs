using System;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace Ecom.Application.Common.Extensions;

public static class JsonExtensions
{
    private static readonly JsonSerializerOptions _defaultOptions = new JsonSerializerOptions 
    { 
        PropertyNameCaseInsensitive = true 
    };

    /// <summary>
    /// Parsed chuỗi JSON sang Object an toàn (Không ném Exception, tự trả về new T() nếu lỗi hoặc chuỗi rỗng).
    /// </summary>
    public static T ParseSafe<T>(this string? jsonData, ILogger? logger = null) where T : new()
    {
        if (string.IsNullOrWhiteSpace(jsonData) || jsonData == "{}") 
        {
            return new T();
        }

        try
        {
            return JsonSerializer.Deserialize<T>(jsonData, _defaultOptions) ?? new T();
        }
        catch (Exception ex)
        {
            logger?.LogWarning(ex, "Failed to parse JSON data for type {Type}", typeof(T).Name);
            return new T();
        }
    }
}

