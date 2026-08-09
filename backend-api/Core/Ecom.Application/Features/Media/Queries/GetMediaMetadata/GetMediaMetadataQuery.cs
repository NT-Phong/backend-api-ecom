using Ecom.Domain.Entities;

namespace Ecom.Application.Features.Media.Queries.GetMediaMetadata;

public sealed record GetMediaMetadataQuery(Guid MediaAssetId) : IRequest<TResult<MediaMetadataResult>>;

public sealed class GetMediaMetadataQueryHandler(IUnitOfWork unitOfWork, IMediaAccessService access)
    : IRequestHandler<GetMediaMetadataQuery, TResult<MediaMetadataResult>>
{
    public async Task<TResult<MediaMetadataResult>> Handle(GetMediaMetadataQuery request, CancellationToken cancellationToken)
    {
        var media = await unitOfWork.Repository<MediaAsset>().FindByIdAsync(request.MediaAssetId);
        if (media is null) return TResult<MediaMetadataResult>.Failure(MessageKey.ResourceNotFound, ErrorCodes.NOT_FOUND);
        var authorization = access.EnsureOwnerOrManager(media);
        if (!authorization.IsSuccess) return TResult<MediaMetadataResult>.Failure(authorization.Error!, authorization.ErrorCode);
        return TResult<MediaMetadataResult>.Success(MediaMetadataResults.From(media));
    }
}
