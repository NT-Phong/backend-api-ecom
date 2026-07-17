using Ecom.Application.Features.Identity.Commands.AdjustRolePolicy;
using Ecom.Application.Features.Identity.Queries.GetPolicies;
using Ecom.Application.Features.Identity.Queries.GetPoliciesByRole;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;

namespace Ecom.API.Controllers.V1
{

    [ApiVersion("1.0")]
    [Produces("application/json")]
    [Authorize]
    public class IdentityController : BaseController
    {

        /// <summary>
        /// Lấy tất cả các quyền trong hệ thống
        /// </summary>
        [HttpGet("Policies")]
        [Authorize(Policy = Permissions.Roles.Admin)]
        [ProducesResponseType(typeof(ApiResponse<List<PoliciesDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllPolies([FromQuery] GetPoliciesQuery query, CancellationToken cancellationToken)
        {
            var result = await Mediator.Send(query, cancellationToken);
            return result.IsSuccess ? Ok(ApiResponse<object>.Ok(result.Data!)) : HandleResult(result);
        }

        /// <summary>
        /// Lấy tất cả các quyền của một role
        /// </summary>
        [HttpGet("{RoleId:guid}/Policies")]
        [Authorize(Policy = Permissions.RolePolicies.Read)]
        [ProducesResponseType(typeof(ApiResponse<List<PoliciesByRoleDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllPoliesByRole([FromRoute] Guid RoleId, [FromQuery] GetPoliciesByRoleQuery query, CancellationToken cancellationToken)
        {
            query.RoleId = RoleId;
            var result = await Mediator.Send(query, cancellationToken);
            return result.IsSuccess ? Ok(ApiResponse<object>.Ok(result.Data!)) : HandleResult(result);
        }

        [HttpPost("{RoleId:guid}/adjust-role-policy")]
        [Authorize(Policy = Permissions.Roles.Admin)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> PostRole([FromRoute] Guid RoleId, [FromBody] AdjustRolePolicyCommand command, CancellationToken cancellationToken)
        {
            command.RoleId = RoleId;
            var result = await Mediator.Send(command, cancellationToken);
            return result.IsSuccess ? Ok(ApiResponse<object>.Ok(result)) : HandleResult(result);

        }
    }
}


