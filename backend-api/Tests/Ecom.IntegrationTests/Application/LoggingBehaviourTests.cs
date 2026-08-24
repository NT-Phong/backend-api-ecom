using Ecom.Application.Common.Behaviours;
using Ecom.Application.Common.Interfaces;
using Ecom.Application.Common.Models;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Ecom.IntegrationTests.Application;

public sealed class LoggingBehaviourTests
{
    [Fact]
    public async Task Anonymous_request_logs_actor_state_without_empty_guid()
    {
        var logger = new RecordingLogger<LoggingBehaviour<TestRequest, TResult>>();
        var behavior = new LoggingBehaviour<TestRequest, TResult>(logger, new TestCurrentUser(Guid.Empty, false));

        var result = await behavior.Handle(
            new TestRequest(),
            _ => Task.FromResult(TResult.Success()),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        var renderedLogs = string.Join(Environment.NewLine, logger.Messages);
        Assert.Contains("ActorKind: Anonymous", renderedLogs, StringComparison.Ordinal);
        Assert.DoesNotContain(Guid.Empty.ToString(), renderedLogs, StringComparison.Ordinal);
    }

    private sealed record TestRequest : IRequest<TResult>;

    private sealed class TestCurrentUser(Guid userId, bool isAuthenticated) : ICurrentUser
    {
        public Guid UserId => userId;
        public string? UserIdString => userId == Guid.Empty ? null : userId.ToString();
        public string? PhoneNumber => null;
        public string? Email => null;
        public bool IsAuthenticated => isAuthenticated;
        public string? Role => null;
        public IEnumerable<string> Roles => [];
        public IEnumerable<string> Policies => [];
        public Guid SessionId => Guid.Empty;
        public string? SecurityStamp => null;
        public bool HasRole(string role) => false;
        public bool HasPolicy(string policy) => false;
    }

    private sealed class RecordingLogger<T> : ILogger<T>
    {
        public List<string> Messages { get; } = [];

        public IDisposable BeginScope<TState>(TState state) where TState : notnull => NoopDisposable.Instance;
        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            Messages.Add(formatter(state, exception));
        }

        private sealed class NoopDisposable : IDisposable
        {
            public static readonly NoopDisposable Instance = new();
            public void Dispose() { }
        }
    }
}
