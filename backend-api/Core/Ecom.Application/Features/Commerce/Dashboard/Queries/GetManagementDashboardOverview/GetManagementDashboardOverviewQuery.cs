using Ecom.Application.Features.Commerce.Orders.Queries.GetManagementOrderAnalyticsOverview;

namespace Ecom.Application.Features.Commerce.Dashboard.Queries.GetManagementDashboardOverview;

public sealed record GetManagementDashboardOverviewQuery : IRequest<TResult<ManagementDashboardOverviewDto>>
{
    public DateOnly? From { get; init; }
    public DateOnly? To { get; init; }
    public ManagementOrderAnalyticsGranularity Granularity { get; init; } = ManagementOrderAnalyticsGranularity.Day;
    public int TopLimit { get; init; } = 10;
}

public sealed class GetManagementDashboardOverviewQueryValidator : AbstractValidator<GetManagementDashboardOverviewQuery>
{
    public GetManagementDashboardOverviewQueryValidator()
    {
        RuleFor(x => x).Custom((query, context) =>
        {
            if (ManagementAnalyticsRange.TryCreate(query.From, query.To, query.Granularity, query.TopLimit, out _, out var errors))
                return;
            foreach (var error in errors)
                foreach (var message in error.Value)
                    context.AddFailure(error.Key, message);
        });
    }
}

public sealed class GetManagementDashboardOverviewQueryHandler(IManagementDashboardReadStore readStore, ICurrentUser currentUser)
    : IRequestHandler<GetManagementDashboardOverviewQuery, TResult<ManagementDashboardOverviewDto>>
{
    private static readonly string[] RequiredPermissions =
    [
        Permissions.Orders.Read,
        Permissions.CatalogProducts.Read,
        Permissions.Inventory.Read,
        Permissions.Producers.Read,
        Permissions.User.Read
    ];

    public async Task<TResult<ManagementDashboardOverviewDto>> Handle(
        GetManagementDashboardOverviewQuery request,
        CancellationToken cancellationToken)
    {
        if (!currentUser.IsAuthenticated)
            return TResult<ManagementDashboardOverviewDto>.Failure(MessageKey.Unauthorized, ErrorCodes.UNAUTHORIZED);
        if (RequiredPermissions.Any(permission => !currentUser.HasPolicy(permission)))
            return TResult<ManagementDashboardOverviewDto>.Failure(MessageKey.Forbidden, ErrorCodes.FORBIDDEN);
        if (!ManagementAnalyticsRange.TryCreate(request.From, request.To, request.Granularity, request.TopLimit, out var range, out var errors))
            return TResult<ManagementDashboardOverviewDto>.ValidationFailure(errors);

        var analytics = await readStore.GetOrderAnalyticsAsync(range, request.Granularity, request.TopLimit, cancellationToken);
        var snapshot = await readStore.GetSnapshotAsync(range, cancellationToken);
        return TResult<ManagementDashboardOverviewDto>.Success(new ManagementDashboardOverviewDto(
            analytics.CurrencyCode,
            ManagementAnalyticsRange.VietnamTimeZoneId,
            analytics.From,
            analytics.To,
            DateTime.UtcNow,
            new ManagementDashboardOrdersDto(
                snapshot.CurrentPendingFulfillmentCount,
                analytics.Kpis,
                analytics.Series,
                analytics.StatusBreakdown,
                analytics.PaymentMethodBreakdown,
                analytics.TopProducts),
            snapshot.Catalog,
            snapshot.Producers,
            snapshot.Inventory,
            snapshot.Users));
    }
}
