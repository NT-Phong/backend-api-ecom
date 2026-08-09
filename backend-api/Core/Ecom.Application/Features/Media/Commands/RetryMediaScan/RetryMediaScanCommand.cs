using Ecom.Domain.Entities;

namespace Ecom.Application.Features.Media.Commands.RetryMediaScan;

public sealed record RetryMediaScanCommand(Guid MediaAssetId)
    : IRequest<TResult<MediaMetadataResult>>, ITransactionalRequest;

public sealed class RetryMediaScanCommandValidator : AbstractValidator<RetryMediaScanCommand>
{
    public RetryMediaScanCommandValidator() => RuleFor(x => x.MediaAssetId).NotEmpty();
}

public sealed class RetryMediaScanCommandHandler(IUnitOfWork unitOfWork, IMediaAccessService access)
    : IRequestHandler<RetryMediaScanCommand, TResult<MediaMetadataResult>>
{
    public async Task<TResult<MediaMetadataResult>> Handle(
        RetryMediaScanCommand request,
        CancellationToken cancellationToken)
    {
        var media = await unitOfWork.Repository<MediaAsset>().FindByIdAsync(request.MediaAssetId);
        if (media is null)
            return TResult<MediaMetadataResult>.Failure(MessageKey.ResourceNotFound, ErrorCodes.NOT_FOUND);

        var authorization = access.EnsureOwnerOrManager(media);
        if (!authorization.IsSuccess)
            return TResult<MediaMetadataResult>.Failure(authorization.Error!, authorization.ErrorCode);

        if (media.ScanStatus == MediaScanStatus.Pending)
            return TResult<MediaMetadataResult>.Success(MediaMetadataResults.From(media));
        if (media.ScanStatus != MediaScanStatus.Failed)
            return TResult<MediaMetadataResult>.Failure("MEDIA_SCAN_RETRY_INVALID", ErrorCodes.BAD_REQUEST);

        media.RetryScan();
        await unitOfWork.Repository<MediaAsset>().UpdateAsync(media, cancellationToken);
        return TResult<MediaMetadataResult>.Success(MediaMetadataResults.From(media));
    }
}
