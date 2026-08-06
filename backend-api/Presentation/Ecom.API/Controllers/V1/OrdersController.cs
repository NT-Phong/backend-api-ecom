using Ecom.Application.Features.Commerce.Orders.Commands.CreateOrder;
using Ecom.Application.Features.Commerce.Orders.Commands.CancelOrder;
using Ecom.Application.Features.Commerce.Orders.Queries.GetOrder;
using Ecom.Application.Features.Commerce.Orders.Queries.GetOrders;
using Microsoft.AspNetCore.Authorization;

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
    public async Task<IActionResult> Create(CreateOrderCommand command, [FromHeader(Name = "Idempotency-Key")] string? idempotencyKey,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(idempotencyKey))
            return BadRequest(ApiResponse<object>.Fail("Idempotency-Key header is required.", ErrorCodes.BAD_REQUEST));
        return HandleResult(await Mediator.Send(command with { IdempotencyKey = idempotencyKey }, cancellationToken));
    }

    [HttpPost("{orderId:guid}/cancel")]
    [Authorize]
    public async Task<IActionResult> Cancel(Guid orderId, CancelOrderCommand command, CancellationToken cancellationToken) =>
        HandleResult(await Mediator.Send(command with { OrderId = orderId }, cancellationToken));
}
