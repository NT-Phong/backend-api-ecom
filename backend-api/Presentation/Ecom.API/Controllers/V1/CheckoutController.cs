using Ecom.Application.Features.Commerce.Checkout.Queries.PreviewCheckout;
using Ecom.Application.Common.Configuration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Ecom.API.Controllers.V1;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/checkout")]
[AllowAnonymous]
public sealed class CheckoutController : BaseController
{
    [HttpPost("preview")]
    [ValidateAntiForgeryToken]
    [EnableRateLimiting(CommerceRateLimitPolicyNames.CheckoutPreview)]
    public async Task<IActionResult> Preview(PreviewCheckoutQuery query, CancellationToken cancellationToken) => HandleResult(await Mediator.Send(query, cancellationToken));
}
