using Ecom.Application.Common.Configuration;
using Ecom.Application.Features.Commerce.Promotions.Commands.ChangeCouponStatus;
using Ecom.Application.Features.Commerce.Promotions.Commands.CreateCoupon;
using Ecom.Application.Features.Commerce.Promotions.Commands.UpdateCoupon;
using Ecom.Application.Features.Commerce.Promotions.Queries.GetCouponRedemptions;
using Ecom.Application.Features.Commerce.Promotions.Queries.GetManagementCouponById;
using Ecom.Application.Features.Commerce.Promotions.Queries.GetManagementCoupons;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Ecom.API.Controllers.V1;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/management/coupons")]
[Authorize]
public sealed class ManagementCouponsController : BaseController
{
    [HttpGet]
    [Authorize(Policy = Permissions.Promotions.Read)]
    public async Task<IActionResult> GetCoupons([FromQuery] GetManagementCouponsQuery query, CancellationToken ct) =>
        HandleResult(await Mediator.Send(query, ct));

    [HttpGet("{id:guid}")]
    [Authorize(Policy = Permissions.Promotions.Read)]
    public async Task<IActionResult> GetCoupon(Guid id, CancellationToken ct) =>
        HandleResult(await Mediator.Send(new GetManagementCouponByIdQuery(id), ct));

    [HttpGet("{id:guid}/redemptions")]
    [Authorize(Policy = Permissions.Promotions.Read)]
    public async Task<IActionResult> GetRedemptions(Guid id, [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default) =>
        HandleResult(await Mediator.Send(new GetCouponRedemptionsQuery { CouponId = id, Page = page, PageSize = pageSize }, ct));

    [HttpPost]
    [Authorize(Policy = Permissions.Promotions.Manage)]
    [ValidateAntiForgeryToken]
    [EnableRateLimiting(CommerceRateLimitPolicyNames.ManagementMutation)]
    public async Task<IActionResult> Create(CreateCouponCommand command, CancellationToken ct) =>
        HandleResult(await Mediator.Send(command, ct));

    [HttpPut("{id:guid}")]
    [Authorize(Policy = Permissions.Promotions.Manage)]
    [ValidateAntiForgeryToken]
    [EnableRateLimiting(CommerceRateLimitPolicyNames.ManagementMutation)]
    public async Task<IActionResult> Update(Guid id, UpdateCouponCommand command, CancellationToken ct) =>
        HandleResult(await Mediator.Send(command with { Id = id }, ct));

    [HttpPatch("{id:guid}/status")]
    [Authorize(Policy = Permissions.Promotions.Manage)]
    [ValidateAntiForgeryToken]
    [EnableRateLimiting(CommerceRateLimitPolicyNames.ManagementMutation)]
    public async Task<IActionResult> ChangeStatus(Guid id, ChangeCouponStatusCommand command, CancellationToken ct) =>
        HandleResult(await Mediator.Send(command with { Id = id }, ct));
}
