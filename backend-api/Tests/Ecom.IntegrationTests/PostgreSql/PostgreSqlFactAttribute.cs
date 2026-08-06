namespace Ecom.IntegrationTests.PostgreSql;

public sealed class PostgreSqlFactAttribute : FactAttribute
{
    public PostgreSqlFactAttribute()
    {
        var connectionString = Environment.GetEnvironmentVariable(
            PostgreSqlTestDatabaseGuard.ExternalConnectionStringEnvironmentVariable);
        var resetAllowed = bool.TryParse(
            Environment.GetEnvironmentVariable(PostgreSqlTestDatabaseGuard.ResetOptInEnvironmentVariable),
            out var allowReset) && allowReset;

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            Skip = $"Set {PostgreSqlTestDatabaseGuard.ExternalConnectionStringEnvironmentVariable} to a dedicated PostgreSQL database ending in _test or _tests.";
        }
        else if (!resetAllowed)
        {
            Skip = $"Set {PostgreSqlTestDatabaseGuard.ResetOptInEnvironmentVariable}=true to allow isolated test-schema reset.";
        }
    }
}
