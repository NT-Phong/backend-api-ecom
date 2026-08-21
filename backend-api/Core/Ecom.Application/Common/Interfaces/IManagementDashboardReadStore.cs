using Ecom.Application.Features.Commerce.Dashboard;
using Ecom.Application.Features.Commerce.Dashboard.Queries.GetManagementDashboardOverview;
using Ecom.Application.Features.Commerce.Orders;
using Ecom.Application.Features.Commerce.Orders.Queries.GetManagementOrderAnalyticsOverview;

namespace Ecom.Application.Common.Interfaces;

public interface IManagementDashboardReadStore
{
    Task<ManagementOrderAnalyticsOverviewDto> GetOrderAnalyticsAsync(
        ManagementAnalyticsRange range,
        ManagementOrderAnalyticsGranularity granularity,
        int topLimit,
        CancellationToken cancellationToken = default);

    Task<ManagementDashboardSnapshotDto> GetSnapshotAsync(
        ManagementAnalyticsRange range,
        CancellationToken cancellationToken = default);
}
