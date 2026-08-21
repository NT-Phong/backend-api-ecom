using Ecom.Application.Common.Interfaces;
using Ecom.Application.Features.Commerce.Dashboard;
using Ecom.Application.Features.Commerce.Dashboard.Queries.GetManagementDashboardOverview;
using Ecom.Application.Features.Commerce.Orders;
using Ecom.Application.Features.Commerce.Orders.Queries.GetManagementOrderAnalyticsOverview;
using Ecom.Domain.Constants;

namespace Ecom.IntegrationTests.Application;

public sealed class ManagementDashboardOverviewQueryValidatorTests
{
    [Fact]
    public void Accepts_default_and_maximum_supported_range()
    {
        var validator = new GetManagementDashboardOverviewQueryValidator();

        Assert.True(validator.Validate(new GetManagementDashboardOverviewQuery()).IsValid);
        Assert.True(validator.Validate(new GetManagementDashboardOverviewQuery
        {
            From = new DateOnly(2026, 1, 1),
            To = new DateOnly(2026, 12, 31),
            Granularity = ManagementOrderAnalyticsGranularity.Month,
            TopLimit = 50
        }).IsValid);
    }

    [Fact]
    public void Default_range_covers_thirty_vietnam_calendar_days()
    {
        var accepted = ManagementAnalyticsRange.TryCreate(null, null, ManagementOrderAnalyticsGranularity.Day, 10,
            out var range, out var errors);

        Assert.True(accepted);
        Assert.Empty(errors);
        Assert.Equal(29, range.To.DayNumber - range.From.DayNumber);
        Assert.Equal(range.From.AddDays(30), DateOnly.FromDateTime(range.ToExclusiveUtc.AddHours(7)));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(51)]
    public void Rejects_top_limit_outside_supported_range(int topLimit)
    {
        var result = new GetManagementDashboardOverviewQueryValidator()
            .Validate(new GetManagementDashboardOverviewQuery { TopLimit = topLimit });

        Assert.False(result.IsValid);
    }

    [Fact]
    public void Rejects_reversed_or_overwide_date_ranges()
    {
        var validator = new GetManagementDashboardOverviewQueryValidator();

        Assert.False(validator.Validate(new GetManagementDashboardOverviewQuery
        {
            From = new DateOnly(2026, 8, 2),
            To = new DateOnly(2026, 8, 1)
        }).IsValid);
        Assert.False(validator.Validate(new GetManagementDashboardOverviewQuery
        {
            From = new DateOnly(2025, 1, 1),
            To = new DateOnly(2026, 1, 2)
        }).IsValid);
    }

    [Fact]
    public async Task Order_analytics_handler_returns_validation_failure_when_called_directly()
    {
        var handler = new GetManagementOrderAnalyticsOverviewQueryHandler(
            new ThrowingReadStore(), new TestCurrentUser(Permissions.Orders.Read));

        var result = await handler.Handle(new GetManagementOrderAnalyticsOverviewQuery
        {
            From = new DateOnly(2026, 8, 2),
            To = new DateOnly(2026, 8, 1)
        }, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCodes.BAD_REQUEST, result.ErrorCode);
        Assert.Contains("from", result.ValidationErrors!.Keys);
    }

    [Fact]
    public async Task Dashboard_handler_returns_validation_failure_when_called_directly()
    {
        var handler = new GetManagementDashboardOverviewQueryHandler(new ThrowingReadStore(), new TestCurrentUser(
            Permissions.Orders.Read,
            Permissions.CatalogProducts.Read,
            Permissions.Inventory.Read,
            Permissions.Producers.Read,
            Permissions.User.Read));

        var result = await handler.Handle(new GetManagementDashboardOverviewQuery { TopLimit = 51 }, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCodes.BAD_REQUEST, result.ErrorCode);
        Assert.Contains("topLimit", result.ValidationErrors!.Keys);
    }

    private sealed class ThrowingReadStore : IManagementDashboardReadStore
    {
        public Task<ManagementOrderAnalyticsOverviewDto> GetOrderAnalyticsAsync(
            ManagementAnalyticsRange range, ManagementOrderAnalyticsGranularity granularity, int topLimit,
            CancellationToken cancellationToken = default) => throw new InvalidOperationException("The read store must not run for invalid input.");

        public Task<ManagementDashboardSnapshotDto> GetSnapshotAsync(
            ManagementAnalyticsRange range, CancellationToken cancellationToken = default) => throw new InvalidOperationException();
    }

    private sealed class TestCurrentUser(params string[] policies) : ICurrentUser
    {
        public Guid UserId => Guid.Empty;
        public string? UserIdString => null;
        public string? PhoneNumber => null;
        public string? Email => null;
        public bool IsAuthenticated => true;
        public string? Role => null;
        public IEnumerable<string> Roles => [];
        public IEnumerable<string> Policies => policies;
        public Guid SessionId => Guid.Empty;
        public string? SecurityStamp => null;
        public bool HasRole(string role) => false;
        public bool HasPolicy(string policy) => policies.Contains(policy, StringComparer.Ordinal);
    }
}
