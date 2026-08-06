using Ecom.Application.Common.Interfaces;
using Ecom.Domain.Interfaces.Services;
using Ecom.Infrastructure.Persistence.Database;
using Ecom.Infrastructure.Persistence.Database.Interceptors;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Ecom.IntegrationTests.PostgreSql;

public sealed class PostgreSqlFixture : IAsyncLifetime
{
    private readonly string _schemaName = $"ecom_it_{Guid.NewGuid():N}";
    private readonly TestCurrentUser _currentUser = new();
    private readonly TestDateTimeService _dateTime = new();
    private string? _externalConnectionString;

    public PostgreSqlFixture()
    {
        var externalConnectionString = Environment.GetEnvironmentVariable(
            PostgreSqlTestDatabaseGuard.ExternalConnectionStringEnvironmentVariable);

        if (string.IsNullOrWhiteSpace(externalConnectionString)) return;

        var resetAllowed = bool.TryParse(
            Environment.GetEnvironmentVariable(PostgreSqlTestDatabaseGuard.ResetOptInEnvironmentVariable),
            out var allowReset) && allowReset;
        var runtimeConnectionString = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection");
        _externalConnectionString = PostgreSqlTestDatabaseGuard.ValidateExternalConnectionString(
            externalConnectionString,
            resetAllowed,
            runtimeConnectionString);
    }

    public string ConnectionString { get; private set; } = string.Empty;

    public async Task InitializeAsync()
    {
        if (string.IsNullOrWhiteSpace(_externalConnectionString)) return;

        await RecreateSchemaAsync();

        await using var context = CreateDbContext();
        await context.Database.MigrateAsync();
    }

    public async Task ResetDatabaseAsync(CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(ConnectionString))
        {
            throw new InvalidOperationException("The PostgreSQL fixture has not been initialized.");
        }

        await using var connection = new NpgsqlConnection(ConnectionString);
        await connection.OpenAsync(cancellationToken);

        var tableNames = new List<string>();
        await using (var listTables = connection.CreateCommand())
        {
            listTables.CommandText =
                "SELECT quote_ident(schemaname) || '.' || quote_ident(tablename) "
                + "FROM pg_tables WHERE schemaname = @schema "
                + "AND tablename <> '__EFMigrationsHistory' ORDER BY tablename;";
            listTables.Parameters.AddWithValue("schema", _schemaName);
            await using var reader = await listTables.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                tableNames.Add(reader.GetString(0));
            }
        }

        if (tableNames.Count == 0) return;

        await using var truncate = connection.CreateCommand();
        truncate.CommandText = $"TRUNCATE TABLE {string.Join(", ", tableNames)} RESTART IDENTITY CASCADE;";
        await truncate.ExecuteNonQueryAsync(cancellationToken);
    }

    private async Task RecreateSchemaAsync(CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_externalConnectionString))
        {
            throw new InvalidOperationException("The PostgreSQL fixture has not been initialized.");
        }

        var schemaIdentifier = new NpgsqlCommandBuilder().QuoteIdentifier(_schemaName);
        await using var connection = new NpgsqlConnection(_externalConnectionString);
        await connection.OpenAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = $"DROP SCHEMA IF EXISTS {schemaIdentifier} CASCADE; CREATE SCHEMA {schemaIdentifier};";
        await command.ExecuteNonQueryAsync(cancellationToken);

        var scopedBuilder = new NpgsqlConnectionStringBuilder(_externalConnectionString)
        {
            SearchPath = $"{_schemaName},public",
            ApplicationName = "ecom-integration-tests"
        };
        ConnectionString = scopedBuilder.ConnectionString;
    }

    public ApplicationDbContext CreateDbContext()
    {
        if (string.IsNullOrWhiteSpace(ConnectionString))
        {
            throw new InvalidOperationException("The PostgreSQL fixture has not been initialized.");
        }

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql(ConnectionString)
            .AddInterceptors(new AuditableEntityInterceptor(_currentUser, _dateTime))
            .Options;

        return new ApplicationDbContext(
            options,
            _currentUser,
            _dateTime,
            new TestConnectionService(ConnectionString));
    }

    internal string SchemaName => _schemaName;

    public async Task DisposeAsync()
    {
        if (!string.IsNullOrWhiteSpace(_externalConnectionString))
        {
            var schemaIdentifier = new NpgsqlCommandBuilder().QuoteIdentifier(_schemaName);
            await using var connection = new NpgsqlConnection(_externalConnectionString);
            await connection.OpenAsync();
            await using var command = connection.CreateCommand();
            command.CommandText = $"DROP SCHEMA IF EXISTS {schemaIdentifier} CASCADE;";
            await command.ExecuteNonQueryAsync();
        }

    }

    private sealed class TestConnectionService(string connectionString) : IConnectionService
    {
        public string GetReadConnectionString() => connectionString;
        public string GetWriteConnectionString() => connectionString;
    }

    private sealed class TestDateTimeService : IDateTimeService
    {
        public DateTimeOffset UtcNow => DateTimeOffset.UnixEpoch;
        public DateTimeOffset Now => DateTimeOffset.UnixEpoch;
        public DateOnly Today => DateOnly.FromDateTime(DateTime.UnixEpoch);
        public TimeOnly TimeNow => TimeOnly.MinValue;
    }

    private sealed class TestCurrentUser : ICurrentUser
    {
        public Guid UserId => Guid.Empty;
        public string? UserIdString => null;
        public string? PhoneNumber => null;
        public string? Email => null;
        public bool IsAuthenticated => false;
        public string? Role => null;
        public IEnumerable<string> Roles => [];
        public IEnumerable<string> Policies => [];
        public Guid SessionId => Guid.Empty;
        public string? SecurityStamp => null;
        public bool HasRole(string role) => false;
        public bool HasPolicy(string policy) => false;
    }
}
