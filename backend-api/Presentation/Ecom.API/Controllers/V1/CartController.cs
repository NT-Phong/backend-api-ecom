using Ecom.Application.Features.Commerce.Cart.Commands.AddCartItem;
using Ecom.Application.Features.Commerce.Cart.Commands.ChangeCartItemQuantity;
using Ecom.Application.Features.Commerce.Cart.Commands.RemoveCartItem;
using Ecom.Application.Features.Commerce.Cart.Commands.MergeGuestCart;
using Ecom.Application.Features.Commerce.Cart.Queries.GetCart;
using Ecom.Application.Common.Configuration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Ecom.API.Controllers.V1;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/cart")]
public sealed class CartController : BaseController
{
    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken cancellationToken) => HandleResult(await Mediator.Send(new GetCartQuery(), cancellationToken));

    [HttpPost("items")]
    [ValidateAntiForgeryToken]
    [EnableRateLimiting(CommerceRateLimitPolicyNames.CartMutation)]
    public async Task<IActionResult> AddItem(AddCartItemCommand command, CancellationToken cancellationToken) => HandleResult(await Mediator.Send(command, cancellationToken));

    [HttpPatch("items/{cartItemId:guid}")]
    [ValidateAntiForgeryToken]
    [EnableRateLimiting(CommerceRateLimitPolicyNames.CartMutation)]
    public async Task<IActionResult> ChangeQuantity(Guid cartItemId, ChangeCartItemQuantityCommand command, CancellationToken cancellationToken) =>
        HandleResult(await Mediator.Send(command with { CartItemId = cartItemId }, cancellationToken));

    [HttpDelete("items/{cartItemId:guid}")]
    [ValidateAntiForgeryToken]
    [EnableRateLimiting(CommerceRateLimitPolicyNames.CartMutation)]
    public async Task<IActionResult> Remove(Guid cartItemId, CancellationToken cancellationToken) => HandleResult(await Mediator.Send(new RemoveCartItemCommand(cartItemId), cancellationToken));

    [HttpPost("merge-guest")]
    [Authorize]
    [ValidateAntiForgeryToken]
    [EnableRateLimiting(CommerceRateLimitPolicyNames.CartMutation)]
    public async Task<IActionResult> MergeGuest(CancellationToken cancellationToken) => HandleResult(await Mediator.Send(new MergeGuestCartCommand(), cancellationToken));
}
