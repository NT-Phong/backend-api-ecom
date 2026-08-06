using Ecom.Application.Features.Commerce.Checkout.Queries.PreviewCheckout;
using Microsoft.AspNetCore.Authorization;

namespace Ecom.API.Controllers.V1;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/checkout")]
[AllowAnonymous]
public sealed class CheckoutController : BaseController
{
    [HttpPost("preview")]
    public async Task<IActionResult> Preview(PreviewCheckoutQuery query, CancellationToken cancellationToken) => HandleResult(await Mediator.Send(query, cancellationToken));
}
