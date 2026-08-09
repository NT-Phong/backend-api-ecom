using Ecom.Application.Common.Configuration;
using Ecom.Application.Features.Commerce.Payments.Commands.ProcessSePayBankWebhook;
using Ecom.Application.Features.Commerce.Payments.Commands.ProcessSePayIpn;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Ecom.API.Controllers.V1;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/payments/sepay")]
[AllowAnonymous]
public sealed class SePayPaymentsController : BaseController
{
    [HttpPost("ipn")]
    [Consumes("application/json")]
    [RequestSizeLimit(16 * 1024)]
    [EnableRateLimiting(CommerceRateLimitPolicyNames.PaymentIpn)]
    public async Task<IActionResult> ProcessIpn([FromHeader(Name = "X-Secret-Key")] string? secret,
        [FromBody] SePayIpnPayload payload, CancellationToken cancellationToken) =>
        HandleResult(await Mediator.Send(new ProcessSePayIpnCommand(secret, payload), cancellationToken));
}

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/payments/sepay-bank")]
[AllowAnonymous]
public sealed class SePayBankPaymentsController : BaseController
{
    [HttpPost("webhook")]
    [Consumes("application/json")]
    [RequestSizeLimit(16 * 1024)]
    [EnableRateLimiting(CommerceRateLimitPolicyNames.PaymentBankWebhook)]
    public async Task<IActionResult> ProcessWebhook([FromHeader(Name = "X-SePay-Timestamp")] string? timestamp,
        [FromHeader(Name = "X-SePay-Signature")] string? signature, CancellationToken cancellationToken)
    {
        Request.EnableBuffering();
        using var reader = new StreamReader(Request.Body, leaveOpen: true);
        var rawBody = await reader.ReadToEndAsync(cancellationToken);
        return HandleResult(await Mediator.Send(new ProcessSePayBankWebhookCommand(timestamp, rawBody, signature), cancellationToken));
    }
}
