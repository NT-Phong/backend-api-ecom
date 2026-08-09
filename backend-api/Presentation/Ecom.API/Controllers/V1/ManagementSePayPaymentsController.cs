using Ecom.Application.Features.Commerce.Payments.Queries.GetSePayReconciliation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ecom.API.Controllers.V1;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/management/payments/sepay")]
[Authorize]
public sealed class ManagementSePayPaymentsController : BaseController
{
    [HttpGet("reconciliation")]
    [Authorize(Policy = Permissions.Payments.Verify)]
    public async Task<IActionResult> GetReconciliation(CancellationToken cancellationToken) =>
        HandleResult(await Mediator.Send(new GetSePayReconciliationQuery(), cancellationToken));
}
