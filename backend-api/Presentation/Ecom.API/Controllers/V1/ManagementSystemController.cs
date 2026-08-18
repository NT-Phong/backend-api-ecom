using Ecom.Application.Common.Configuration;
using Ecom.Application.Features.Commerce.System.Commands.RevokeManagementSession;
using Ecom.Application.Features.Commerce.System.Commands.UpsertSystemSetting;
using Ecom.Application.Features.Commerce.System.Queries.GetManagementAuditLogs;
using Ecom.Application.Features.Commerce.System.Queries.GetManagementSecurity;
using Ecom.Application.Features.Commerce.System.Queries.GetManagementSystemSettings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Ecom.API.Controllers.V1;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/management")]
[Authorize]
public sealed class ManagementSystemController:BaseController
{
 [HttpGet("settings")][Authorize(Policy=Permissions.Settings.Read)] public async Task<IActionResult> GetSettings(CancellationToken ct)=>HandleResult(await Mediator.Send(new GetManagementSystemSettingsQuery(),ct));
 [HttpPut("settings")][Authorize(Policy=Permissions.Settings.Update)][ValidateAntiForgeryToken][EnableRateLimiting(CommerceRateLimitPolicyNames.ManagementMutation)] public async Task<IActionResult> UpsertSetting(UpsertSystemSettingCommand command,CancellationToken ct)=>HandleResult(await Mediator.Send(command,ct));
 [HttpGet("audit-logs")][Authorize(Policy=Permissions.Audit.Read)] public async Task<IActionResult> GetAuditLogs([FromQuery]GetManagementAuditLogsQuery query,CancellationToken ct)=>HandleResult(await Mediator.Send(query,ct));
 [HttpGet("security/sessions")][Authorize(Policy=Permissions.SecuritySessions.Read)] public async Task<IActionResult> GetSessions([FromQuery]GetManagementUserSessionsQuery query,CancellationToken ct)=>HandleResult(await Mediator.Send(query,ct));
 [HttpPost("security/sessions/{sessionId:guid}/revoke")][Authorize(Policy=Permissions.SecuritySessions.Revoke)][ValidateAntiForgeryToken][EnableRateLimiting(CommerceRateLimitPolicyNames.ManagementMutation)] public async Task<IActionResult> RevokeSession(Guid sessionId,RevokeManagementSessionCommand command,CancellationToken ct)=>HandleResult(await Mediator.Send(command with{SessionId=sessionId},ct));
 [HttpGet("security/events")][Authorize(Policy=Permissions.SecurityEvents.Read)] public async Task<IActionResult> GetEvents([FromQuery]GetManagementSecurityEventsQuery query,CancellationToken ct)=>HandleResult(await Mediator.Send(query,ct));
}
