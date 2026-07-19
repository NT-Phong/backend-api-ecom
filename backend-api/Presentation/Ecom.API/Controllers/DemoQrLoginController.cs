using Asp.Versioning;
using Ecom.Application.Common.Configuration;
using Ecom.Application.Features.Demo.QrLogin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace Ecom.API.Controllers;

[ApiController]
[ApiVersion("1.0")]
[ApiExplorerSettings(GroupName = "v1")]
[Route("api/v{version:apiVersion}/demo/qr-login")]
public sealed class DemoQrLoginController(
    IOptions<DemoQrLoginOptions> options,
    IHostEnvironment environment) : BaseController
{
    private bool IsEnabled => environment.IsDevelopment() && options.Value.Enabled;

    private IActionResult Disabled() =>
        NotFound(ApiResponse<object>.Fail("DemoQrLoginDisabled", ErrorCodes.NOT_FOUND));

    [HttpPost("start")]
    [AllowAnonymous]
    [EnableRateLimiting(AuthRateLimitPolicyNames.DemoQrStartIp)]
    public async Task<IActionResult> Start(CancellationToken cancellationToken)
    {
        if (!IsEnabled) return Disabled();
        return HandleResult(await Mediator.Send(new StartDemoQrLoginCommand(), cancellationToken));
    }

    [HttpGet("{id:guid}/status")]
    [AllowAnonymous]
    [EnableRateLimiting(AuthRateLimitPolicyNames.DemoQrStatusIp)]
    public async Task<IActionResult> Status(Guid id, CancellationToken cancellationToken)
    {
        if (!IsEnabled) return Disabled();
        return HandleResult(await Mediator.Send(new GetDemoQrLoginStatusQuery(id), cancellationToken));
    }

    [HttpPost("{id:guid}/approve")]
    [Authorize]
    [EnableRateLimiting(AuthRateLimitPolicyNames.DemoQrApproveIp)]
    public async Task<IActionResult> Approve(Guid id, CancellationToken cancellationToken)
    {
        if (!IsEnabled) return Disabled();
        return HandleResult(await Mediator.Send(new ApproveDemoQrLoginCommand(id), cancellationToken));
    }

    [HttpPost("{id:guid}/reject")]
    [Authorize]
    [EnableRateLimiting(AuthRateLimitPolicyNames.DemoQrApproveIp)]
    public async Task<IActionResult> Reject(Guid id, CancellationToken cancellationToken)
    {
        if (!IsEnabled) return Disabled();
        return HandleResult(await Mediator.Send(new RejectDemoQrLoginCommand(id), cancellationToken));
    }

    [HttpGet("{id:guid}/approval-page")]
    [AllowAnonymous]
    public IActionResult ApprovalPage(Guid id)
    {
        if (!IsEnabled) return Disabled();
        var idJson = JsonSerializer.Serialize(id);
        var html = """
<!doctype html><html lang="vi"><head><meta charset="utf-8"><meta name="viewport" content="width=device-width, initial-scale=1">
<title>Xác nhận đăng nhập QR Demo</title><style>body{font-family:system-ui;margin:2rem;max-width:28rem}input,button{box-sizing:border-box;width:100%;padding:.75rem;margin:.4rem 0}button{cursor:pointer}#otpForm{display:none}.ok{color:#087f23}.error{color:#b42318}</style></head>
<body><h1>Xác nhận đăng nhập demo</h1><p id="message">Xác thực số điện thoại để xác nhận trên màn hình desktop.</p>
<form id="phoneForm"><input id="phone" inputmode="tel" autocomplete="tel" placeholder="Số điện thoại" required><button>Gửi OTP</button></form>
<form id="otpForm"><input id="otp" inputmode="numeric" autocomplete="one-time-code" placeholder="Mã OTP" required><button>Xác thực OTP</button></form>
<div id="decision" style="display:none"><button id="approve">Xác nhận đăng nhập desktop</button><button id="reject">Từ chối</button></div>
<script>const id=__DEMO_ID__;let phone='';let accessToken='';const message=document.getElementById('message');const phoneForm=document.getElementById('phoneForm');const otpForm=document.getElementById('otpForm');const decision=document.getElementById('decision');
function show(text,kind=''){message.textContent=text;message.className=kind;}
async function call(url,options={}){const response=await fetch(url,options);const body=await response.json().catch(()=>null);if(!response.ok||!body?.success)throw new Error(body?.message||'Yêu cầu không thành công.');return body.data;}
phoneForm.addEventListener('submit',async event=>{event.preventDefault();phone=document.getElementById('phone').value.trim();try{await call('/api/v1/auth/send-otp',{method:'POST',headers:{'Content-Type':'application/json'},body:JSON.stringify({phoneNumber:phone})});phoneForm.style.display='none';otpForm.style.display='block';show('OTP đã được chấp nhận. Nhập mã Development đã được cấu hình.');}catch(error){show(error.message,'error');}});
otpForm.addEventListener('submit',async event=>{event.preventDefault();try{const login=await call('/api/v1/auth/verify-otp',{method:'POST',headers:{'Content-Type':'application/json'},body:JSON.stringify({phoneNumber:phone,otpCode:document.getElementById('otp').value.trim()})});accessToken=login.accessToken;otpForm.style.display='none';decision.style.display='block';show('OTP đã xác thực. Chọn xác nhận hoặc từ chối.');}catch(error){accessToken='';show(error.message,'error');}});
async function decide(action,successMessage){if(!accessToken){show('Hãy xác thực OTP trước.','error');return;}try{await call('/api/v1/demo/qr-login/'+id+'/'+action,{method:'POST',headers:{Authorization:'Bearer '+accessToken}});accessToken='';decision.style.display='none';show(successMessage,'ok');}catch(error){show(error.message,'error');}}
document.getElementById('approve').addEventListener('click',()=>decide('approve','Đã xác nhận. Hãy xem màn hình desktop.'));
document.getElementById('reject').addEventListener('click',()=>decide('reject','Đã từ chối yêu cầu demo.'));</script></body></html>
""";
        return Content(html.Replace("__DEMO_ID__", idJson, StringComparison.Ordinal), "text/html; charset=utf-8");
    }
}
