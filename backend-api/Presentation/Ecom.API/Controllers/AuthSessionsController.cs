using Asp.Versioning;
using Ecom.Application.Features.Auth.Commands.Logout;
using Microsoft.AspNetCore.Authorization;
using Ecom.Application.Common.Configuration;
using Microsoft.Extensions.Options;

namespace Ecom.API.Controllers;

[ApiController]
[ApiVersion("2.0")]
[Route("api/v{version:apiVersion}/auth")]
public sealed class AuthSessionsController(IOptions<PasswordAuthenticationV2Options> feature) : BaseController
{
    private IActionResult Disabled()=>NotFound(ApiResponse<object>.Fail("PasswordAuthenticationV2Disabled",ErrorCodes.NOT_FOUND));
    [HttpPost("logout")]
    [AllowAnonymous]
    public async Task<IActionResult> Logout([FromBody] LogoutCommand command, CancellationToken ct)
        => !feature.Value.Enabled ? Disabled() : HandleResult(await Mediator.Send(command with { LogoutAllDevices = false }, ct));

    [HttpPost("logout-all")]
    [Authorize]
    public async Task<IActionResult> LogoutAll([FromBody] LogoutCommand command, CancellationToken ct)
        => !feature.Value.Enabled ? Disabled() : HandleResult(await Mediator.Send(command with { LogoutAllDevices = true }, ct));

    [HttpDelete("sessions/{sessionId:guid}")]
    [Authorize]
    public async Task<IActionResult> RevokeSession(Guid sessionId, CancellationToken ct)
        => HandleResult(await Mediator.Send(new LogoutCommand { SessionId = sessionId }, ct));
}
