using Ecom.Application.Common.Services;
using Ecom.Application.Common.Configuration;
using Ecom.Application.Features.Media.Commands.DeleteMedia;
using Ecom.Application.Features.Media.Commands.RetryMediaScan;
using Ecom.Application.Features.Media.Queries.GetMediaMetadata;
using Ecom.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.RateLimiting;

namespace Ecom.API.Controllers.V1;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/media")]
[Authorize]
public sealed class MediaController(MediaUploadOrchestrator uploads) : BaseController
{
    [HttpPost]
    [Authorize(Policy = Permissions.Media.Upload)]
    [RequestSizeLimit(10 * 1024 * 1024)]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Upload([FromForm] UploadMediaForm form, CancellationToken cancellationToken)
    {
        if (form.File is null || form.File.Length == 0)
            return BadRequest(ApiResponse<object>.Fail("A file is required.", ErrorCodes.BAD_REQUEST));
        if (form.Intent != MediaUploadIntent.ProductImage)
            return BadRequest(ApiResponse<object>.Fail("Only ProductImage is supported in V1.", ErrorCodes.BAD_REQUEST));

        await using var stream = form.File.OpenReadStream();
        var result = await uploads.UploadAsync(stream, form.File.FileName, form.File.ContentType, form.File.Length,
            form.Intent, form.AltText, cancellationToken);
        return result.IsSuccess
            ? StatusCode(StatusCodes.Status201Created, ApiResponse<MediaAssetResult>.Ok(result.Data))
            : HandleResult(result);
    }

    [HttpGet("{mediaAssetId:guid}")]
    [Authorize(Policy = Permissions.Media.Read)]
    public async Task<IActionResult> Get(Guid mediaAssetId, CancellationToken cancellationToken) =>
        HandleResult(await Mediator.Send(new GetMediaMetadataQuery(mediaAssetId), cancellationToken));

    [HttpPost("{mediaAssetId:guid}/retry-scan")]
    [Authorize(Policy = Permissions.Media.Read)]
    [ValidateAntiForgeryToken]
    [EnableRateLimiting(CommerceRateLimitPolicyNames.ManagementMutation)]
    public async Task<IActionResult> RetryScan(Guid mediaAssetId, CancellationToken cancellationToken) =>
        HandleResult(await Mediator.Send(new RetryMediaScanCommand(mediaAssetId), cancellationToken));

    [HttpDelete("{mediaAssetId:guid}")]
    [Authorize(Policy = Permissions.Media.Delete)]
    public async Task<IActionResult> Delete(Guid mediaAssetId, CancellationToken cancellationToken) =>
        HandleResult(await Mediator.Send(new DeleteMediaCommand(mediaAssetId), cancellationToken));
}

public sealed class UploadMediaForm
{
    public IFormFile? File { get; init; }
    public MediaUploadIntent Intent { get; init; }
    public string? AltText { get; init; }
}
