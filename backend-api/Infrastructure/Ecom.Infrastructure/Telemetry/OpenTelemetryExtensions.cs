using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using System;

namespace Ecom.Infrastructure.Telemetry;

public static class OpenTelemetryExtensions
{
    public static IServiceCollection AddOpenTelemetryServices(this IServiceCollection services, IConfiguration configuration)
    {
        var telemetrySettings = configuration.GetSection("OpenTelemetry").Get<OpenTelemetrySettings>();
        if (telemetrySettings == null || !telemetrySettings.Enabled) return services;

        services.AddOpenTelemetry()
            .ConfigureResource(resource => resource.AddService(
                serviceName: telemetrySettings.ServiceName,
                serviceVersion: telemetrySettings.ServiceVersion,
                serviceInstanceId: Environment.MachineName))
            .WithTracing(tracing =>
            {
                tracing
                    .AddAspNetCoreInstrumentation(options =>
                    {
                        options.RecordException = true;
                        options.Filter = (httpContext) =>
                        {
                            var path = httpContext.Request.Path.Value ?? string.Empty;
                            if (string.IsNullOrEmpty(path))
                            {
                                return true;
                            }

                            if (path.Contains("health", StringComparison.OrdinalIgnoreCase) ||
                                path.Contains("livez", StringComparison.OrdinalIgnoreCase) ||
                                path.Contains("readyz", StringComparison.OrdinalIgnoreCase) ||
                                path.Contains("metrics", StringComparison.OrdinalIgnoreCase))
                            {
                                return false;
                            }

                            return true;
                        };
                    })
                    .AddHttpClientInstrumentation(options =>
                    {
                        options.RecordException = true;
                    })
                    .AddEntityFrameworkCoreInstrumentation()
                    .AddSource("Ecom.*")
                    .AddSource("MediatR");

                if (!string.IsNullOrEmpty(telemetrySettings.OtlpEndpoint))
                {
                    tracing.AddOtlpExporter(options =>
                    {
                        options.Endpoint = new Uri(telemetrySettings.OtlpEndpoint);
                        options.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.Grpc;
                        if (!string.IsNullOrEmpty(telemetrySettings.OtlpHeaders))
                        {
                            options.Headers = telemetrySettings.OtlpHeaders;
                        }
                    });
                }
                if (telemetrySettings.EnableConsoleExporter)
                {
                    tracing.AddConsoleExporter();
                }
            })
            .WithMetrics(metrics =>
            {
                metrics
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddRuntimeInstrumentation()
                    .AddMeter("Ecom.*")
                    .AddMeter("Microsoft.AspNetCore.Hosting")
                    .AddMeter("Microsoft.AspNetCore.Server.Kestrel")
                    .AddMeter("System.Net.Http")
                    .AddMeter("Microsoft.EntityFrameworkCore")
                    .AddView("http.server.request.duration",
                        new ExplicitBucketHistogramConfiguration
                        {
                            Boundaries = [0, 0.005, 0.01, 0.025, 0.05, 0.075, 0.1, 0.25, 0.5, 0.75, 1, 2.5, 5, 7.5, 10]
                        });

                if (!string.IsNullOrEmpty(telemetrySettings.OtlpEndpoint))
                {
                    metrics.AddOtlpExporter(options =>
                    {
                        options.Endpoint = new Uri(telemetrySettings.OtlpEndpoint);
                        options.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.Grpc;
                        if (!string.IsNullOrEmpty(telemetrySettings.OtlpHeaders))
                        {
                            options.Headers = telemetrySettings.OtlpHeaders;
                        }
                    });
                }
                if (telemetrySettings.EnableConsoleExporter)
                {
                    metrics.AddConsoleExporter();
                }

                metrics.AddPrometheusExporter();
            });

        return services;
    }
}

public class OpenTelemetrySettings
{
    public bool Enabled { get; set; } = true;
    public string ServiceName { get; set; } = "Ecom";
    public string ServiceVersion { get; set; } = "1.0.0";
    public string? OtlpEndpoint { get; set; }
    public string? OtlpHeaders { get; set; }
    public bool EnableConsoleExporter { get; set; }
}
