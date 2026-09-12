namespace Ecom.Application.Features.Catalog.Common;

/// <summary>
/// Public, non-quantitative selling signal. It is deliberately not a checkout guarantee.
/// </summary>
public enum CatalogAvailabilityStatus
{
    Available,
    OutOfStock,
    Unavailable
}
