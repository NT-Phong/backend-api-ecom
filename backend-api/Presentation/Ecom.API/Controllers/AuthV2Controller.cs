using Asp.Versioning;
using Ecom.Application.Features.AuthV2.Register;
using Ecom.Application.Features.AuthV2.Login;
using Ecom.Application.Features.AuthV2.EmailVerification;
using Ecom.Application.Features.AuthV2.PasswordManagement;
using Ecom.Application.Features.AuthV2.Refresh;
using Ecom.Application.Features.AuthV2.Me;
using Ecom.Domain.Enums;
using Ecom.Application.Common.Configuration;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Antiforgery;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Ecom.API.Controllers;
[ApiController,ApiVersion("2.0"),Route("api/v{version:apiVersion}/auth")]
public sealed class AuthV2Controller(IOptions<PasswordAuthenticationV2Options> feature) : BaseController
{
 private IActionResult Disabled()=>NotFound(ApiResponse<object>.Fail("PasswordAuthenticationV2Disabled",ErrorCodes.NOT_FOUND));
 [HttpGet("csrf"),Authorize]
 public IActionResult Csrf([FromServices]IAntiforgery antiforgery) { var tokens=antiforgery.GetAndStoreTokens(HttpContext); return Ok(ApiResponse<object>.Ok(new { token=tokens.RequestToken })); }
 [HttpPost("register"),AllowAnonymous,EnableRateLimiting(AuthRateLimitPolicyNames.RegisterIp)]
 public async Task<IActionResult> Register(RegisterPasswordCommand command,CancellationToken ct)
 { if(!feature.Value.Enabled)return Disabled(); var enriched=command with{IpFingerprint=HttpContext.TraceIdentifier};var result=await Mediator.Send(enriched,ct); return result.IsSuccess?StatusCode(StatusCodes.Status202Accepted,ApiResponse<RegisterPasswordResult>.Ok(result.Data)):HandleResult(result); }

 [HttpPost("email/verify/confirm"),AllowAnonymous]
 public async Task<IActionResult> ConfirmEmail(ConfirmEmailCommand command,CancellationToken ct)
 {if(!feature.Value.Enabled)return Disabled();return HandleResult(await Mediator.Send(command,ct));}

 [HttpPost("login/password"),AllowAnonymous,EnableRateLimiting(AuthRateLimitPolicyNames.PasswordLoginIp)]
 public async Task<IActionResult> Login(PasswordLoginCommand command,CancellationToken ct)
 {
  if(!feature.Value.Enabled)return Disabled();
  var enriched=command with { IpFingerprint=HttpContext.TraceIdentifier, UserAgentSummary=Request.Headers.UserAgent.ToString()[..Math.Min(300,Request.Headers.UserAgent.ToString().Length)] };
  return HandleResult(await Mediator.Send(enriched,ct));
 }

 [HttpPost("password/forgot"),AllowAnonymous]
 public async Task<IActionResult> Forgot(ForgotPasswordCommand command,CancellationToken ct){var result=await Mediator.Send(command,ct);return result.IsSuccess?StatusCode(202,ApiResponse<RegisterPasswordResult>.Ok(result.Data)):HandleResult(result);}
 [HttpPost("password/reset"),AllowAnonymous]
 public async Task<IActionResult> Reset(ResetPasswordCommand command,CancellationToken ct)=>HandleResult(await Mediator.Send(command,ct));
 [HttpPost("password/change"),Authorize,ValidateAntiForgeryToken]
 public async Task<IActionResult> Change(ChangePasswordCommand command,CancellationToken ct)=>HandleResult(await Mediator.Send(command,ct));
 [HttpPost("password/setup"),Authorize,ValidateAntiForgeryToken]
 public async Task<IActionResult> Setup(SetupPasswordCommand command,CancellationToken ct)=>HandleResult(await Mediator.Send(command,ct));
 [HttpPost("refresh"),AllowAnonymous,EnableRateLimiting(AuthRateLimitPolicyNames.RefreshIp)]
 public async Task<IActionResult> Refresh(RefreshSessionCommand command,CancellationToken ct)
 {if(!feature.Value.Enabled)return Disabled();return HandleResult(await Mediator.Send(command,ct));}
 [HttpGet("me"),Authorize]
 public async Task<IActionResult> Me(CancellationToken ct)
 {if(!feature.Value.Enabled)return Disabled();return HandleResult(await Mediator.Send(new GetAuthenticationSessionQuery(),ct));}
}
