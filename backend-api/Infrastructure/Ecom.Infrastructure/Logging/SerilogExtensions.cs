using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http;
using Serilog;
using Serilog.Events;
using Serilog.Configuration;

namespace Ecom.Infrastructure.Logging;

public static class SerilogExtensions
{
    public static IServiceCollection AddSerilogLogging(this IServiceCollection services, IConfiguration configuration)
    {
        // Register HttpContextAccessor if not already registered
        services.AddHttpContextAccessor();

        // Initialize CustomJsonFormatter patterns
        CustomJsonFormatter.InitializePatterns(configuration);


        // Console JSON format with all required fields: timestamp, client-ip, x-forwarded-ip, level, status, latency, endpoint, message, error, trace-id
        var loggerConfig = new LoggerConfiguration()
            .ReadFrom.Configuration(configuration)
            .Enrich.FromLogContext()
            .Enrich.With<SerilogEnricher>()
            .Enrich.With<HttpContextEnricher>()
            .Enrich.WithProperty("Application", "Ecom");

        // Console JSON output only - will be collected by external systems (Grafana Loki)
        loggerConfig.WriteTo.Console(new CustomJsonFormatter());
        
        // Optional file logging for local development
        var serilogSection = configuration.GetSection("Serilog");
        var enableFileLogging = serilogSection.GetValue<bool>("EnableFileLogging", false);
        if (enableFileLogging)
        {
            var filePath = serilogSection.GetValue<string>("FilePath") ?? "logs/Ecom-api-.log";
            loggerConfig.WriteTo.File(
                formatter: new CustomJsonFormatter(),
                path: filePath,
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 7,
                shared: true);
        }

        Log.Logger = loggerConfig.CreateLogger();

        services.AddLogging(loggingBuilder =>
        {
            loggingBuilder.ClearProviders();
            loggingBuilder.AddSerilog(dispose: true);
        });

        return services;
    }

    public static void InitializeHttpContextEnricher(IServiceProvider serviceProvider)
    {
        var httpContextAccessor = serviceProvider.GetRequiredService<IHttpContextAccessor>();
        HttpContextEnricher.SetHttpContextAccessor(httpContextAccessor);
    }
} 

