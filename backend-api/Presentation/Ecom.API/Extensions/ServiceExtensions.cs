using Ecom.API.Filters;
using System.Text.Json.Serialization;
using Ecom.API.Serialization;
using Microsoft.OpenApi.Models;

namespace Ecom.API.Extensions;

public static class ServiceExtensions
{
    public static IServiceCollection AddApiServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddControllers(options =>
        {
            options.Filters.Add<ModelValidationFilter>();
        })
        .AddJsonOptions(o =>
        {
            o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            // Ensure DateTime values are serialized/deserialized as UTC ISO-8601 (with Z)
            o.JsonSerializerOptions.Converters.Add(new DateTimeUtcConverter());
            o.JsonSerializerOptions.Converters.Add(new NullableDateTimeUtcConverter());
        });

        // Disable automatic model validation to use our custom filter
        services.Configure<ApiBehaviorOptions>(options =>
        {
            options.SuppressModelStateInvalidFilter = true;
        });

        services.AddApiVersioning(options =>
        {
            options.DefaultApiVersion = new ApiVersion(1, 0);
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.ReportApiVersions = true;
            options.ApiVersionReader = new UrlSegmentApiVersionReader();
        }).AddApiExplorer(options =>
        {
            options.GroupNameFormat = "'v'VVV";
            options.SubstituteApiVersionInUrl = true;
        });

        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();
        services.ConfigureOptions<ConfigureSwaggerOptions>();

        services.AddCors(options =>
        {
            options.AddPolicy("DefaultPolicy", builder =>
            {
                builder.WithOrigins("http://localhost:3000", "https://localhost:3000", "http://localhost:5173", "https://test-portal.Ecom.vn", "https://mebi-mebione-d-portal-as-1.azurewebsites.net", "https://Ecom-log-viewer.vercel.app")
                       .AllowAnyMethod()
                       .AllowAnyHeader()
                       .AllowCredentials();
            });
        });

        // Health checks
        var healthChecksBuilder = services.AddHealthChecks()
            .AddCheck("service",
                () => Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy("Application is running"),
                tags: new[] { "live" });

        // Guard the DB health check registration with a clear message if the connection string is missing
        var defaultConnection = configuration.GetConnectionString("DefaultConnection");
        if (!string.IsNullOrWhiteSpace(defaultConnection))
        {
            // Register DB health check only when connection string is provided
            healthChecksBuilder.AddNpgSql(defaultConnection, tags: new[] { "ready" });
        }
        else
        {
            // Fail fast with an actionable exception so the operator can fix configuration
            throw new InvalidOperationException(
                "Missing connection string 'DefaultConnection'. Please configure it in appsettings.json or via environment variable 'ConnectionStrings__DefaultConnection'.");
        }

        return services;
    }
    // Helper for Swagger
    public class SwaggerDefaultValues : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var apiDescription = context.ApiDescription;
            operation.Deprecated |= apiDescription.IsDeprecated();

            if (operation.Parameters == null) return;

            foreach (var parameter in operation.Parameters)
            {
                var description = apiDescription.ParameterDescriptions.First(p => p.Name == parameter.Name);
                if (parameter.Description == null)
                {
                    parameter.Description = description.ModelMetadata.Description;
                }
            }
        }
    }
}

