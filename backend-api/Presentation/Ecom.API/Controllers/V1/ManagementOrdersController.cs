using Ecom.Application.Features.Commerce.Orders.Commands.CancelOrder;
using Ecom.Application.Features.Commerce.Orders.Commands.ConfirmOrder;
using Ecom.Application.Features.Commerce.Payments.Commands.VerifyBankTransfer;
using Ecom.Application.Features.Commerce.Payments.Commands.RefundPayment;
using Ecom.Application.Features.Commerce.Shipments.Commands.PrepareShipment;
using Ecom.Application.Features.Commerce.Shipments.Commands.StartShipment;
using Ecom.Application.Features.Commerce.Shipments.Commands.CompleteShipment;
using Ecom.Application.Features.Commerce.Shipments.Commands.MarkDeliveryFailed;
using Ecom.Application.Features.Commerce.Orders.Commands.AddManagementOrderNote;
using Ecom.Application.Features.Commerce.Orders.Queries.GetManagementOrderById;
using Ecom.Application.Features.Commerce.Orders.Queries.GetManagementOrders;
using Ecom.Application.Common.Configuration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Ecom.API.Controllers.V1;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/management/orders")]
[Authorize]
public sealed class ManagementOrdersController : BaseController
{
    [HttpGet]
    [Authorize(Policy = Permissions.Orders.Manage)]
    public async Task<IActionResult> GetOrders([FromQuery] GetManagementOrdersQuery query, CancellationToken ct) => HandleResult(await Mediator.Send(query, ct));

    [HttpGet("{orderId:guid}")]
    [Authorize(Policy = Permissions.Orders.Manage)]
    public async Task<IActionResult> GetOrder(Guid orderId, CancellationToken ct) => HandleResult(await Mediator.Send(new GetManagementOrderByIdQuery(orderId), ct));

    [HttpPost("{orderId:guid}/confirm")]
    [Authorize(Policy = Permissions.Orders.Manage)]
    [ValidateAntiForgeryToken]
    [EnableRateLimiting(CommerceRateLimitPolicyNames.ManagementMutation)]
    public async Task<IActionResult> Confirm(Guid orderId, CancellationToken ct) => HandleResult(await Mediator.Send(new ConfirmOrderCommand(orderId), ct));

    [HttpPost("{orderId:guid}/cancel")]
    [Authorize(Policy = Permissions.Orders.Manage)]
    [ValidateAntiForgeryToken]
    [EnableRateLimiting(CommerceRateLimitPolicyNames.ManagementMutation)]
    public async Task<IActionResult> Cancel(Guid orderId, CancelOrderCommand command, CancellationToken ct) => HandleResult(await Mediator.Send(command with { OrderId = orderId }, ct));

    [HttpPost("{orderId:guid}/payment/verify-bank-transfer")]
    [Authorize(Policy = Permissions.Payments.Verify)]
    [ValidateAntiForgeryToken]
    [EnableRateLimiting(CommerceRateLimitPolicyNames.ManagementMutation)]
    public async Task<IActionResult> VerifyBankTransfer(Guid orderId, VerifyBankTransferCommand command, CancellationToken ct) => HandleResult(await Mediator.Send(command with { OrderId = orderId }, ct));

    [HttpPost("{orderId:guid}/payment/refund")]
    [Authorize(Policy = Permissions.Payments.Refund)]
    [ValidateAntiForgeryToken]
    [EnableRateLimiting(CommerceRateLimitPolicyNames.ManagementMutation)]
    public async Task<IActionResult> Refund(Guid orderId, RefundPaymentCommand command, CancellationToken ct) =>
        HandleResult(await Mediator.Send(command with { OrderId = orderId }, ct));

    [HttpPost("{orderId:guid}/shipment/prepare")]
    [Authorize(Policy = Permissions.Shipments.Manage)]
    [ValidateAntiForgeryToken]
    [EnableRateLimiting(CommerceRateLimitPolicyNames.ManagementMutation)]
    public async Task<IActionResult> PrepareShipment(Guid orderId, CancellationToken ct) => HandleResult(await Mediator.Send(new PrepareShipmentCommand(orderId), ct));

    [HttpPost("{orderId:guid}/shipment/start")]
    [Authorize(Policy = Permissions.Shipments.Manage)]
    [ValidateAntiForgeryToken]
    [EnableRateLimiting(CommerceRateLimitPolicyNames.ManagementMutation)]
    public async Task<IActionResult> StartShipment(Guid orderId, StartShipmentCommand command, CancellationToken ct) => HandleResult(await Mediator.Send(command with { OrderId = orderId }, ct));

    [HttpPost("{orderId:guid}/shipment/complete")]
    [Authorize(Policy = Permissions.Shipments.Manage)]
    [ValidateAntiForgeryToken]
    [EnableRateLimiting(CommerceRateLimitPolicyNames.ManagementMutation)]
    public async Task<IActionResult> CompleteShipment(Guid orderId, CancellationToken ct) => HandleResult(await Mediator.Send(new CompleteShipmentCommand(orderId), ct));

    [HttpPost("{orderId:guid}/shipment/delivery-failed")]
    [Authorize(Policy = Permissions.Shipments.Manage)]
    [ValidateAntiForgeryToken]
    [EnableRateLimiting(CommerceRateLimitPolicyNames.ManagementMutation)]
    public async Task<IActionResult> MarkDeliveryFailed(Guid orderId, MarkDeliveryFailedCommand command, CancellationToken ct) =>
        HandleResult(await Mediator.Send(command with { OrderId = orderId }, ct));

    [HttpPost("{orderId:guid}/notes")]
    [Authorize(Policy = Permissions.Orders.Manage)]
    [ValidateAntiForgeryToken]
    [EnableRateLimiting(CommerceRateLimitPolicyNames.ManagementMutation)]
    public async Task<IActionResult> AddNote(Guid orderId, AddManagementOrderNoteCommand command, CancellationToken ct) =>
        HandleResult(await Mediator.Send(command with { OrderId = orderId }, ct));
}
