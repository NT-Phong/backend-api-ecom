using Ecom.Application.Common.Configuration;
using Ecom.Application.Features.Commerce.Inventory.Commands.AdjustInventoryLevel;
using Ecom.Application.Features.Commerce.Inventory.Commands.CreateStockLocation;
using Ecom.Application.Features.Commerce.Inventory.Commands.InitializeInventoryLevel;
using Ecom.Application.Features.Commerce.Inventory.Commands.UpdateStockLocation;
using Ecom.Application.Features.Commerce.Inventory.Queries.GetManagementInventoryLevels;
using Ecom.Application.Features.Commerce.Inventory.Queries.GetManagementInventoryMovements;
using Ecom.Application.Features.Commerce.Inventory.Queries.GetStockLocations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Ecom.API.Controllers.V1;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/management/inventory")]
[Authorize]
public sealed class ManagementInventoryController : BaseController
{
    [HttpGet("levels")]
    [Authorize(Policy = Permissions.Inventory.Read)]
    public async Task<IActionResult> GetLevels([FromQuery] GetManagementInventoryLevelsQuery query, CancellationToken ct) => HandleResult(await Mediator.Send(query, ct));

    [HttpGet("movements")]
    [Authorize(Policy = Permissions.Inventory.Read)]
    public async Task<IActionResult> GetMovements([FromQuery] GetManagementInventoryMovementsQuery query, CancellationToken ct) => HandleResult(await Mediator.Send(query, ct));

    [HttpGet("locations")]
    [Authorize(Policy = Permissions.Inventory.Read)]
    public async Task<IActionResult> GetLocations([FromQuery] bool? isActive, CancellationToken ct) => HandleResult(await Mediator.Send(new GetStockLocationsQuery(isActive), ct));

    [HttpPost("locations")]
    [Authorize(Policy = Permissions.Inventory.LocationsManage)]
    [ValidateAntiForgeryToken]
    [EnableRateLimiting(CommerceRateLimitPolicyNames.ManagementMutation)]
    public async Task<IActionResult> CreateLocation(CreateStockLocationCommand command, CancellationToken ct) => HandleResult(await Mediator.Send(command, ct));

    [HttpPut("locations/{stockLocationId:guid}")]
    [Authorize(Policy = Permissions.Inventory.LocationsManage)]
    [ValidateAntiForgeryToken]
    [EnableRateLimiting(CommerceRateLimitPolicyNames.ManagementMutation)]
    public async Task<IActionResult> UpdateLocation(Guid stockLocationId, UpdateStockLocationCommand command, CancellationToken ct) => HandleResult(await Mediator.Send(command with { StockLocationId = stockLocationId }, ct));

    [HttpPost("levels")]
    [Authorize(Policy = Permissions.Inventory.Adjust)]
    [ValidateAntiForgeryToken]
    [EnableRateLimiting(CommerceRateLimitPolicyNames.ManagementMutation)]
    public async Task<IActionResult> InitializeLevel(InitializeInventoryLevelCommand command, CancellationToken ct) => HandleResult(await Mediator.Send(command, ct));

    [HttpPost("levels/adjustments")]
    [Authorize(Policy = Permissions.Inventory.Adjust)]
    [ValidateAntiForgeryToken]
    [EnableRateLimiting(CommerceRateLimitPolicyNames.ManagementMutation)]
    public async Task<IActionResult> Adjust(AdjustInventoryLevelCommand command, CancellationToken ct) => HandleResult(await Mediator.Send(command, ct));
}
