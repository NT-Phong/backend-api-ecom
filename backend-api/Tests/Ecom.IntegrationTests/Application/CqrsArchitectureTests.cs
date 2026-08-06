using Ecom.Application.Common.Interfaces;
using MediatR;
using System.Text.RegularExpressions;

namespace Ecom.IntegrationTests.Application;

public sealed class CqrsArchitectureTests
{
    private static readonly IReadOnlyDictionary<string, int> ExistingGroupedHandlerFiles =
        new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
        {
            ["AuthV2/PasswordManagement/PasswordCommands.cs"] = 4,
            ["Demo/QrLogin/DemoQrLogin.cs"] = 4
        };

    [Fact]
    public void Catalog_commands_are_transactional_requests()
    {
        var commandTypes = typeof(ITransactionalRequest).Assembly.GetTypes()
            .Where(type => type is { IsAbstract: false, IsInterface: false }
                           && type.Namespace?.StartsWith("Ecom.Application.Features.Catalog", StringComparison.Ordinal) == true
                           && type.Name.EndsWith("Command", StringComparison.Ordinal)
                           && type.GetInterfaces().Any(IsMediatRRequest))
            .ToArray();

        Assert.NotEmpty(commandTypes);
        var violations = commandTypes
            .Where(type => !typeof(ITransactionalRequest).IsAssignableFrom(type))
            .Select(type => type.FullName)
            .OrderBy(name => name)
            .ToArray();

        Assert.True(violations.Length == 0,
            $"Catalog commands must implement {nameof(ITransactionalRequest)}: {string.Join(", ", violations)}");
    }

    [Fact]
    public void Commerce_commands_are_transactional_requests()
    {
        var commandTypes = typeof(ITransactionalRequest).Assembly.GetTypes()
            .Where(type => type is { IsAbstract: false, IsInterface: false }
                           && type.Namespace?.StartsWith("Ecom.Application.Features.Commerce", StringComparison.Ordinal) == true
                           && type.Name.EndsWith("Command", StringComparison.Ordinal)
                           && type.GetInterfaces().Any(IsMediatRRequest))
            .ToArray();

        Assert.NotEmpty(commandTypes);
        var violations = commandTypes.Where(type => !typeof(ITransactionalRequest).IsAssignableFrom(type))
            .Select(type => type.FullName).OrderBy(name => name).ToArray();
        Assert.True(violations.Length == 0,
            $"Commerce commands must implement {nameof(ITransactionalRequest)}: {string.Join(", ", violations)}");
    }

    [Fact]
    public void Grouped_handler_file_debt_does_not_expand()
    {
        var featuresRoot = Path.Combine(FindRepositoryRoot(), "Core", "Ecom.Application", "Features");
        var current = Directory.EnumerateFiles(featuresRoot, "*.cs", SearchOption.AllDirectories)
            .Select(path => new
            {
                Path = Path.GetRelativePath(featuresRoot, path).Replace('\\', '/'),
                Count = Regex.Matches(File.ReadAllText(path), @"IRequestHandler\s*<").Count
            })
            .Where(item => item.Count > 1)
            .ToDictionary(item => item.Path, item => item.Count, StringComparer.OrdinalIgnoreCase);

        Assert.Equal(ExistingGroupedHandlerFiles.OrderBy(item => item.Key), current.OrderBy(item => item.Key));
    }

    private static bool IsMediatRRequest(Type interfaceType) =>
        interfaceType.IsGenericType && interfaceType.GetGenericTypeDefinition() == typeof(IRequest<>);

    private static string FindRepositoryRoot()
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory); directory is not null; directory = directory.Parent)
        {
            if (Directory.Exists(Path.Combine(directory.FullName, "Core", "Ecom.Application", "Features")))
                return directory.FullName;
        }

        throw new DirectoryNotFoundException("Could not locate the repository root for CQRS architecture verification.");
    }
}
