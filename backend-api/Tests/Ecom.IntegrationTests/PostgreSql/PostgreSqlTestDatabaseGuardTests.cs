namespace Ecom.IntegrationTests.PostgreSql;

public sealed class PostgreSqlTestDatabaseGuardTests
{
    [Theory]
    [InlineData("Host=localhost;Database=ecom_test;Username=test;Password=test")]
    [InlineData("Host=localhost;Database=ecom_integration_tests;Username=test;Password=test")]
    public void Accepts_explicitly_test_scoped_database_names(string connectionString)
    {
        var validated = PostgreSqlTestDatabaseGuard.ValidateExternalConnectionString(
            connectionString,
            resetAllowed: true);

        Assert.Contains("Database=ecom_", validated, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData("Host=localhost;Database=ecom_dev;Username=test;Password=test")]
    [InlineData("Host=localhost;Database=ecom_staging;Username=test;Password=test")]
    [InlineData("Host=localhost;Database=ecom_production;Username=test;Password=test")]
    public void Rejects_non_test_database_names(string connectionString)
    {
        var exception = Assert.Throws<InvalidOperationException>(() =>
            PostgreSqlTestDatabaseGuard.ValidateExternalConnectionString(connectionString, resetAllowed: true));

        Assert.Contains("Refusing to use PostgreSQL database", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Rejects_external_database_without_explicit_reset_opt_in()
    {
        var exception = Assert.Throws<InvalidOperationException>(() =>
            PostgreSqlTestDatabaseGuard.ValidateExternalConnectionString(
                "Host=localhost;Database=ecom_integration_tests;Username=test;Password=test",
                resetAllowed: false));

        Assert.Contains(PostgreSqlTestDatabaseGuard.ResetOptInEnvironmentVariable, exception.Message,
            StringComparison.Ordinal);
    }

    [Fact]
    public void Rejects_the_runtime_application_database()
    {
        const string connectionString =
            "Host=localhost;Database=ecom_integration_tests;Username=test;Password=test";

        var exception = Assert.Throws<InvalidOperationException>(() =>
            PostgreSqlTestDatabaseGuard.ValidateExternalConnectionString(
                connectionString,
                resetAllowed: true,
                runtimeConnectionString: connectionString));

        Assert.Contains("same PostgreSQL database", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Rejects_disabled_ssl_for_azure_postgresql()
    {
        var exception = Assert.Throws<InvalidOperationException>(() =>
            PostgreSqlTestDatabaseGuard.ValidateExternalConnectionString(
                "Host=ecom.postgres.database.azure.com;Database=ecom_tests;Username=test;Password=test;SSL Mode=Disable",
                resetAllowed: true));

        Assert.Contains("require SSL", exception.Message, StringComparison.Ordinal);
    }
}
