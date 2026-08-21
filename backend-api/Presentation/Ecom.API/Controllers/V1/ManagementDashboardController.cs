using Ecom.Application.Features.Commerce.Dashboard.Queries.GetManagementDashboardOverview;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ecom.API.Controllers.V1;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/management/dashboard")]
[Authorize]
public sealed class ManagementDashboardController : BaseController
{
    [HttpGet("overview")]
    [Authorize(Policy = Permissions.Orders.Read)]
    public async Task<IActionResult> GetOverview([FromQuery] GetManagementDashboardOverviewQuery query, CancellationToken ct) =>
        HandleResult(await Mediator.Send(query, ct));
}
