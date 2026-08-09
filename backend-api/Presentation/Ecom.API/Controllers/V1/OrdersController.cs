using Ecom.Application.Features.Commerce.Orders.Commands.CreateOrder;
using Ecom.Application.Features.Commerce.Orders.Commands.CancelOrder;
using Ecom.Application.Features.Commerce.Orders.Queries.GetOrder;
using Ecom.Application.Features.Commerce.Orders.Queries.GetOrders;
using Ecom.Application.Features.Commerce.Payments.Commands.CreateSePayCheckout;
using Ecom.Application.Common.Configuration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Ecom.API.Controllers.V1;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/orders")]
public sealed class OrdersController : BaseController
{
    [HttpGet]
    public async Task<IActionResult> GetOrders(CancellationToken cancellationToken) =>
        HandleResult(await Mediator.Send(new GetOrdersQuery(), cancellationToken));

    [HttpGet("{orderId:guid}")]
    public async Task<IActionResult> GetOrder(Guid orderId, CancellationToken cancellationToken) =>
        HandleResult(await Mediator.Send(new GetOrderQuery(orderId), cancellationToken));

    [HttpPost]
    [ValidateAntiForgeryToken]
    [EnableRateLimiting(CommerceRateLimitPolicyNames.OrderCreate)]
    public async Task<IActionResult> Create(CreateOrderCommand command, [FromHeader(Name = "Idempotency-Key")] string? idempotencyKey,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(idempotencyKey))
            return BadRequest(ApiResponse<object>.Fail("Idempotency-Key header is required.", ErrorCodes.BAD_REQUEST));
        return HandleResult(await Mediator.Send(command with { IdempotencyKey = idempotencyKey }, cancellationToken));
    }

    [HttpPost("{orderId:guid}/payments/sepay/checkout")]
    [ValidateAntiForgeryToken]
    [EnableRateLimiting(CommerceRateLimitPolicyNames.PaymentCheckout)]
    public async Task<IActionResult> CreateSePayCheckout(Guid orderId, CancellationToken cancellationToken) =>
        HandleResult(await Mediator.Send(new CreateSePayCheckoutCommand(orderId), cancellationToken));

    [HttpPost("{orderId:guid}/cancel")]
    [ValidateAntiForgeryToken]
    [EnableRateLimiting(CommerceRateLimitPolicyNames.CartMutation)]
    public async Task<IActionResult> Cancel(Guid orderId, CancelOrderCommand command, CancellationToken cancellationToken) =>
        HandleResult(await Mediator.Send(command with { OrderId = orderId }, cancellationToken));
}
