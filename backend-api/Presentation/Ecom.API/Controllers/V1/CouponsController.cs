using Ecom.Application.Features.Commerce.Promotions.Queries.ValidateCoupon;
using Ecom.Application.Features.Commerce.Promotions.Queries.GetAvailableCoupons;
using Ecom.Application.Common.Configuration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Ecom.API.Controllers.V1;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/coupons")]
[AllowAnonymous]
public sealed class CouponsController : BaseController
{
    [HttpPost("validate")]
    [ValidateAntiForgeryToken]
    [EnableRateLimiting(CommerceRateLimitPolicyNames.CheckoutPreview)]
    public async Task<IActionResult> Validate(ValidateCouponQuery query, CancellationToken cancellationToken) =>
        HandleResult(await Mediator.Send(query, cancellationToken));

    [HttpGet("available")]
    [EnableRateLimiting(CommerceRateLimitPolicyNames.CheckoutPreview)]
    public async Task<IActionResult> GetAvailable(CancellationToken cancellationToken) =>
        HandleResult(await Mediator.Send(new GetAvailableCouponsQuery(), cancellationToken));
}
