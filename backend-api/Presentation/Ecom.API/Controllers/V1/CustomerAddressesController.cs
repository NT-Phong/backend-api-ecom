using Ecom.Application.Features.Commerce.Addresses.Commands.CreateCustomerAddress;
using Ecom.Application.Features.Commerce.Addresses.Commands.DeleteCustomerAddress;
using Ecom.Application.Features.Commerce.Addresses.Commands.SetDefaultCustomerAddress;
using Ecom.Application.Features.Commerce.Addresses.Commands.UpdateCustomerAddress;
using Ecom.Application.Features.Commerce.Addresses.Queries.GetCustomerAddresses;
using Microsoft.AspNetCore.Authorization;

namespace Ecom.API.Controllers.V1;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/customer/addresses")]
[Authorize]
public sealed class CustomerAddressesController : BaseController
{
    [HttpGet] public async Task<IActionResult> Get(CancellationToken ct) => HandleResult(await Mediator.Send(new GetCustomerAddressesQuery(), ct));
    [HttpPost] public async Task<IActionResult> Create(CreateCustomerAddressCommand command, CancellationToken ct) => HandleResult(await Mediator.Send(command, ct));
    [HttpPut("{addressId:guid}")] public async Task<IActionResult> Update(Guid addressId, UpdateCustomerAddressCommand command, CancellationToken ct) => HandleResult(await Mediator.Send(command with { AddressId = addressId }, ct));
    [HttpPost("{addressId:guid}/default")] public async Task<IActionResult> SetDefault(Guid addressId, CancellationToken ct) => HandleResult(await Mediator.Send(new SetDefaultCustomerAddressCommand(addressId), ct));
    [HttpDelete("{addressId:guid}")] public async Task<IActionResult> Delete(Guid addressId, CancellationToken ct) => HandleResult(await Mediator.Send(new DeleteCustomerAddressCommand(addressId), ct));
}
