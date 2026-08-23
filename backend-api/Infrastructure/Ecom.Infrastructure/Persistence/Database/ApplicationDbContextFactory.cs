using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Ecom.Application.Common.Interfaces;
using Ecom.Domain.Interfaces;
using MediatR;

namespace Ecom.Infrastructure.Persistence.Database;

/// <summary>
/// Design-time factory for ApplicationDbContext.
/// Used by EF Core tools (migrations, etc.) when the application is not running.
/// </summary>
public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        // Build configuration - try multiple paths to find appsettings
        var basePath = Directory.GetCurrentDirectory();
        var apiProjectPath = Path.Combine(basePath, "..", "..", "Presentation", "Ecom.API");
        
        // Determine which path to use
        string configPath = basePath;
        if (Directory.Exists(apiProjectPath) && File.Exists(Path.Combine(apiProjectPath, "appsettings.json")))
        {
            configPath = apiProjectPath;
        }

        var configuration = new ConfigurationBuilder()
            .SetBasePath(configPath)
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddJsonFile("appsettings.Local.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        // Get connection string
        var connectionString = configuration.GetConnectionString("DefaultConnection") 
                               ?? configuration.GetConnectionString("WriteConnection")
                               ?? throw new InvalidOperationException("No connection string found in configuration.");

        // Build DbContext options
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        optionsBuilder.UseNpgsql(connectionString, sqlOptions =>
        {
            sqlOptions.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName);
            sqlOptions.CommandTimeout(30);
        });

        // Create mock services for design-time
        var currentUser = new DesignTimeCurrentUser();
        var dateTimeService = new DesignTimeDateTimeService();
        var connectionService = new DesignTimeConnectionService(connectionString);

        return new ApplicationDbContext(
            optionsBuilder.Options,
            currentUser,
            dateTimeService,
            connectionService);
    }
}

// Design-time stub implementations
internal class DesignTimeCurrentUser : ICurrentUser
{
    private readonly Guid _userId = Guid.Empty;
    public string? UserId => "design-time-user";
    public string? UserIdString { get; }
    public string? PhoneNumber { get; }
    public string? UserName { get; }

    Guid ICurrentUser.UserId => _userId;

    public string? Email { get; }
    public bool IsAuthenticated => false;
    public string? Role { get; }
    public IEnumerable<string> Roles => [];
    public IEnumerable<string> Policies { get; } = [];
    public Guid SessionId => Guid.Empty;
    public string? SecurityStamp => null;

    public bool HasRole(string role)
    {
        throw new NotImplementedException();
    }

    public bool HasPolicy(string policy)
    {
        throw new NotImplementedException();
    }
}

internal class DesignTimeDateTimeService : IDateTimeService
{
    DateTimeOffset IDateTimeService.UtcNow => UtcNow;

    DateTimeOffset IDateTimeService.Now => Now;

    public DateOnly Today { get; }
    public TimeOnly TimeNow { get; }
    public DateTime Now => DateTime.UtcNow;
    public DateTime UtcNow => DateTime.UtcNow;
}

internal class DesignTimeMediator : IMediator
{
    public IAsyncEnumerable<TResponse> CreateStream<TResponse>(IStreamRequest<TResponse> request, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException("Design-time mediator does not support CreateStream");
    }

    public IAsyncEnumerable<object?> CreateStream(object request, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException("Design-time mediator does not support CreateStream");
    }

    public Task Publish(object notification, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public Task Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default) where TNotification : INotification
    {
        return Task.CompletedTask;
    }

    public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException("Design-time mediator does not support Send");
    }

    public Task Send<TRequest>(TRequest request, CancellationToken cancellationToken = new CancellationToken()) where TRequest : IRequest
    {
        throw new NotImplementedException();
    }

    public Task<object?> Send(object request, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException("Design-time mediator does not support Send");
    }
}

internal class DesignTimeConnectionService : IConnectionService
{
    private readonly string _connectionString;

    public DesignTimeConnectionService(string connectionString)
    {
        _connectionString = connectionString;
    }

    public string GetReadConnectionString() => _connectionString;
    public string GetWriteConnectionString() => _connectionString;
}
