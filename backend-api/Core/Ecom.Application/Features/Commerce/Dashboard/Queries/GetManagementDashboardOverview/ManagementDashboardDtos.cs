using Ecom.Application.Features.Commerce.Orders;

namespace Ecom.Application.Features.Commerce.Dashboard.Queries.GetManagementDashboardOverview;

public sealed record ManagementDashboardOverviewDto(
    string CurrencyCode,
    string Timezone,
    string From,
    string To,
    DateTime RefreshedAtUtc,
    ManagementDashboardOrdersDto Orders,
    ManagementDashboardCatalogDto Catalog,
    ManagementDashboardProducersDto Producers,
    ManagementDashboardInventoryDto Inventory,
    ManagementDashboardUsersDto Users);

public sealed record ManagementDashboardOrdersDto(
    int CurrentPendingFulfillmentCount,
    ManagementOrderAnalyticsKpisDto Kpis,
    IReadOnlyList<ManagementOrderAnalyticsSeriesItemDto> Series,
    IReadOnlyList<ManagementOrderStatusBreakdownDto> StatusBreakdown,
    IReadOnlyList<ManagementPaymentMethodCashBreakdownDto> PaymentMethodBreakdown,
    IReadOnlyList<ManagementTopProductSalesDto> TopProducts);

public sealed record ManagementDashboardCatalogDto(
    int TotalProducts,
    int DraftProducts,
    int ReviewProducts,
    int PublishedProducts,
    int PausedProducts,
    int DiscontinuedProducts,
    int ActiveVariants,
    int SellableActiveVariants,
    int ProductsWithoutActiveVariant);

public sealed record ManagementDashboardProducersDto(
    int Total,
    int Published,
    int Verified,
    int Unverified);

public sealed record ManagementDashboardInventoryDto(
    int TrackedVariantCount,
    decimal StockedQuantity,
    decimal ReservedQuantity,
    decimal AvailableQuantity,
    decimal IncomingQuantity,
    int OutOfStockVariantCount);

public sealed record ManagementDashboardUsersDto(
    int TotalRegistered,
    int NewRegisteredInPeriod);

public sealed record ManagementDashboardSnapshotDto(
    int CurrentPendingFulfillmentCount,
    ManagementDashboardCatalogDto Catalog,
    ManagementDashboardProducersDto Producers,
    ManagementDashboardInventoryDto Inventory,
    ManagementDashboardUsersDto Users);
