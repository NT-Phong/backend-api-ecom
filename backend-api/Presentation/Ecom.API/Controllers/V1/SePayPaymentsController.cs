using Ecom.Application.Common.Configuration;
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
