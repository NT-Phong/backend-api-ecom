using Ecom.Application.Features.Commerce.Producers.Commands.CreateManagementProducer;
using Ecom.Application.Features.Commerce.Producers.Commands.CreateProducerContact;
using Ecom.Application.Features.Commerce.Producers.Commands.CreateProductionFacility;
using Ecom.Application.Features.Commerce.Producers.Commands.HideManagementProducer;
using Ecom.Application.Features.Commerce.Producers.Commands.PublishManagementProducer;
using Ecom.Application.Features.Commerce.Producers.Commands.UpdateManagementProducer;
using Ecom.Application.Features.Commerce.Producers.Commands.UpdateProducerContact;
using Ecom.Application.Features.Commerce.Producers.Commands.VerifyManagementProducer;
using Ecom.Application.Features.Commerce.Producers.Queries.GetManagementProducerById;
using Ecom.Application.Features.Commerce.Producers.Queries.GetManagementProducers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Ecom.Application.Common.Configuration;

namespace Ecom.API.Controllers.V1;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/management/producers")]
[Authorize]
public sealed class ManagementProducersController : BaseController
{
    [HttpGet]
    [Authorize(Policy = Permissions.Producers.Read)]
    public async Task<IActionResult> GetList([FromQuery] GetManagementProducersQuery query, CancellationToken ct) =>
        HandleResult(await Mediator.Send(query, ct));

    [HttpGet("{producerId:guid}")]
    [Authorize(Policy = Permissions.Producers.Read)]
    public async Task<IActionResult> GetById(Guid producerId, CancellationToken ct) =>
        HandleResult(await Mediator.Send(new GetManagementProducerByIdQuery(producerId), ct));

    [HttpPost]
    [Authorize(Policy = Permissions.Producers.Create)]
    [ValidateAntiForgeryToken]
    [EnableRateLimiting(CommerceRateLimitPolicyNames.ManagementMutation)]
    public async Task<IActionResult> Create(CreateManagementProducerCommand command, CancellationToken ct) =>
        HandleResult(await Mediator.Send(command, ct));

    [HttpPut("{producerId:guid}")]
    [Authorize(Policy = Permissions.Producers.Update)]
    [ValidateAntiForgeryToken]
    [EnableRateLimiting(CommerceRateLimitPolicyNames.ManagementMutation)]
    public async Task<IActionResult> Update(Guid producerId, UpdateManagementProducerCommand command, CancellationToken ct) =>
        HandleResult(await Mediator.Send(command with { ProducerId = producerId }, ct));

    [HttpPost("{producerId:guid}/verify")]
    [Authorize(Policy = Permissions.Producers.Verify)]
    [ValidateAntiForgeryToken]
    [EnableRateLimiting(CommerceRateLimitPolicyNames.ManagementMutation)]
    public async Task<IActionResult> Verify(Guid producerId, VerifyManagementProducerCommand command, CancellationToken ct) =>
        HandleResult(await Mediator.Send(command with { ProducerId = producerId }, ct));

    [HttpPost("{producerId:guid}/publish")]
    [Authorize(Policy = Permissions.Producers.Publish)]
    [ValidateAntiForgeryToken]
    [EnableRateLimiting(CommerceRateLimitPolicyNames.ManagementMutation)]
    public async Task<IActionResult> Publish(Guid producerId, PublishManagementProducerCommand command, CancellationToken ct) =>
        HandleResult(await Mediator.Send(command with { ProducerId = producerId }, ct));

    [HttpPost("{producerId:guid}/hide")]
    [Authorize(Policy = Permissions.Producers.Publish)]
    [ValidateAntiForgeryToken]
    [EnableRateLimiting(CommerceRateLimitPolicyNames.ManagementMutation)]
    public async Task<IActionResult> Hide(Guid producerId, HideManagementProducerCommand command, CancellationToken ct) =>
        HandleResult(await Mediator.Send(command with { ProducerId = producerId }, ct));

    [HttpPost("{producerId:guid}/contacts")]
    [Authorize(Policy = Permissions.Producers.Update)]
    [ValidateAntiForgeryToken]
    [EnableRateLimiting(CommerceRateLimitPolicyNames.ManagementMutation)]
    public async Task<IActionResult> CreateContact(Guid producerId, CreateProducerContactCommand command, CancellationToken ct) =>
        HandleResult(await Mediator.Send(command with { ProducerId = producerId }, ct));

    [HttpPut("{producerId:guid}/contacts/{contactId:guid}")]
    [Authorize(Policy = Permissions.Producers.Update)]
    [ValidateAntiForgeryToken]
    [EnableRateLimiting(CommerceRateLimitPolicyNames.ManagementMutation)]
    public async Task<IActionResult> UpdateContact(Guid producerId, Guid contactId, UpdateProducerContactCommand command, CancellationToken ct) =>
        HandleResult(await Mediator.Send(command with { ProducerId = producerId, ContactId = contactId }, ct));

    [HttpPost("{producerId:guid}/facilities")]
    [Authorize(Policy = Permissions.Producers.Update)]
    [ValidateAntiForgeryToken]
    [EnableRateLimiting(CommerceRateLimitPolicyNames.ManagementMutation)]
    public async Task<IActionResult> CreateFacility(Guid producerId, CreateProductionFacilityCommand command, CancellationToken ct) =>
        HandleResult(await Mediator.Send(command with { ProducerId = producerId }, ct));
}
