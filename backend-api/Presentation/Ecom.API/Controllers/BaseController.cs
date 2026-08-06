using Microsoft.AspNetCore.Http;

namespace Ecom.API.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
//TODO: route config 
[Produces("application/json")]
public abstract class BaseController : ControllerBase
{
    protected ISender Mediator => field ??= HttpContext.RequestServices.GetRequiredService<ISender>();
    
    // HandleResult methods
    protected IActionResult HandleResult<T>(TResult<T> result)
    {
        if (result.IsSuccess && result.Data != null)
            return Ok(ApiResponse<T>.Ok(result.Data));

        if (result.ValidationErrors != null && result.ValidationErrors.Any())
            return BadRequest(ApiResponse<T>.Fail("Validation failed", ErrorCodes.BAD_REQUEST, result.ValidationErrors));

        return result.ErrorCode switch
        {
            ErrorCodes.NOT_FOUND => NotFound(ApiResponse<T>.Fail(result.Error!, result.ErrorCode)),
            ErrorCodes.UNAUTHORIZED => Unauthorized(ApiResponse<T>.Fail(result.Error!, result.ErrorCode)),
            ErrorCodes.FORBIDDEN => StatusCode(StatusCodes.Status403Forbidden, ApiResponse<T>.Fail(result.Error!, result.ErrorCode)),
            ErrorCodes.ALREADY_EXISTS => Conflict(ApiResponse<T>.Fail(result.Error!, result.ErrorCode)),
            ErrorCodes.UNPROCESSABLE_ENTITY => StatusCode(StatusCodes.Status422UnprocessableEntity, ApiResponse<T>.Fail(result.Error!, result.ErrorCode)),
            ErrorCodes.TOO_MANY_REQUESTS => StatusCode(StatusCodes.Status429TooManyRequests, ApiResponse<T>.Fail(result.Error!, result.ErrorCode)),
            ErrorCodes.EXTERNAL_SERVICE_ERROR => StatusCode(StatusCodes.Status502BadGateway, ApiResponse<T>.Fail(MessageKey.AuthDependencyUnavailable, result.ErrorCode)),
            ErrorCodes.SERVICE_UNAVAILABLE => StatusCode(StatusCodes.Status503ServiceUnavailable, ApiResponse<T>.Fail(MessageKey.AuthDependencyUnavailable, result.ErrorCode)),
            ErrorCodes.INTERNAL_SERVER_ERROR or ErrorCodes.SERVER_ERROR => StatusCode(StatusCodes.Status500InternalServerError, ApiResponse<T>.Fail("An internal server error has occurred.", result.ErrorCode)),
            _ => BadRequest(ApiResponse<T>.Fail(result.Error!, result.ErrorCode))
        };
    }

    protected IActionResult HandleResult(TResult result)
    {
        if (result.IsSuccess)
            return Ok(ApiResponse<object>.Ok(null, "Operation completed successfully"));

        return HandleResult(TResult<object>.Failure(result.Error!, result.ErrorCode));
    }
}

