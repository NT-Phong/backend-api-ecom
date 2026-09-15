using Ecom.Application.Common.Configuration;
using Ecom.Application.Features.Commerce.Promotions.Commands.ChangePromotionStatus;
using Ecom.Application.Features.Commerce.Promotions.Commands.CreatePromotion;
using Ecom.Application.Features.Commerce.Promotions.Commands.UpdatePromotion;
using Ecom.Application.Features.Commerce.Promotions.Queries.GetManagementPromotions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Ecom.API.Controllers.V1;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/management/promotions")]
[Authorize]
public sealed class ManagementPromotionsController : BaseController
{
    [HttpGet]
    [Authorize(Policy = Permissions.Promotions.Read)]
    public async Task<IActionResult> GetPromotions([FromQuery] GetManagementPromotionsQuery query, CancellationToken ct) =>
        HandleResult(await Mediator.Send(query, ct));

    [HttpPost]
    [Authorize(Policy = Permissions.Promotions.Manage)]
    [ValidateAntiForgeryToken]
    [EnableRateLimiting(CommerceRateLimitPolicyNames.ManagementMutation)]
    public async Task<IActionResult> Create(CreatePromotionCommand command, CancellationToken ct) =>
        HandleResult(await Mediator.Send(command, ct));

    [HttpPut("{id:guid}")]
    [Authorize(Policy = Permissions.Promotions.Manage)]
    [ValidateAntiForgeryToken]
    [EnableRateLimiting(CommerceRateLimitPolicyNames.ManagementMutation)]
    public async Task<IActionResult> Update(Guid id, UpdatePromotionCommand command, CancellationToken ct) =>
        HandleResult(await Mediator.Send(command with { Id = id }, ct));

    [HttpPatch("{id:guid}/status")]
    [Authorize(Policy = Permissions.Promotions.Manage)]
    [ValidateAntiForgeryToken]
    [EnableRateLimiting(CommerceRateLimitPolicyNames.ManagementMutation)]
    public async Task<IActionResult> ChangeStatus(Guid id, ChangePromotionStatusCommand command, CancellationToken ct) =>
        HandleResult(await Mediator.Send(command with { Id = id }, ct));
}
