using Ecom.Application.Features.Commerce.Dashboard;

namespace Ecom.Application.Features.Commerce.Orders.Queries.GetManagementOrderAnalyticsOverview;

public enum ManagementOrderAnalyticsGranularity { Day, Week, Month }

public sealed record GetManagementOrderAnalyticsOverviewQuery : IRequest<TResult<ManagementOrderAnalyticsOverviewDto>>
{
    public DateOnly? From { get; init; }
    public DateOnly? To { get; init; }
    public ManagementOrderAnalyticsGranularity Granularity { get; init; } = ManagementOrderAnalyticsGranularity.Day;
    public int TopLimit { get; init; } = 10;
}

public sealed class GetManagementOrderAnalyticsOverviewQueryValidator : AbstractValidator<GetManagementOrderAnalyticsOverviewQuery>
{
    public GetManagementOrderAnalyticsOverviewQueryValidator()
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

public sealed class GetManagementOrderAnalyticsOverviewQueryHandler(IManagementDashboardReadStore readStore, ICurrentUser currentUser)
    : IRequestHandler<GetManagementOrderAnalyticsOverviewQuery, TResult<ManagementOrderAnalyticsOverviewDto>>
{
    public async Task<TResult<ManagementOrderAnalyticsOverviewDto>> Handle(
        GetManagementOrderAnalyticsOverviewQuery request, CancellationToken cancellationToken)
    {
        if (!currentUser.IsAuthenticated)
            return TResult<ManagementOrderAnalyticsOverviewDto>.Failure(MessageKey.Unauthorized, ErrorCodes.UNAUTHORIZED);
        if (!currentUser.HasPolicy(Permissions.Orders.Read))
            return TResult<ManagementOrderAnalyticsOverviewDto>.Failure(MessageKey.Forbidden, ErrorCodes.FORBIDDEN);

        if (!ManagementAnalyticsRange.TryCreate(request.From, request.To, request.Granularity, request.TopLimit, out var range, out var errors))
            return TResult<ManagementOrderAnalyticsOverviewDto>.ValidationFailure(errors);

        return TResult<ManagementOrderAnalyticsOverviewDto>.Success(await readStore.GetOrderAnalyticsAsync(
            range, request.Granularity, request.TopLimit, cancellationToken));
    }
}
