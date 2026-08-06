using System.Text.Json;
using System.Text.Encodings.Web;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;
using Serilog.Events;
using Serilog.Formatting;

namespace Ecom.Infrastructure.Logging;

public class CustomJsonFormatter : ITextFormatter
{
    private static readonly List<Regex> SensitivePatterns = new();
    private static readonly List<Regex> FallbackPatterns = new();
    private static bool _patternsInitialized = false;
    private static bool _fallbackInitialized = false;
    private static bool _includeExceptionDetails;
    private static readonly object _lockObject = new();

    public static void InitializePatterns(IConfiguration configuration)
    {
        if (_patternsInitialized) return;

        lock (_lockObject)
        {
            if (_patternsInitialized) return;

            var patterns = configuration.GetSection("Serilog:SensitiveDataPatterns").Get<string[]>()
                ?? GetDefaultPatterns();
            _includeExceptionDetails = configuration.GetValue<bool>("Serilog:IncludeExceptionDetails");

            foreach (var pattern in patterns)
            {
                try
                {
                    SensitivePatterns.Add(new Regex(pattern,
                        RegexOptions.Compiled | RegexOptions.IgnoreCase));
                }
                catch (ArgumentException)
                {
                    // Skip invalid regex patterns
                    continue;
                }
            }
            _patternsInitialized = true;
        }
    }

    private static string[] GetDefaultPatterns()
    {
        return new[]
        {
            @"token[=:\s]+[^\s&]+",
            @"otp[=:\s]+[^\s&]+",
            @"refresh[_-]?token[=:\s]+[^\s&]+",
            @"authorization[_-]?code[=:\s]+[^\s&]+",
            @"cookie[=:\s]+[^\s&]+",
            @"phone(number)?[=:\s]+[^\s&]+",
            @"email[=:\s]+[^\s&]+",
            @"api[_-]?key[=:\s]+[^\s&]+",
            @"password[=:\s]+[^\s&]+",
            @"secret[=:\s]+[^\s&]+",
            @"authorization[=:\s]+[^\s&]+",
            @"bearer\s+[^\s&]+",
            @"basic\s+[^\s&]+",
            @"x-api-key[=:\s]+[^\s&]+",
            @"x-auth-token[=:\s]+[^\s&]+"
        };
    }

    public void Format(LogEvent logEvent, TextWriter output)
    {
        using var jsonWriter = new Utf8JsonWriter(
            new WriterAdapter(output),
            new JsonWriterOptions
            {
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                Indented = false
            }
        );

        jsonWriter.WriteStartObject();

        // Write in exact order required by client
        WriteProperty(
            jsonWriter,
            "timestamp",
            logEvent.Timestamp.UtcDateTime.ToString(
                "yyyy-MM-ddTHH:mm:ss.fff'Z'",
                System.Globalization.CultureInfo.InvariantCulture));
        WriteProperty(jsonWriter, "client-ip", GetPropertyValue(logEvent, "ClientIp"));
        WriteProperty(jsonWriter, "x-forwarded-ip", GetPropertyValue(logEvent, "XForwardedIp"));
        WriteProperty(jsonWriter, "level", GetLogLevel(logEvent.Level));
        WriteProperty(jsonWriter, "status", GetPropertyValue(logEvent, "StatusCode"));
        WriteProperty(jsonWriter, "latency", GetPropertyValue(logEvent, "Latency"));
        WriteProperty(jsonWriter, "endpoint", GetPropertyValue(logEvent, "Endpoint"));
        WriteProperty(jsonWriter, "message", FilterSensitiveData(logEvent.RenderMessage()));
        //WriteProperty(jsonWriter, "error", FilterSensitiveData(GetPropertyValue(logEvent, "ErrorMessage")));
        WriteProperty(jsonWriter, "error", logEvent.Exception?.GetType().Name);
        WriteProperty(jsonWriter, "trace-id", GetPropertyValue(logEvent, "TraceId"));
        WriteProperty(jsonWriter, "detail", _includeExceptionDetails
            ? FilterSensitiveData(logEvent.Exception?.Message)
            : "");


        jsonWriter.WriteEndObject();
        output.WriteLine();
    }

    private static void WriteProperty(Utf8JsonWriter writer, string propertyName, string? value)
    {
        writer.WriteString(propertyName, value ?? "");
    }

    private static string? GetPropertyValue(LogEvent logEvent, string propertyName)
    {
        if (logEvent.Properties.TryGetValue(propertyName, out var property))
        {
            return property.ToString().Trim('"');
        }
        return "";
    }

    private static string GetLogLevel(LogEventLevel level)
    {
        return level switch
        {
            LogEventLevel.Error => "Error",
            LogEventLevel.Warning => "Warning",
            LogEventLevel.Information => "Information",
            LogEventLevel.Debug => "Debug",
            LogEventLevel.Verbose => "Verbose",
            LogEventLevel.Fatal => "Fatal",
            _ => "Information"
        };
    }

    private static string FilterSensitiveData(string? message)
    {
        if (string.IsNullOrEmpty(message))
            return message ?? "";

        // Use fallback patterns if main patterns not initialized
        if (!_patternsInitialized)
        {
            InitializeFallbackPatterns();
            var filteredMessage = message;
            foreach (var compiledPattern in FallbackPatterns)
            {
                try
                {
                    filteredMessage = compiledPattern.Replace(filteredMessage, "[REDACTED]");
                }
                catch
                {
                    // Continue if regex fails
                    continue;
                }
            }
            return filteredMessage;
        }

        // Use compiled patterns for better performance
        var result = message;
        foreach (var compiledPattern in SensitivePatterns)
        {
            try
            {
                result = compiledPattern.Replace(result, "[REDACTED]");
            }
            catch
            {
                // Continue if regex fails
                continue;
            }
        }

        return result;
    }

    private static void InitializeFallbackPatterns()
    {
        if (_fallbackInitialized) return;

        lock (_lockObject)
        {
            if (_fallbackInitialized) return;

            var defaultPatterns = GetDefaultPatterns();
            foreach (var pattern in defaultPatterns)
            {
                try
                {
                    FallbackPatterns.Add(new Regex(pattern,
                        RegexOptions.Compiled | RegexOptions.IgnoreCase));
                }
                catch (ArgumentException)
                {
                    // Skip invalid regex patterns
                    continue;
                }
            }
            _fallbackInitialized = true;
        }
    }
}

internal class WriterAdapter : Stream
{
    private readonly TextWriter _textWriter;

    public WriterAdapter(TextWriter textWriter)
    {
        _textWriter = textWriter;
    }

    public override bool CanRead => false;
    public override bool CanSeek => false;
    public override bool CanWrite => true;
    public override long Length => throw new NotSupportedException();
    public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }

    public override void Flush() => _textWriter.Flush();
    public override int Read(byte[] buffer, int offset, int count) => throw new NotSupportedException();
    public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
    public override void SetLength(long value) => throw new NotSupportedException();

    public override void Write(byte[] buffer, int offset, int count)
    {
        var text = System.Text.Encoding.UTF8.GetString(buffer, offset, count);
        _textWriter.Write(text);
    }
}

