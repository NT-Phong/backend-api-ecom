using Npgsql;

namespace Ecom.IntegrationTests.PostgreSql;

internal static class PostgreSqlTestDatabaseGuard
{
    internal const string ExternalConnectionStringEnvironmentVariable = "ECOM_TEST_POSTGRES";
    internal const string ResetOptInEnvironmentVariable = "ECOM_TEST_ALLOW_RESET";

    public static string ValidateExternalConnectionString(
        string connectionString,
        bool resetAllowed,
        string? runtimeConnectionString = null)
    {
        if (!resetAllowed)
        {
            throw new InvalidOperationException(
                $"Refusing to reset PostgreSQL because {ResetOptInEnvironmentVariable} is not true.");
        }

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                $"{ExternalConnectionStringEnvironmentVariable} must not be empty when it is configured.");
        }

        var builder = new NpgsqlConnectionStringBuilder(connectionString);
        var database = builder.Database;

        if (string.IsNullOrWhiteSpace(database)
            || (!database.EndsWith("_test", StringComparison.OrdinalIgnoreCase)
                && !database.EndsWith("_tests", StringComparison.OrdinalIgnoreCase)))
        {
            throw new InvalidOperationException(
                $"Refusing to use PostgreSQL database '{database}'. "
                + "Integration-test database names must end with '_test' or '_tests'.");
        }

        if (IsSameDatabase(builder, runtimeConnectionString))
        {
            throw new InvalidOperationException(
                "Refusing to use the same PostgreSQL database as the running application.");
        }

        if ((builder.Host ?? string.Empty).EndsWith(".postgres.database.azure.com", StringComparison.OrdinalIgnoreCase)
            && builder.SslMode == SslMode.Disable)
        {
            throw new InvalidOperationException("Azure PostgreSQL integration tests require SSL.");
        }

        builder.ApplicationName = "ecom-integration-tests";

        return builder.ConnectionString;
    }

    private static bool IsSameDatabase(NpgsqlConnectionStringBuilder candidate, string? runtimeConnectionString)
    {
        if (string.IsNullOrWhiteSpace(runtimeConnectionString)) return false;

        var runtime = new NpgsqlConnectionStringBuilder(runtimeConnectionString);
        return string.Equals(candidate.Host, runtime.Host, StringComparison.OrdinalIgnoreCase)
               && candidate.Port == runtime.Port
               && string.Equals(candidate.Database, runtime.Database, StringComparison.OrdinalIgnoreCase);
    }
}
